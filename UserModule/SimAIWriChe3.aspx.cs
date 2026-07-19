using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CASApp1.UserModule
{
    public class SimAIWriChe3 : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
        private const string UploadDirectoryPath = "~/App_Data/Uploads/SimAICheckUploads/";

        protected Label lblPageStatus;
        protected TextBox txtTitle;
        protected TextBox txtAuthors;
        protected TextBox txtReceiptNo;
        protected FileUpload fuCopy;
        protected FileUpload fuDocument;
        protected TextBox txtCollege;
        protected RadioButtonList rblType;
        protected TextBox txtOthers;
        protected TextBox txtRevNo;
        protected Button btnSubmit;
        protected Label lblMessage;
        protected FileUpload fuDeclaration;
        protected Button btnUploadDeclaration;
        protected Label lblUploadStatus;

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
            int userId = Convert.ToInt32(Session["UserID"]);
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    const string checkDisabledQuery = "SELECT IsDisabled FROM SimilarityAICheck_Submission WHERE UserID = @UserID ORDER BY SubmissionID DESC";
                    using (SqlCommand command = new SqlCommand(checkDisabledQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && Convert.ToInt32(result) == 1)
                        {
                            DisableSimAIWriChe();
                            lblPageStatus.Text = "Your certification has already been issued. For any questions or concerns regarding your submission, please visit the MMSC office or contact us via email at nvsu.mmsc@nvsu.edu.ph. When submitting a request or inquiry to this email, please include your full name, course, and student ID (if applicable) to ensure a prompt and accurate response.";
                            lblPageStatus.ForeColor = Color.Black;
                            return;
                        }
                    }

                    const string loadPreviousQuery = "SELECT TOP 1 Title, Author, ReceiptNo, College, RevisionNo FROM SimilarityAICheck_Submission WHERE UserID = @UserID ORDER BY SubmissionID DESC";
                    using (SqlCommand command = new SqlCommand(loadPreviousQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                txtTitle.Text = reader["Title"]?.ToString();
                                txtAuthors.Text = reader["Author"]?.ToString();
                                txtReceiptNo.Text = reader["ReceiptNo"]?.ToString();
                                txtCollege.Text = reader["College"]?.ToString();
                                txtRevNo.Text = reader["RevisionNo"]?.ToString();

                                lblMessage.Text = "Your previous submission is loaded. You can edit and resubmit.";
                                lblMessage.ForeColor = Color.Blue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"An error occurred while loading previous data: {ex.Message}";
                lblMessage.ForeColor = Color.Red;
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtAuthors.Text) ||
                string.IsNullOrWhiteSpace(txtReceiptNo.Text) || string.IsNullOrWhiteSpace(txtCollege.Text) ||
                string.IsNullOrWhiteSpace(txtRevNo.Text) || !fuDocument.HasFile || !fuCopy.HasFile)
            {
                lblMessage.Text = "Please complete all required fields and upload both files before submitting.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                string finalType = !string.IsNullOrWhiteSpace(txtOthers.Text) ? txtOthers.Text.Trim() : rblType.SelectedValue;

                try
                {
                    string targetFolder = Server.MapPath(UploadDirectoryPath);
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    string fullName = await GetFullNameByUserIdAsync(userId);
                    string docSavedPath = await SaveFileAsync(fuDocument, targetFolder, fullName, userId);
                    string copySavedPath = await SaveFileAsync(fuCopy, targetFolder, fullName, userId);

                    if (docSavedPath == null || copySavedPath == null)
                    {
                        return; // File type validation failed inside SaveFileAsync, message already set
                    }

                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        const string getLatestSubmissionQuery = "SELECT TOP 1 SubmissionID FROM SimilarityAICheck_Submission WHERE UserID = @UserID ORDER BY SubmissionID DESC";
                        object existingSubmissionId = null;

                        using (SqlCommand command = new SqlCommand(getLatestSubmissionQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userId);
                            existingSubmissionId = await command.ExecuteScalarAsync();
                        }

                        SqlCommand actionCommand;
                        if (existingSubmissionId != null)
                        {
                            const string updateQuery = @"
                                UPDATE SimilarityAICheck_Submission SET
                                    Title = @Title,
                                    Author = @Author,
                                    ReceiptNo = @ReceiptNo,
                                    DocumentFilePath = @DocPath,
                                    CopyFilePath = @CopyPath,
                                    RevisionNo = @RevisionNo,
                                    College = @College,
                                    Type = @Type,
                                    SubmissionDate = GETDATE(),
                                    IsHighlighted = 1
                                WHERE SubmissionID = @SubmissionID";

                            actionCommand = new SqlCommand(updateQuery, connection);
                            actionCommand.Parameters.AddWithValue("@SubmissionID", Convert.ToInt32(existingSubmissionId));
                        }
                        else
                        {
                            const string insertQuery = @"
                                INSERT INTO SimilarityAICheck_Submission
                                    (UserID, Title, Author, ReceiptNo, DocumentFilePath, CopyFilePath, RevisionNo, College, Type, SubmissionDate, IsHighlighted)
                                VALUES
                                    (@UserID, @Title, @Author, @ReceiptNo, @DocPath, @CopyPath, @RevisionNo, @College, @Type, GETDATE(), 1)";

                            actionCommand = new SqlCommand(insertQuery, connection);
                            actionCommand.Parameters.AddWithValue("@UserID", userId);
                        }

                        using (actionCommand)
                        {
                            actionCommand.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                            actionCommand.Parameters.AddWithValue("@Author", txtAuthors.Text.Trim());
                            actionCommand.Parameters.AddWithValue("@ReceiptNo", txtReceiptNo.Text.Trim());
                            actionCommand.Parameters.AddWithValue("@DocPath", docSavedPath);
                            actionCommand.Parameters.AddWithValue("@CopyPath", copySavedPath);
                            actionCommand.Parameters.AddWithValue("@RevisionNo", txtRevNo.Text.Trim());
                            actionCommand.Parameters.AddWithValue("@College", txtCollege.Text.Trim());
                            actionCommand.Parameters.AddWithValue("@Type", finalType);

                            await actionCommand.ExecuteNonQueryAsync();
                        }
                    }

                    lblMessage.Text = $"✅ Submission successful! Revision No: {txtRevNo.Text}";
                    lblMessage.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    lblMessage.Text = $"❌ An error occurred during submission: {ex.Message}";
                    lblMessage.ForeColor = Color.Red;
                }
            }));
        }

        protected void btnDeclaration_Click(object sender, EventArgs e)
        {
            if (sender is Button sourceButton)
            {
                string relativePath = sourceButton.CommandArgument;
                if (string.IsNullOrEmpty(relativePath)) return;

                string fullPath = Server.MapPath(relativePath);
                if (File.Exists(fullPath))
                {
                    Response.Clear();
                    Response.ContentType = MimeMapping.GetMimeMapping(fullPath);
                    Response.AppendHeader("Content-Disposition", $"attachment; filename={Path.GetFileName(fullPath)}");
                    Response.TransmitFile(fullPath);
                    Response.End();
                }
                else
                {
                    lblMessage.Text = "Declaration file not found.";
                    lblMessage.ForeColor = Color.Red;
                }
            }
        }

        protected void btnUploadDeclaration_Click(object sender, EventArgs e)
        {
            if (!fuDeclaration.HasFile)
            {
                lblUploadStatus.Text = "⚠️ Please choose a file to upload.";
                lblUploadStatus.ForeColor = Color.Red;
                return;
            }

            string fileExtension = Path.GetExtension(fuDeclaration.FileName).ToLower();
            string[] allowedExtensions = { ".pdf", ".docx", ".doc" };

            if (!allowedExtensions.Contains(fileExtension))
            {
                lblUploadStatus.Text = $"❌ Invalid file type: {fileExtension}";
                lblUploadStatus.ForeColor = Color.Red;
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                try
                {
                    string cleanName = (await GetFullNameByUserIdAsync(userId)).Replace(" ", "_");
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string fileName = $"AIUseDeclaration_{cleanName}_{timestamp}{fileExtension}";
                    string targetFolder = Server.MapPath(UploadDirectoryPath);
                    string fullSavePath = Path.Combine(targetFolder, fileName);

                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    fuDeclaration.SaveAs(fullSavePath);

                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        const string countQuery = "SELECT COUNT(*) FROM SimilarityAICheck_Submission WHERE UserID = @UserID";
                        using (SqlCommand command = new SqlCommand(countQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", userId);
                            int existingCount = Convert.ToInt32(await command.ExecuteScalarAsync());

                            if (existingCount > 0)
                            {
                                const string updateQuery = @"
                                    UPDATE SimilarityAICheck_Submission 
                                    SET DeclarationAIPath = @Path, SubmissionDate = GETDATE()
                                    WHERE UserID = @UserID";

                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@Path", $"{UploadDirectoryPath}{fileName}");
                                    updateCommand.Parameters.AddWithValue("@UserID", userId);

                                    await updateCommand.ExecuteNonQueryAsync();
                                }

                                lblUploadStatus.Text = "✅ Declaration uploaded successfully.";
                                lblUploadStatus.ForeColor = Color.Green;
                            }
                            else
                            {
                                lblUploadStatus.Text = "⚠️ Please submit your main form before uploading the declaration.";
                                lblUploadStatus.ForeColor = Color.Orange;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblUploadStatus.Text = $"❌ Declaration upload failed: {ex.Message}";
                    lblUploadStatus.ForeColor = Color.Red;
                }
            }));
        }

        private async Task<string> SaveFileAsync(FileUpload uploadControl, string folderPath, string fullName, int userId)
        {
            string[] allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".txt", ".jpg", ".jpeg", ".png", ".bmp" };
            string fileExtension = Path.GetExtension(uploadControl.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                lblMessage.Text = $"❌ Invalid file type: {fileExtension}.";
                lblMessage.ForeColor = Color.Red;
                return null;
            }

            string cleanName = fullName.Replace(" ", "_");
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string pureFileName = Path.GetFileNameWithoutExtension(uploadControl.FileName);
            string calculatedFileName = $"{pureFileName}_{cleanName}_{timestamp}{fileExtension}";

            string destinationPath = Path.Combine(folderPath, calculatedFileName);

            // FileUpload.SaveAs is inherently synchronous, wrap inside a Task or run directly
            await Task.Run(() => uploadControl.SaveAs(destinationPath));

            return $"{UploadDirectoryPath}{calculatedFileName}";
        }

        private async Task<string> GetFullNameByUserIdAsync(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT FullName FROM Users WHERE UserID = @UserID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    await connection.OpenAsync();

                    object output = await command.ExecuteScalarAsync();
                    if (output != null && output != DBNull.Value)
                    {
                        return output.ToString();
                    }
                }
            }
            return "UnknownUser";
        }

        private void DisableSimAIWriChe()
        {
            txtTitle.Enabled = false;
            txtAuthors.Enabled = false;
            txtReceiptNo.Enabled = false;
            txtCollege.Enabled = false;
            txtOthers.Enabled = false;
            txtRevNo.Enabled = false;
            rblType.Enabled = false;
            fuDocument.Enabled = false;
            fuCopy.Enabled = false;
            fuDeclaration.Enabled = false;
            btnSubmit.Enabled = false;
            btnUploadDeclaration.Enabled = false;
        }
    }
}