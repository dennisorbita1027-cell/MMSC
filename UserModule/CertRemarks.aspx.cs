using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CASApp1.UserModule
{
    public class CertRemarks : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;

        protected Label lblMessage;
        protected Repeater rptSim;
        protected Repeater rptStat;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
            }
        }

        protected async Task LoadRemarksDataAsync()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string studentType = Session["StudentType"]?.ToString();

            const string query = @"
                SELECT 
                    R.ReturnID,
                    R.SubmissionID,
                    R.ModuleType,
                    R.DateSent,
                    R.SimilarityFilePath,
                    R.AIFilePath,
                    R.MMSCReportFilePath,
                    R.MMSCCertFilePath,
                    R.StatComCertPath,
                    R.MessageToStudent
                FROM ProcessorReturnedReports R
                WHERE R.UserID = @UserID
                ORDER BY R.DateSent DESC";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();

                        // Open connection asynchronously to minimize pool blockages
                        await connection.OpenAsync();
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            DataView dataView = new DataView(dataTable);

                            switch (studentType)
                            {
                                case "Similarity":
                                    dataView.RowFilter = "ModuleType = 'Similarity & AI Writing'";
                                    rptSim.DataSource = dataView.ToTable();
                                    rptSim.DataBind();
                                    rptSim.Visible = true;
                                    rptStat.Visible = false;
                                    break;

                                case "StatVal":
                                    dataView.RowFilter = "ModuleType = 'StatComVal'";
                                    rptStat.DataSource = dataView.ToTable();
                                    rptStat.DataBind();
                                    rptStat.Visible = true;
                                    rptSim.Visible = false;
                                    break;

                                default:
                                    // Fallback if StudentType session does not match expected criteria
                                    rptSim.Visible = false;
                                    rptStat.Visible = false;
                                    break;
                            }
                        }
                        else
                        {
                            lblMessage.Text = "No records found.";
                            lblMessage.Visible = true;
                            rptSim.Visible = false;
                            rptStat.Visible = false;
                        }
                    }
                }
            }
        }
    }
}