using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace CASApp1.ProcessorModule
{
    public class UserList : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
        private const string SessionStudents = "Student";
        private const string SessionProcessors = "Admin";
        private const string ViewStateCurrentView = "CurrentView";

        protected Panel pnlStudentCode;
        protected Label lblRegistrationStu;
        protected Label lblStudentCode;
        protected Button btnGenerateStudentCode;
        protected Panel pnlProcessorCode;
        protected Label lblRegistrationProc;
        protected Label lblProcessorCode;
        protected Button btnGenerateProcessorCode;
        protected Button btnShowStudents;
        protected Button btnShowProcessors;
        protected TextBox txtSearch;
        protected Button btnSearch;
        protected Button btnReset;
        protected Panel pnlStudents;
        protected GridView gvStudents;
        protected HtmlInputCheckBox chkStatComVal;
        protected HtmlInputCheckBox chkSimAiWri;
        protected TextBox txtConfirmPassword;
        protected Button btnConfirmReset;
        protected HiddenField hfResetUserId;
        protected Panel pnlProcessors;
        protected GridView gvProcessors;
        protected HiddenField hfDeleteUserId;
        protected TextBox txtDeletePassword;
        protected Button btnConfirmDelete;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(InitializePageDataAsync));
            }
        }

        private async Task InitializePageDataAsync()
        {
            await LoadStudentDataAsync();
            await LoadProcessorDataAsync();
            ShowStudents();

            string currentRole = Session["Role"]?.ToString();
            int currentUserId = Convert.ToInt32(Session["UserID"]);

            bool isSuperAdmin = currentUserId == 1;
            bool isStandardAdmin = currentRole == "Admin" && !isSuperAdmin;

            if (isStandardAdmin)
            {
                pnlProcessorCode.Visible = false;
            }

            btnGenerateStudentCode.Visible = isSuperAdmin || isStandardAdmin;
            btnGenerateProcessorCode.Visible = isSuperAdmin;

            string existingStudentCode = await GetExistingCodeAsync("Student");
            if (!string.IsNullOrEmpty(existingStudentCode))
            {
                lblStudentCode.Text = existingStudentCode;
                lblStudentCode.Visible = true;
                btnGenerateStudentCode.Text = "Disable Code";
            }
            else
            {
                lblStudentCode.Visible = false;
                btnGenerateStudentCode.Text = "Generate Registration Code";
            }

            string existingProcessorCode = await GetExistingCodeAsync("Admin");
            if (!string.IsNullOrEmpty(existingProcessorCode))
            {
                lblProcessorCode.Text = existingProcessorCode;
                lblProcessorCode.Visible = true;
                btnGenerateProcessorCode.Text = "Disable Code";
            }
            else
            {
                lblProcessorCode.Visible = false;
                btnGenerateProcessorCode.Text = "Generate Registration Code";
            }
        }

        private async Task LoadStudentDataAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT UserID, FullName, Email, StudentID, College FROM Users WHERE Role = 'Student'";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        // Fill is synchronously blocking, but opening connection asynchronously reduces connection pool stress
                        await connection.OpenAsync();
                        adapter.Fill(dataTable);

                        Session[SessionStudents] = dataTable;
                        gvStudents.DataSource = dataTable;
                        gvStudents.DataBind();
                    }
                }
            }
        }

        private async Task LoadProcessorDataAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT UserID, FullName, Affiliation, Specialization, Email FROM Users WHERE Role = 'Admin' AND UserID <> 1";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        await connection.OpenAsync();
                        adapter.Fill(dataTable);

                        Session[SessionProcessors] = dataTable;
                        if (gvProcessors.Visible)
                        {
                            gvProcessors.DataSource = dataTable;
                            gvProcessors.DataBind();
                        }
                    }
                }
            }
        }

        private void ShowStudents()
        {
            ViewState[ViewStateCurrentView] = "Students";
            pnlStudents.Visible = true;
            pnlProcessors.Visible = false;
            pnlStudentCode.Visible = true;
            pnlProcessorCode.Visible = false;
            btnShowStudents.CssClass = "btn btn-primary";
            btnShowProcessors.CssClass = "btn btn-outline-secondary";
        }

        private void ShowProcessors()
        {
            ViewState[ViewStateCurrentView] = "Admin";
            int currentUserId = Convert.ToInt32(Session["UserID"]);
            bool isSuperAdmin = currentUserId == 1;

            pnlStudents.Visible = false;
            pnlProcessors.Visible = true;
            pnlStudentCode.Visible = false;
            pnlProcessorCode.Visible = isSuperAdmin;
            btnShowStudents.CssClass = "btn btn-outline-secondary";
            btnShowProcessors.CssClass = "btn btn-primary";
        }

        protected void btnShowStudents_Click(object sender, EventArgs e)
        {
            ShowStudents();
            txtSearch.Text = string.Empty;
            gvStudents.DataSource = Session[SessionStudents];
            gvStudents.DataBind();
        }

        protected void btnShowProcessors_Click(object sender, EventArgs e)
        {
            ShowProcessors();
            txtSearch.Text = string.Empty;
            gvProcessors.DataSource = Session[SessionProcessors];
            gvProcessors.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim().Replace("'", "''");
            string currentView = ViewState[ViewStateCurrentView] as string;

            if (currentView == "Students" && Session[SessionStudents] is DataTable studentTable)
            {
                DataView view = studentTable.DefaultView;
                view.RowFilter = $"FullName LIKE '%{searchTerm}%'";
                gvStudents.DataSource = view;
                gvStudents.DataBind();
            }
            else if (currentView == "Admin" && Session[SessionProcessors] is DataTable processorTable)
            {
                DataView view = processorTable.DefaultView;
                view.RowFilter = $"FullName LIKE '%{searchTerm}%'";
                gvProcessors.DataSource = view;
                gvProcessors.DataBind();
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            if (pnlStudents.Visible)
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await LoadStudentDataAsync();
                    gvStudents.DataSource = Session[SessionStudents] as DataTable;
                    gvStudents.DataBind();
                }));
            }
            else if (pnlProcessors.Visible)
            {
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await LoadProcessorDataAsync();
                    gvProcessors.DataSource = Session[SessionProcessors] as DataTable;
                    gvProcessors.DataBind();
                }));
            }
        }

        protected void gvStudents_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvStudents.PageIndex = e.NewPageIndex;
            if (Session[SessionStudents] is DataTable dataTable)
            {
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchTerm = txtSearch.Text.Trim().Replace("'", "''");
                    DataView view = dataTable.DefaultView;
                    view.RowFilter = $"FullName LIKE '%{searchTerm}%'";
                    gvStudents.DataSource = view;
                }
                else
                {
                    gvStudents.DataSource = dataTable;
                }
                gvStudents.DataBind();
            }
        }

        private async Task DeleteUserAsync(int userId)
        {
            if (userId == 1 || Convert.ToInt32(Session["UserID"]) != 1)
                return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string[] deletionQueries =
                {
                    "DELETE FROM StatComVal_Submission WHERE UserID = @UserID",
                    "DELETE FROM SimilarityAICheck_Submission WHERE UserID = @UserID",
                    "DELETE FROM ProcessorReturnedReports WHERE UserID = @UserID",
                    "DELETE FROM RegistrationCodes WHERE CreatedByUserID = @UserID",
                    "DELETE FROM Users WHERE UserID = @UserID"
                };

                foreach (string query in deletionQueries)
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        protected void gvStudents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteUser" && int.TryParse(e.CommandArgument.ToString(), out int targetUserId))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "openDeleteModal", $"openDeleteModal({targetUserId});", true);
            }
        }

        protected void gvProcessors_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteUser" && int.TryParse(e.CommandArgument.ToString(), out int targetUserId))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "openDeleteModal", $"openDeleteModal({targetUserId});", true);
            }
        }

        protected void gvStudents_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            bool isSuperAdmin = Convert.ToInt32(Session["UserID"]) == 1;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.FindControl("btnDeleteStudent") is Button deleteButton)
                {
                    deleteButton.Visible = isSuperAdmin;
                }
            }
        }

        protected void gvProcessors_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            bool isSuperAdmin = Convert.ToInt32(Session["UserID"]) == 1;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "UserID")) == 1)
                {
                    e.Row.Visible = false;
                    return;
                }

                if (e.Row.FindControl("btnDeleteProcessor") is Button deleteButton)
                {
                    deleteButton.Visible = isSuperAdmin;
                }
            }
            else if (e.Row.RowType == DataControlRowType.Header && !isSuperAdmin)
            {
                e.Row.Cells[e.Row.Cells.Count - 1].Text = string.Empty;
            }
        }

        private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        protected void btnGenerateStudentCode_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (btnGenerateStudentCode.Text.Contains("Disable"))
                {
                    await DeleteExistingCodeAsync("Student");
                    lblStudentCode.Visible = false;
                    btnGenerateStudentCode.Text = "Generate Code";
                }
                else
                {
                    string targetCode = await GetExistingCodeAsync("Student");
                    if (string.IsNullOrEmpty(targetCode))
                    {
                        targetCode = GenerateUniqueCode();
                        using (SqlConnection connection = new SqlConnection(_connectionString))
                        {
                            const string query = "INSERT INTO RegistrationCodes (Code, Role, CreatedByUserID) VALUES (@Code, @Role, @CreatedByUserID)";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Code", targetCode);
                                command.Parameters.AddWithValue("@Role", "Student");
                                command.Parameters.AddWithValue("@CreatedByUserID", Session["UserID"]);
                                await connection.OpenAsync();
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    lblStudentCode.Text = $" {targetCode}";
                    lblStudentCode.Visible = true;
                    btnGenerateStudentCode.Text = "Disable Code";
                }
            }));
        }

        protected void btnGenerateProcessorCode_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (btnGenerateProcessorCode.Text.Contains("Disable"))
                {
                    await DeleteExistingCodeAsync("Admin");
                    lblProcessorCode.Visible = false;
                    btnGenerateProcessorCode.Text = "Generate Code";
                }
                else
                {
                    string targetCode = await GetExistingCodeAsync("Admin");
                    if (string.IsNullOrEmpty(targetCode))
                    {
                        targetCode = GenerateUniqueCode();
                        using (SqlConnection connection = new SqlConnection(_connectionString))
                        {
                            const string query = "INSERT INTO RegistrationCodes (Code, Role, CreatedByUserID) VALUES (@Code, @Role, @CreatedByUserID)";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Code", targetCode);
                                command.Parameters.AddWithValue("@Role", "Admin");
                                command.Parameters.AddWithValue("@CreatedByUserID", Session["UserID"]);
                                await connection.OpenAsync();
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    lblProcessorCode.Text = $" {targetCode}";
                    lblProcessorCode.Visible = true;
                    btnGenerateProcessorCode.Text = "Disable Code";
                }
            }));
        }

        private async Task<string> GetExistingCodeAsync(string role)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT Code FROM RegistrationCodes WHERE Role = @Role AND CreatedByUserID = @CreatedByUserID AND IsUsed = 0";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@CreatedByUserID", Session["UserID"]);
                    await connection.OpenAsync();
                    object output = await command.ExecuteScalarAsync();
                    return output?.ToString();
                }
            }
        }

        private async Task DeleteExistingCodeAsync(string role)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "DELETE FROM RegistrationCodes WHERE Role = @Role AND CreatedByUserID = @CreatedByUserID AND IsUsed = 0";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@CreatedByUserID", Session["UserID"]);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        protected void btnDisableStudentCode_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                await DeleteExistingCodeAsync("Student");
                btnGenerateStudentCode.Text = "Generate Code";
            }));
        }

        protected void btnDisableProcessorCode_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                await DeleteExistingCodeAsync("Admin");
                btnGenerateProcessorCode.Text = "Generate Code";
            }));
        }

        protected void btnConfirmReset_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfResetUserId.Value, out int targetUserId))
                return;

            GridViewRow targetRow = gvStudents.Rows.Cast<GridViewRow>()
                .FirstOrDefault(r => Convert.ToInt32(gvStudents.DataKeys[r.RowIndex]?.Value) == targetUserId);

            if (targetRow == null || !(targetRow.FindControl("spnResetStatus") is HtmlGenericControl statusSpan))
                return;

            statusSpan.InnerText = string.Empty;

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                int currentUserId = Convert.ToInt32(Session["UserID"]);
                bool isPasswordValid = await ValidatePasswordAsync(currentUserId, txtConfirmPassword.Text.Trim());

                if (!isPasswordValid)
                {
                    statusSpan.InnerText = "Failed";
                    statusSpan.Style["color"] = "red";
                    return;
                }

                bool isSuccessful = false;
                try
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        if (chkStatComVal.Checked)
                        {
                            const string query = "DELETE FROM StatComVal_Submission WHERE UserID = @UserID";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@UserID", targetUserId);
                                await command.ExecuteNonQueryAsync();
                                isSuccessful = true;
                            }
                        }
                        if (chkSimAiWri.Checked)
                        {
                            const string query = "DELETE FROM SimilarityAICheck_Submission WHERE UserID = @UserID";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@UserID", targetUserId);
                                await command.ExecuteNonQueryAsync();
                                isSuccessful = true;
                            }
                        }
                    }

                    statusSpan.InnerText = isSuccessful ? "✔" : "Failed";
                    statusSpan.Style["color"] = isSuccessful ? "orange" : "red";
                }
                catch
                {
                    statusSpan.InnerText = "Failed!";
                    statusSpan.Style["color"] = "red";
                }
            }));
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteUserId.Value, out int targetUserId))
                return;

            GridView activeGrid = pnlStudents.Visible ? gvStudents : gvProcessors;
            GridViewRow targetRow = activeGrid.Rows.Cast<GridViewRow>()
                .FirstOrDefault(r => Convert.ToInt32(activeGrid.DataKeys[r.RowIndex]?.Value) == targetUserId);

            if (targetRow == null || !(targetRow.FindControl("spnDeleteStatus") is HtmlGenericControl statusSpan))
                return;

            statusSpan.InnerText = string.Empty;
            string inputPassword = txtDeletePassword.Text.Trim();
            int currentUserId = Convert.ToInt32(Session["UserID"]);

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                bool isPasswordValid = await ValidatePasswordAsync(currentUserId, inputPassword);

                if (!isPasswordValid || currentUserId != 1)
                {
                    statusSpan.InnerText = "Failed!";
                    statusSpan.Style["color"] = "red";
                    return;
                }

                try
                {
                    await DeleteUserAsync(targetUserId);
                    if (pnlStudents.Visible)
                    {
                        await LoadStudentDataAsync();
                    }
                    else if (pnlProcessors.Visible)
                    {
                        await LoadProcessorDataAsync();
                        gvProcessors.DataSource = Session[SessionProcessors];
                        gvProcessors.DataBind();
                    }

                    foreach (GridViewRow row in activeGrid.Rows)
                    {
                        if (row.FindControl("spnDeleteStatus") is HtmlGenericControl rowSpan)
                        {
                            rowSpan.InnerText = string.Empty;
                        }
                    }
                }
                catch
                {
                    statusSpan.InnerText = "❌";
                    statusSpan.Style["color"] = "red";
                }
            }));
        }

        private async Task<bool> ValidatePasswordAsync(int userId, string inputPassword)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT Password FROM Users WHERE UserID = @UserID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    await connection.OpenAsync();

                    string storedHash = (await command.ExecuteScalarAsync())?.ToString();
                    if (string.IsNullOrEmpty(storedHash))
                        return false;

                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                        string computedHash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                        return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }
    }
}