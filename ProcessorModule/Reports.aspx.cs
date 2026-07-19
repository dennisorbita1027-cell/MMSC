using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CASApp1.ProcessorModule
{
    public class Reports : Page
    {
        private static readonly string ConnStr = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;

        protected Button btnStatCom;
        protected Button btnSimilarity;
        protected TextBox txtSearch;
        protected Button btnSearch;
        protected Button btnReset;
        protected Label lblNoResults;
        protected GridView gvStatComReports;
        protected GridView gvSimilarityReports;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx");
                return;
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                Session["ActiveModule"] = "StatCom";
                RegisterAsyncTask(new PageAsyncTask(() => LoadReportsAsync()));
            }
        }

        protected void btnStatCom_Click(object sender, EventArgs e)
        {
            Session["ActiveModule"] = "StatCom";
            SetActiveTab();
            RegisterAsyncTask(new PageAsyncTask(() => LoadReportsAsync()));
        }

        protected void btnSimilarity_Click(object sender, EventArgs e)
        {
            Session["ActiveModule"] = "Similarity";
            SetActiveTab();
            RegisterAsyncTask(new PageAsyncTask(() => LoadReportsAsync()));
        }

        private void SetActiveTab()
        {
            string activeModule = Session["ActiveModule"]?.ToString();
            btnStatCom.CssClass = "module-tab";
            btnSimilarity.CssClass = "module-tab";

            if (activeModule == "StatCom")
            {
                btnStatCom.CssClass += " active-tab";
            }
            else
            {
                btnSimilarity.CssClass += " active-tab";
            }
        }

        private async Task LoadReportsAsync(string searchQuery = "")
        {
            string activeModule = Session["ActiveModule"]?.ToString();
            lblNoResults.Visible = false;

            if (activeModule == "StatCom")
            {
                DataTable statComReports = await GetStatComReportsAsync(searchQuery);
                gvStatComReports.DataSource = statComReports;
                gvStatComReports.DataBind();

                gvStatComReports.Visible = statComReports.Rows.Count > 0;
                gvSimilarityReports.Visible = false;
                lblNoResults.Visible = statComReports.Rows.Count == 0;
            }
            else
            {
                DataTable similarityReports = await GetSimilarityReportsAsync(searchQuery);
                gvSimilarityReports.DataSource = similarityReports;
                gvSimilarityReports.DataBind();

                gvSimilarityReports.Visible = similarityReports.Rows.Count > 0;
                gvStatComReports.Visible = false;
                lblNoResults.Visible = similarityReports.Rows.Count == 0;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();
            RegisterAsyncTask(new PageAsyncTask(() => LoadReportsAsync(query)));
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            RegisterAsyncTask(new PageAsyncTask(() => LoadReportsAsync()));
        }

        protected void gvReports_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (sender is GridView gv)
            {
                gv.PageIndex = e.NewPageIndex;
            }
            string query = txtSearch.Text.Trim();
            RegisterAsyncTask(new PageAsyncTask(() => LoadReportsAsync(query)));
        }

        protected void gvReports_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string returnId = e.CommandArgument.ToString();
            string activeModule = Session["ActiveModule"]?.ToString();

            if (e.CommandName == "DownloadStatCom" && activeModule == "StatCom")
            {
                RegisterAsyncTask(new PageAsyncTask(() => ProcessStatComDownloadAsync(returnId)));
            }
            else if (e.CommandName == "DownloadSimilarity" && activeModule == "Similarity")
            {
                RegisterAsyncTask(new PageAsyncTask(() => ProcessSimilarityDownloadAsync(returnId)));
            }
        }

        private async Task ProcessStatComDownloadAsync(string returnId)
        {
            string relativePath = null;

            using (var connection = new SqlConnection(ConnStr))
            {
                const string query = "SELECT StatComCertPath FROM ProcessorReturnedReports WHERE ReturnID = @ReturnID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReturnID", returnId);
                    await connection.OpenAsync();
                    relativePath = (await command.ExecuteScalarAsync())?.ToString();
                }
            }

            if (!string.IsNullOrEmpty(relativePath))
            {
                string physicalPath = Server.MapPath(relativePath);
                if (File.Exists(physicalPath))
                {
                    string fileName = Path.GetFileName(physicalPath);
                    string mimeType = MimeMapping.GetMimeMapping(physicalPath);

                    Response.Clear();
                    Response.ContentType = mimeType;
                    Response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                    Response.TransmitFile(physicalPath);
                    Response.Flush();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                    return;
                }
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "nofile", "alert('⚠️ StatCom certificate not found.');", true);
        }

        private async Task ProcessSimilarityDownloadAsync(string returnId)
        {
            var absoluteFilePaths = new List<string>();

            using (var connection = new SqlConnection(ConnStr))
            {
                const string query = @"
                    SELECT StatComCertPath, SimilarityFilePath, AIFilePath, MMSCReportFilePath, MMSCCertFilePath
                    FROM ProcessorReturnedReports
                    WHERE ReturnID = @ReturnID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReturnID", returnId);
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string[] columns = { "StatComCertPath", "SimilarityFilePath", "AIFilePath", "MMSCReportFilePath", "MMSCCertFilePath" };
                            foreach (string col in columns)
                            {
                                string relativePath = reader[col]?.ToString();
                                if (!string.IsNullOrEmpty(relativePath))
                                {
                                    string physicalPath = Server.MapPath(relativePath);
                                    if (File.Exists(physicalPath))
                                    {
                                        absoluteFilePaths.Add(physicalPath);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (absoluteFilePaths.Count == 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "noFiles", "alert('⚠️ No returned files found for this report.');", true);
                return;
            }

            string zipFileName = $"ReturnedDocs_Report_{returnId}.zip";
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (string filePath in absoluteFilePaths)
                    {
                        string fileName = Path.GetFileName(filePath);
                        archive.CreateEntryFromFile(filePath, fileName);
                    }
                }

                Response.Clear();
                Response.ContentType = "application/zip";
                Response.AppendHeader("Content-Disposition", $"attachment; filename=\"{zipFileName}\"");
                memoryStream.Position = 0;
                memoryStream.CopyTo(Response.OutputStream);
                Response.Flush();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        protected string GetFileName(object pathObj)
        {
            string path = pathObj?.ToString();
            return !string.IsNullOrWhiteSpace(path) ? Path.GetFileName(path) : "-";
        }

        private static async Task<DataTable> GetStatComReportsAsync(string searchQuery)
        {
            const string query = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY r.DateSent DESC) AS RowNumber,
                    r.ReturnID, 
                    r.DateSent, 
                    r.Author, 
                    r.Title, 
                    u.FullName AS ProcessorName, 
                    r.StatComCertPath
                FROM ProcessorReturnedReports r
                LEFT JOIN Users u ON r.SentByProcessorID = u.UserID
                WHERE r.ModuleType = 'StatComVal'
                  AND (@search = '' OR r.Author LIKE '%' + @search + '%' OR r.Title LIKE '%' + @search + '%')";

            using (var connection = new SqlConnection(ConnStr))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@search", searchQuery ?? string.Empty);
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        return dataTable;
                    }
                }
            }
        }

        private static async Task<DataTable> GetSimilarityReportsAsync(string searchQuery)
        {
            const string query = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY r.DateSent DESC) AS RowNumber,
                    r.ReturnID, 
                    r.DateSent, 
                    r.Author, 
                    r.Title, 
                    r.CaseNumber, 
                    u.FullName AS ProcessorName,
                    r.AIFilePath, 
                    r.SimilarityFilePath, 
                    r.MMSCReportFilePath, 
                    r.MMSCCertFilePath
                FROM ProcessorReturnedReports r
                LEFT JOIN Users u ON r.SentByProcessorID = u.UserID
                WHERE r.ModuleType = 'Similarity & AI Writing'
                  AND (@search = '' OR r.Author LIKE '%' + @search + '%' OR r.Title LIKE '%' + @search + '%')";

            using (var connection = new SqlConnection(ConnStr))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@search", searchQuery ?? string.Empty);
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        return dataTable;
                    }
                }
            }
        }
    }
}