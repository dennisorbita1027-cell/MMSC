using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using BCrypt.Net;

namespace CASApp1.LogInPage
{
    public partial class LoginPage : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.IsSecureConnection)
            {
                Response.Redirect(Request.Url.ToString().Replace("http://", "https://"), true);
                return;
            }

            if (!IsPostBack)
            {
                mvLogin.ActiveViewIndex = 0;
            }

            if (Request.QueryString["registered"] == "1")
            {
                mvLogin.ActiveViewIndex = 1;
                lblLoginError.Text = "Registration successful. You can now authenticate.";
                lblLoginError.ForeColor = Color.Green;
            }
        }

        protected void btnAdmin_Click(object sender, EventArgs e)
        {
            lblRole.Text = "Processor";
            mvLogin.ActiveViewIndex = 1;
            pnlStudentOptions.Visible = false;
            hfRegisterRole.Value = "Admin";
            lblRegister.Text = "Processor Register Gateway";

            txtAffiliation.Visible = true;
            txtSpecialization.Visible = true;
            txtStudentID.Visible = false;
            txtDegreeProgram.Visible = false;
        }

        protected void btnStudent_Click(object sender, EventArgs e)
        {
            lblRole.Text = "Student";
            mvLogin.ActiveViewIndex = 1;
            pnlStudentOptions.Visible = true;
            hfRegisterRole.Value = "Student";
            lblRegister.Text = "Student Register Gateway";

            txtAffiliation.Visible = false;
            txtSpecialization.Visible = false;
            txtStudentID.Visible = true;
            txtDegreeProgram.Visible = true;
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            mvLogin.ActiveViewIndex = 0;
            lblLoginError.Text = string.Empty;
            txtLoginID.Text = string.Empty;
            txtPassword.Text = string.Empty;
            pnlStudentOptions.Visible = false;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblLoginError.Text = string.Empty;
            string targetRole = lblRole.Text == "Processor" ? "Admin" : "Student";
            string loginInput = txtLoginID.Text.Trim();
            string passwordInput = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(loginInput) || string.IsNullOrEmpty(passwordInput))
            {
                lblLoginError.Text = "All authentication fields are required.";
                lblLoginError.ForeColor = Color.Red;
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string derivedEmail = loginInput.Contains("@") ? loginInput : $"{loginInput}@gmail.com";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT UserID, Role, LoginID, Password FROM Users WHERE (LoginID = @Input OR Email = @Email) AND Role = @Role";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Input", loginInput);
                        command.Parameters.AddWithValue("@Email", derivedEmail);
                        command.Parameters.AddWithValue("@Role", targetRole);

                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string databaseHash = reader["Password"].ToString();

                                if (BCrypt.Net.BCrypt.EnhancedVerify(passwordInput, databaseHash))
                                {
                                    string userId = reader["UserID"].ToString();
                                    string role = reader["Role"].ToString();
                                    string loginId = reader["LoginID"].ToString();

                                    Session["UserID"] = userId;
                                    Session["Username"] = loginId;
                                    Session["LoginID"] = loginId;
                                    Session["Role"] = role;

                                    if (userId == "1")
                                    {
                                        Session["IsHeadAdmin"] = true;
                                    }

                                    if (targetRole == "Admin")
                                    {
                                        Response.Redirect("~/ProcessorModule/ProcessorHome.aspx", false);
                                        Context.ApplicationInstance.CompleteRequest();
                                    }
                                    else
                                    {
                                        bool isStatCheck = StaticChlk.Checked;
                                        bool isSimCheck = SimChk.Checked;

                                        if ((!isStatCheck && !isSimCheck) || (isStatCheck && isSimCheck))
                                        {
                                            lblLoginError.Text = "Please select precisely one application module option.";
                                            lblLoginError.ForeColor = Color.Red;
                                            return;
                                        }

                                        Session["StaticChlk"] = isStatCheck;
                                        Session["SimChk"] = isSimCheck;
                                        Session["StudentType"] = isStatCheck ? "StatVal" : "Similarity";

                                        Response.Redirect("~/UserModule/Home.aspx", false);
                                        Context.ApplicationInstance.CompleteRequest();
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }

                lblLoginError.Text = "Invalid credentials tracking criteria matching account.";
                lblLoginError.ForeColor = Color.Red;
            }));
        }

        protected void btnSubmitRegister_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(ProcessRegistrationAsync));
        }

        private async Task ProcessRegistrationAsync()
        {
            string fullName = txtFullName.Text.Trim();
            string studentId = txtStudentID.Text.Trim();
            string email = txtEmail.Text.Trim();
            string degreeProg = txtDegreeProgram.Text.Trim();
            string regCode = txtRegistrationCode.Text.Trim();
            string affiliation = txtAffiliation?.Text.Trim();
            string specialization = txtSpecialization?.Text.Trim();

            string rawPassword = CleanInput(txtRegPassword.Text);
            string confirmPassword = CleanInput(txtConfirmPassword.Text);
            string targetRole = hfRegisterRole.Value == "Admin" ? "Admin" : "Student";

            string calculatedUsername = email.Contains("@") ? email.Substring(0, email.IndexOf("@")) : email;

            if (string.IsNullOrWhiteSpace(regCode) || string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(rawPassword))
            {
                ShowRegisterError("All fields marked are structural requirements.");
                return;
            }

            if (calculatedUsername.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                ShowRegisterError("Protected routing string identifier 'admin' is unavailable.");
                return;
            }

            if (rawPassword != confirmPassword)
            {
                ShowRegisterError("Password confirmation verification properties mismatch.");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowRegisterError("Invalid structural framework syntax format inside Email property.");
                return;
            }

            if (targetRole == "Student" && (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(degreeProg)))
            {
                ShowRegisterError("Student references and Degree plans are required for target records.");
                return;
            }

            if (targetRole == "Admin" && (string.IsNullOrWhiteSpace(affiliation) || string.IsNullOrWhiteSpace(specialization)))
            {
                ShowRegisterError("Affiliation and technical specialization parameters are critical for Processors.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const string tokenCheckQuery = "SELECT COUNT(*) FROM RegistrationCodes WHERE Code = @Code AND Role = @Role";
                using (SqlCommand tokenCmd = new SqlCommand(tokenCheckQuery, connection))
                {
                    tokenCmd.Parameters.AddWithValue("@Code", regCode);
                    tokenCmd.Parameters.AddWithValue("@Role", targetRole);
                    if (Convert.ToInt32(await tokenCmd.ExecuteScalarAsync()) == 0)
                    {
                        ShowRegisterError("Authorization token mismatch or expired tracking values.");
                        return;
                    }
                }

                const string identityCheckQuery = "SELECT COUNT(*) FROM Users WHERE LoginID = @Username OR Email = @Email";
                using (SqlCommand checkCmd = new SqlCommand(identityCheckQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Username", calculatedUsername);
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0)
                    {
                        ShowRegisterError("An identity entry utilizing that email metadata already exists.");
                        return;
                    }
                }

                string secureHash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawPassword, 11);

                const string insertQuery = @"
                    INSERT INTO Users (FullName, StudentID, Email, College, LoginID, Password, Role, Affiliation, Specialization, DateCreated)
                    VALUES (@FullName, @StudentID, @Email, @College, @LoginID, @Password, @Role, @Affiliation, @Specialization, @DateCreated)";

                using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@FullName", fullName);
                    insertCmd.Parameters.AddWithValue("@StudentID", targetRole == "Student" ? studentId : (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Email", email);
                    insertCmd.Parameters.AddWithValue("@College", targetRole == "Student" ? degreeProg : (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@LoginID", calculatedUsername);
                    insertCmd.Parameters.AddWithValue("@Password", secureHash);
                    insertCmd.Parameters.AddWithValue("@Role", targetRole);
                    insertCmd.Parameters.AddWithValue("@Affiliation", targetRole == "Admin" ? affiliation : (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Specialization", targetRole == "Admin" ? specialization : (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@DateCreated", DateTime.Now);

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            ClearRegistrationInputs();

            ScriptManager.RegisterStartupScript(this, GetType(), "CompleteRegistration",
                "var m = bootstrap.Modal.getInstance(document.getElementById('registerModal')); if(m) m.hide();", true);

            mvLogin.ActiveViewIndex = 1;
            lblLoginError.Text = "Account generated successfully. Complete authentication challenge.";
            lblLoginError.ForeColor = Color.Green;
        }

        private void ShowRegisterError(string message)
        {
            lblRegisterError.ForeColor = Color.Red;
            lblRegisterError.Text = message;
            ScriptManager.RegisterStartupScript(this, GetType(), "MaintainModal", "openRegisterModal();", true);
        }

        private void ClearRegistrationInputs()
        {
            txtFullName.Text = string.Empty;
            txtStudentID.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtDegreeProgram.Text = string.Empty;
            txtRegPassword.Text = string.Empty;
            txtConfirmPassword.Text = string.Empty;
            txtAffiliation.Text = string.Empty;
            txtSpecialization.Text = string.Empty;
            txtRegistrationCode.Text = string.Empty;
            lblRegisterError.Text = string.Empty;
        }

        private string CleanInput(string input) => Regex.Replace(input ?? string.Empty, @"\p{C}+", string.Empty).Trim();
    }
}