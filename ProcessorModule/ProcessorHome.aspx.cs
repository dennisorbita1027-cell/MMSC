using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CASApp1.ProcessorModule
{
    public class ProcessorHome : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;

        protected HiddenField hfTotalStudents;
        protected HiddenField hfCompleted;
        protected HiddenField hfPending;
        protected HiddenField hfSimSubmitted;
        protected HiddenField hfSimCompleted;
        protected HiddenField hfSimPending;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(LoadChartDataAsync));
            }
        }

        private async Task LoadChartDataAsync()
        {
            const string queryTotalStudents = "SELECT COUNT(*) FROM Users WHERE Role = 'Student'";

            const string queryStatCompleted = @"
                SELECT COUNT(*) 
                FROM ProcessorReturnedReports
                WHERE StatComCertPath IS NOT NULL";

            const string queryStatPending = @"
                SELECT COUNT(*)
                FROM StatComVal_Submission s
                WHERE s.RawDataPath IS NOT NULL
                  AND s.ProposalCopyPath IS NOT NULL
                  AND s.StatOutputPath IS NOT NULL
                  AND s.DeclarationPath IS NOT NULL
                  AND s.ReceiptPath IS NOT NULL
                  AND s.SubmissionID NOT IN (
                      SELECT SubmissionID
                      FROM ProcessorReturnedReports
                      WHERE StatComCertPath IS NOT NULL
                  )";

            const string querySimSubmitted = @"
                SELECT COUNT(*)
                FROM SimilarityAICheck_Submission
                WHERE RevisionNo IS NOT NULL";

            const string querySimCompleted = @"
                SELECT COUNT(*)
                FROM ProcessorReturnedReports
                WHERE ModuleType = 'Similarity & AI Writing'
                  AND MMSCCertFilePath IS NOT NULL";

            const string querySimPending = @"
                SELECT COUNT(*)
                FROM SimilarityAICheck_Submission s
                LEFT JOIN ProcessorReturnedReports r
                    ON s.SubmissionID = r.SubmissionID
                   AND r.ModuleType = 'Similarity & AI Writing'
                   AND r.MMSCCertFilePath IS NOT NULL
                WHERE s.RevisionNo IS NOT NULL
                  AND r.SubmissionID IS NULL";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                int totalStudents = await GetScalarAsync(queryTotalStudents, connection);
                int statCompleted = await GetScalarAsync(queryStatCompleted, connection);
                int statPending = await GetScalarAsync(queryStatPending, connection);
                int simSubmitted = await GetScalarAsync(querySimSubmitted, connection);
                int simCompleted = await GetScalarAsync(querySimCompleted, connection);
                int simPending = await GetScalarAsync(querySimPending, connection);

                hfTotalStudents.Value = totalStudents.ToString();
                hfCompleted.Value = statCompleted.ToString();
                hfPending.Value = statPending.ToString();
                hfSimSubmitted.Value = simSubmitted.ToString();
                hfSimCompleted.Value = simCompleted.ToString();
                hfSimPending.Value = simPending.ToString();
            }
        }

        private async Task<int> GetScalarAsync(string query, SqlConnection connection)
        {
            using (var command = new SqlCommand(query, connection))
            {
                object result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }
}