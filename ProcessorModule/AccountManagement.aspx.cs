using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CASApp1.ProcessorModule
{
    public class AccountManagement : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;

        protected TextBox txtNewUsername;
        protected TextBox txtCurrentPasswordForUsername;
        protected Button btnUpdateUsername;
        protected Label lblUsernameStatus;
        protected TextBox txtNewPassword;
        protected TextBox txtConfirmPassword;
        protected Button btnUpdatePassword;
        protected Label lblPassStatus;
        protected PlaceHolder phSemesters;
        protected Button btnDeleteSelected;
        protected Label lblDeleteSelectedStatus;
        protected Button btnDeleteAll;
        protected Label lblDeleteAllStatus;
        protected HiddenField hfActionType;
        protected TextBox txtAdminPasswordConfirm;
        protected Label lblModalError;
        protected Button btnConfirmAction;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Convert.ToInt32(Session["UserID"]) != 1)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx", true);
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Dynamic control instantiation must happen synchronously during Init to correctly join the ViewState pipeline
            LoadSemesters();
        }

        private void LoadSemesters()
        {
            phSemesters.Controls.Clear();

            const string query = @"
                SELECT DISTINCT 
                    CASE 
                        WHEN MONTH(DateCreated) BETWEEN 8 AND 12 THEN 'Aug-Dec ' + CAST(YEAR(DateCreated) AS VARCHAR)
                        WHEN MONTH(DateCreated) BETWEEN 1 AND 5 THEN 'Jan-May ' + CAST(YEAR(DateCreated) AS VARCHAR)
                        ELSE 'Other'
                    END AS SemesterLabel,
                    MIN(DateCreated) AS StartDate, 
                    MAX(DateCreated) AS EndDate
                FROM Users
                WHERE UserID <> 1
                  AND Role = 'Student'
                GROUP BY 
                    CASE 
                        WHEN MONTH(DateCreated) BETWEEN 8 AND 12 THEN 'Aug-Dec ' + CAST(YEAR(DateCreated) AS VARCHAR)
                        WHEN MONTH(DateCreated) BETWEEN 1 AND 5 THEN 'Jan-May ' + CAST(YEAR(DateCreated) AS VARCHAR)
                        ELSE 'Other'
                    END
                ORDER BY MIN(DateCreated) DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int controlIndex = 0;
                        while (reader.Read())
                        {
                            string semesterLabel = reader["SemesterLabel"].ToString();
                            DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
                            DateTime endDate = Convert.ToDateTime(reader["EndDate"]);

                            var checkBox = new CheckBox
                            {
                                ID = $"cbSemester_{controlIndex++}",
                                CssClass = "form-check-input"
                            };

                            checkBox.InputAttributes["data-start"] = startDate.ToString("yyyy-MM-dd");
                            checkBox.InputAttributes["data-end"] = endDate.ToString("yyyy-MM-dd");

                            var wrapperOpen = new LiteralControl("<div class='form-check mb-1'>");
                            var labelControl = new LiteralControl($"<label class='form-check-label ms-2' for='{checkBox.ID}'>{semesterLabel}</label>");
                            var wrapperClose = new LiteralControl("</div>");

                            phSemesters.Controls.Add(wrapperOpen);
                            phSemesters.Controls.Add(checkBox);
                            phSemesters.Controls.Add(labelControl);
                            phSemesters.Controls.Add(wrapperClose);
                        }
                    }
                }
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        protected void btnUpdatePassword_Click(object sender, EventArgs e)
        {
            lblPassStatus.Text = string.Empty;
            lblPassStatus.ForeColor = Color.Red;

            if (string.IsNullOrWhiteSpace(txtNewPassword.Text) || string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                lblPassStatus.Text = "Please fill in both password fields.";
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                lblPassStatus.Text = "Passwords do not match.";
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string hashedNewPassword = HashPassword(txtNewPassword.Text);

                using (var connection = new SqlConnection(_connectionString))
                {
                    const string query = "UPDATE Users SET Password = @Password WHERE UserID = 1";
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Password", hashedNewPassword);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                lblPassStatus.Text = "Password updated successfully.";
                lblPassStatus.ForeColor = Color.Green;
                lblPassStatus.Visible = true;
                txtNewPassword.Text = string.Empty;
                txtConfirmPassword.Text = string.Empty;
            }));
        }

        protected void btnUpdateUsername_Click(object sender, EventArgs e)
        {
            lblUsernameStatus.Text = string.Empty;
            lblUsernameStatus.ForeColor = Color.Red;

            string cleanNewUsername = txtNewUsername.Text.Trim();
            string currentPasswordRaw = txtCurrentPasswordForUsername.Text;

            if (string.IsNullOrEmpty(cleanNewUsername))
            {
                lblUsernameStatus.Text = "Please enter a new username.";
                return;
            }

            if (string.IsNullOrEmpty(currentPasswordRaw))
            {
                lblUsernameStatus.Text = "Please enter your current password to confirm.";
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string verifyPasswordHash = HashPassword(currentPasswordRaw);
                string existingHashedPassword = null;
                string currentUsername = null;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    const string selectUserQuery = "SELECT Password, LoginID FROM Users WHERE UserID = 1";
                    using (var command = new SqlCommand(selectUserQuery, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                existingHashedPassword = reader["Password"].ToString();
                                currentUsername = reader["LoginID"].ToString();
                            }
                        }
                    }

                    if (verifyPasswordHash != existingHashedPassword)
                    {
                        lblUsernameStatus.Text = "Incorrect current password.";
                        lblUsernameStatus.ForeColor = Color.Red;
                        lblUsernameStatus.Visible = true;
                        return;
                    }

                    const string checkDuplicateQuery = "SELECT COUNT(*) FROM Users WHERE LoginID = @LoginID AND UserID <> 1";
                    using (var command = new SqlCommand(checkDuplicateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@LoginID", cleanNewUsername);
                        int duplicateCount = (int)await command.ExecuteScalarAsync();

                        if (duplicateCount > 0)
                        {
                            lblUsernameStatus.Text = "This username is already taken. Please choose another.";
                            lblUsernameStatus.ForeColor = Color.Red;
                            lblUsernameStatus.Visible = true;
                            return;
                        }
                    }

                    if (cleanNewUsername == currentUsername)
                    {
                        lblUsernameStatus.Text = "The new username is the same as the current one.";
                        lblUsernameStatus.ForeColor = Color.Orange;
                        lblUsernameStatus.Visible = true;
                        return;
                    }

                    const string updateUsernameQuery = "UPDATE Users SET LoginID = @LoginID WHERE UserID = 1";
                    using (var command = new SqlCommand(updateUsernameQuery, connection))
                    {
                        command.Parameters.AddWithValue("@LoginID", cleanNewUsername);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                lblUsernameStatus.Text = "Username updated successfully.";
                lblUsernameStatus.ForeColor = Color.Green;
                lblUsernameStatus.Visible = true;
                txtNewUsername.Text = string.Empty;
                txtCurrentPasswordForUsername.Text = string.Empty;
            }));
        }

        protected void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            lblModalError.Visible = false;
            lblModalError.Text = string.Empty;
            hfActionType.Value = "DeleteSelected";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal",
                "var modal = new bootstrap.Modal(document.getElementById('passwordModal')); modal.show();", true);
        }

        protected void btnDeleteAll_Click(object sender, EventArgs e)
        {
            lblModalError.Visible = false;
            lblModalError.Text = string.Empty;
            hfActionType.Value = "DeleteAll";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal",
                "var modal = new bootstrap.Modal(document.getElementById('passwordModal')); modal.show();", true);
        }

        protected void btnConfirmAction_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string inputAdminHash = HashPassword(txtAdminPasswordConfirm.Text);
                string trueAdminHash;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT Password FROM Users WHERE UserID = 1", connection))
                    {
                        var scalarResult = await command.ExecuteScalarAsync();
                        trueAdminHash = scalarResult?.ToString();
                    }
                }

                if (inputAdminHash != trueAdminHash)
                {
                    lblModalError.Text = "Incorrect admin password.";
                    lblModalError.Visible = true;
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowModalAgain",
                        "var modal = new bootstrap.Modal(document.getElementById('passwordModal')); modal.show();", true);
                    return;
                }

                if (hfActionType.Value == "DeleteAll")
                {
                    await DeleteAllExceptAdminAsync();
                    lblDeleteAllStatus.Visible = true;
                    lblDeleteAllStatus.Text = "All accounts have been deleted.";
                    lblDeleteAllStatus.ForeColor = Color.Green;
                }
                else if (hfActionType.Value == "DeleteSelected")
                {
                    int recordsDeleted = await DeleteSelectedSemestersAsync();
                    lblDeleteSelectedStatus.Visible = true;

                    if (recordsDeleted > 0)
                    {
                        lblDeleteSelectedStatus.Text = $"{recordsDeleted} account(s) from selected semester(s) deleted.";
                        lblDeleteSelectedStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblDeleteSelectedStatus.Text = "⚠️ No accounts found for selected semester(s).";
                        lblDeleteSelectedStatus.ForeColor = Color.Red;
                    }
                }

                LoadSemesters();

                ScriptManager.RegisterStartupScript(this, GetType(), "closeModal",
                    "var modalEl = document.getElementById('passwordModal'); var modal = bootstrap.Modal.getInstance(modalEl); modal.hide();", true);

                lblModalError.Visible = false;
                lblModalError.Text = string.Empty;
                txtAdminPasswordConfirm.Text = string.Empty;
            }));
        }

        private async Task DeleteUserCascadeAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var deletionQueries = new[]
                {
                    "DELETE FROM StatComVal_Submission WHERE UserID = @UserID",
                    "DELETE FROM SimilarityAICheck_Submission WHERE UserID = @UserID",
                    "DELETE FROM ProcessorReturnedReports WHERE UserID = @UserID",
                    "DELETE FROM RegistrationCodes WHERE CreatedByUserID = @UserID", // Maps clean logic to original variable parameters safely
                    "DELETE FROM Users WHERE UserID = @UserID"
                };

                foreach (string query in deletionQueries)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        // Maps cleanly to both @UserID and historical tracking variables seamlessly 
                        command.Parameters.AddWithValue("@UserID", userId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        private async Task DeleteAllExceptAdminAsync()
        {
            var userIdsToDelete = new List<int>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string selectQuery = "SELECT UserID FROM Users WHERE UserID <> 1 AND Role IN ('Student', 'Admin')";
                using (var command = new SqlCommand(selectQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            userIdsToDelete.Add(reader.GetInt32(0));
                        }
                    }
                }

                foreach (int userId in userIdsToDelete)
                {
                    await DeleteUserCascadeAsync(userId);
                }

                const string reseedQuery = "DBCC CHECKIDENT ('Users', RESEED, 1)";
                using (var command = new SqlCommand(reseedQuery, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<int> DeleteSelectedSemestersAsync()
        {
            int accountsDeletedCount = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (Control control in phSemesters.Controls)
                {
                    if (control is CheckBox checkBox && checkBox.Checked)
                    {
                        string dataStartAttr = checkBox.InputAttributes["data-start"];
                        string dataEndAttr = checkBox.InputAttributes["data-end"];

                        DateTime semesterStartDate = DateTime.ParseExact(dataStartAttr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime semesterEndExclusiveDate = DateTime.ParseExact(dataEndAttr, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1.0);

                        var targetedUserIds = new List<int>();
                        const string selectTargetedUsersQuery = @"
                            SELECT UserID 
                            FROM Users
                            WHERE UserID <> 1
                              AND Role = 'Student'
                              AND DateCreated >= @Start
                              AND DateCreated < @EndExclusive;";

                        using (var command = new SqlCommand(selectTargetedUsersQuery, connection))
                        {
                            command.Parameters.Add("@Start", SqlDbType.DateTime2).Value = semesterStartDate;
                            command.Parameters.Add("@EndExclusive", SqlDbType.DateTime2).Value = semesterEndExclusiveDate;

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    targetedUserIds.Add(reader.GetInt32(0));
                                }
                            }
                        }

                        foreach (int userId in targetedUserIds)
                        {
                            await DeleteUserCascadeAsync(userId);
                            accountsDeletedCount++;
                        }
                    }
                }

                const string checkReseedQuery = "IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID <> 1) DBCC CHECKIDENT ('Users', RESEED, 1)";
                using (var command = new SqlCommand(checkReseedQuery, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }

            return accountsDeletedCount;
        }
    }
}