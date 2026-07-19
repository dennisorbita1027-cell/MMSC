using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace CASApp1.UserModule
{
    public class StatComVal1 : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
        private const string UploadDirectoryPath = "~/App_Data/Uploads/StatComValUploads/";

        private static readonly string[] AllowedExtensions =
        {
            ".doc", ".docx", ".pdf", ".pptx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".bmp"
        };

        protected HtmlGenericControl containerDiv;
        protected Label lblPageDisabled;
        protected FileUpload fuRawData;
        protected LinkButton btnOpenModalRaw;
        protected Button btnUndoRaw;
        protected Label lblRawStatus;
        protected Label lblRawError;
        protected FileUpload fuProposal;
        protected LinkButton btnOpenModalProposal;
        protected Button btnUndoProposal;
        protected Label lblProposalStatus;
        protected Label lblProposalError;
        protected FileUpload fuInstrument;
        protected LinkButton BtnOpenModalInstrument;
        protected Button btnUndoInstrument;
        protected Label lblInstrumentStatus;
        protected Label lblInstrumentError;
        protected FileUpload fuStatOutput;
        protected LinkButton btnOpenModalStat;
        protected Button btnUndoStat;
        protected Label lblStatStatus;
        protected Label lblStatError;
        protected FileUpload fuDeclaration;
        protected LinkButton btnOpenModalDecl;
        protected Button btnUndoDecl;
        protected Label lblDeclStatus;
        protected Label lblDeclError;
        protected FileUpload fuReceipt;
        protected LinkButton btnOpenModalReceipt;
        protected Button btnUndoReceipt;
        protected Label lblReceiptStatus;
        protected Label lblReceiptError;
        protected Button btnSubmitRaw;
        protected Button btnSubmitProposal;
        protected Button btnSubmitInstrument;
        protected Button btnSubmitStat;
        protected Button btnSubmitDecl;
        protected Button btnSubmitReceipt;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(InitializeFormStateAsync));
            }
        }

        private async Task InitializeFormStateAsync()
        {
            string userIdStr = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userIdStr)) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT IsDisabled, RawDataPath, ProposalCopyPath, Instrumentation, StatOutputPath, DeclarationPath, ReceiptPath FROM StatComVal_Submission WHERE UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userIdStr);
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                bool isDisabled = reader["IsDisabled"] != DBNull.Value && Convert.ToBoolean(reader["IsDisabled"]);
                                if (isDisabled)
                                {
                                    DisableSubmission();
                                }
                                else
                                {
                                    ShowExistingFile(reader["RawDataPath"]?.ToString(), lblRawStatus, btnUndoRaw, btnOpenModalRaw);
                                    ShowExistingFile(reader["ProposalCopyPath"]?.ToString(), lblProposalStatus, btnUndoProposal, btnOpenModalProposal);
                                    ShowExistingFile(reader["Instrumentation"]?.ToString(), lblInstrumentStatus, btnUndoInstrument, BtnOpenModalInstrument);
                                    ShowExistingFile(reader["StatOutputPath"]?.ToString(), lblStatStatus, btnUndoStat, btnOpenModalStat);
                                    ShowExistingFile(reader["DeclarationPath"]?.ToString(), lblDeclStatus, btnUndoDecl, btnOpenModalDecl);
                                    ShowExistingFile(reader["ReceiptPath"]?.ToString(), lblReceiptStatus, btnUndoReceipt, btnOpenModalReceipt);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblRawError.Text = $"An error occurred initializing the page: {ex.Message}";
            }
        }

        private void ShowExistingFile(string filePath, Label statusLabel, Button undoButton, WebControl submitButton)
        {
            bool hasFile = !string.IsNullOrEmpty(filePath);
            statusLabel.Visible = hasFile;
            undoButton.Visible = hasFile;
            submitButton.Visible = !hasFile;
        }

        private async Task HandleUploadAsync(FileUpload fileUpload, string fileType, Label statusLabel, Button undoButton, Label errorLabel, WebControl submitButton)
        {
            if (!fileUpload.HasFile)
            {
                errorLabel.Text = "⚠️ Please select a file before submitting.";
                return;
            }

            string extension = Path.GetExtension(fileUpload.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
            {
                errorLabel.Text = "⚠️ Invalid file format. Please upload a supported document.";
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string cleanName = (await GetFullNameByUserIdAsync(userId)).Replace(" ", "_");

            string rawFileName = Path.GetFileNameWithoutExtension(fileUpload.FileName);
            string calculatedFileName = $"{rawFileName}_{cleanName}_{timestamp}{extension}";

            string targetFolder = Server.MapPath(UploadDirectoryPath);
            string physicalSavePath = Path.Combine(targetFolder, calculatedFileName);
            string databaseRelativePath = $"App_Data/Uploads/StatComValUploads/{calculatedFileName}";

            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                await Task.Run(() => fileUpload.SaveAs(physicalSavePath));

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string upsertQuery = @"
                        UPDATE StatComVal_Submission
                        SET {0} = @FilePath,
                            SubmissionDate = GETDATE(),
                            IsCertificationSent = 0,
                            IsRemarksSent = 0
                        WHERE UserID = @UserID;

                        IF @@ROWCOUNT = 0
                        BEGIN
                            INSERT INTO StatComVal_Submission
                            ([RawDataPath], [ProposalCopyPath], [Instrumentation], [StatOutputPath], [DeclarationPath], [ReceiptPath], [SubmissionDate], [Remarks], [UserID], [IsCertificationSent], [IsRemarksSent])
                            VALUES
                            (@RawDataPath, @ProposalCopyPath, @Instrumentation, @StatOutputPath, @DeclarationPath, @ReceiptPath, GETDATE(), '', @UserID, 0, 0)
                        END";

                    string formattedQuery = string.Format(upsertQuery, fileType);

                    using (SqlCommand command = new SqlCommand(formattedQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@FilePath", databaseRelativePath);
                        command.Parameters.AddWithValue("@RawDataPath", fileType == "RawDataPath" ? databaseRelativePath : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ProposalCopyPath", fileType == "ProposalCopyPath" ? databaseRelativePath : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Instrumentation", fileType == "Instrumentation" ? databaseRelativePath : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@StatOutputPath", fileType == "StatOutputPath" ? databaseRelativePath : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DeclarationPath", fileType == "DeclarationPath" ? databaseRelativePath : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ReceiptPath", fileType == "ReceiptPath" ? databaseRelativePath : (object)DBNull.Value);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }

                statusLabel.Visible = true;
                undoButton.Visible = true;
                errorLabel.Text = string.Empty;
                submitButton.Visible = false;
            }
            catch (Exception ex)
            {
                errorLabel.Text = $"❌ Processing error: {ex.Message}";
            }
        }

        private async Task HandleUndoAsync(string fileType, Label statusLabel, Button undoButton, WebControl submitButton)
        {
            string userIdStr = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userIdStr)) return;

            string existingFilePath = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = $"SELECT {fileType} FROM StatComVal_Submission WHERE UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userIdStr);
                        existingFilePath = (await command.ExecuteScalarAsync())?.ToString();
                    }

                    string updateQuery = $"UPDATE StatComVal_Submission SET {fileType} = NULL WHERE UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userIdStr);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                if (!string.IsNullOrEmpty(existingFilePath))
                {
                    string physicalPath = Server.MapPath($"~/{existingFilePath}");
                    if (File.Exists(physicalPath))
                    {
                        await Task.Run(() => File.Delete(physicalPath));
                    }
                }

                statusLabel.Visible = false;
                undoButton.Visible = false;
                submitButton.Visible = true;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"❌ Undo failed: {ex.Message}";
            }
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

                    object result = await command.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        return result.ToString();
                    }
                }
            }
            return "UnknownUser";
        }

        private void DisableSubmission()
        {
            fuRawData.Enabled = false;
            fuProposal.Enabled = false;
            fuInstrument.Enabled = false;
            fuStatOutput.Enabled = false;
            fuDeclaration.Enabled = false;
            fuReceipt.Enabled = false;

            btnOpenModalRaw.Visible = false;
            btnOpenModalProposal.Visible = false;
            BtnOpenModalInstrument.Visible = false;
            btnOpenModalStat.Visible = false;
            btnOpenModalDecl.Visible = false;
            btnOpenModalReceipt.Visible = false;

            btnUndoRaw.Visible = false;
            btnUndoProposal.Visible = false;
            btnUndoInstrument.Visible = false;
            btnUndoStat.Visible = false;
            btnUndoDecl.Visible = false;
            btnUndoReceipt.Visible = false;

            lblPageDisabled.Text = "Your certification has already been issued. For any questions or concerns regarding your submission, please visit the MMSC office or contact us via email at nvsu.mmsc@nvsu.edu.ph. When submitting a request or inquiry to this email, please include your full name, course, and student ID (if applicable) to ensure a prompt and accurate response.";
            lblPageDisabled.Visible = true;
            containerDiv.Style["background-color"] = "#d3d3d3";
        }

        protected void btnSubmitRaw_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUploadAsync(fuRawData, "RawDataPath", lblRawStatus, btnUndoRaw, lblRawError, btnOpenModalRaw)));

        protected void btnSubmitProposal_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUploadAsync(fuProposal, "ProposalCopyPath", lblProposalStatus, btnUndoProposal, lblProposalError, btnOpenModalProposal)));

        protected void btnSubmitInstrument_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUploadAsync(fuInstrument, "Instrumentation", lblInstrumentStatus, btnUndoInstrument, lblInstrumentError, BtnOpenModalInstrument)));

        protected void btnSubmitStat_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUploadAsync(fuStatOutput, "StatOutputPath", lblStatStatus, btnUndoStat, lblStatError, btnOpenModalStat)));

        protected void btnSubmitDecl_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUploadAsync(fuDeclaration, "DeclarationPath", lblDeclStatus, btnUndoDecl, lblDeclError, btnOpenModalDecl)));

        protected void btnSubmitReceipt_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUploadAsync(fuReceipt, "ReceiptPath", lblReceiptStatus, btnUndoReceipt, lblReceiptError, btnOpenModalReceipt)));

        protected void btnUndoRaw_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUndoAsync("RawDataPath", lblRawStatus, btnUndoRaw, btnOpenModalRaw)));

        protected void btnUndoProposal_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUndoAsync("ProposalCopyPath", lblProposalStatus, btnUndoProposal, btnOpenModalProposal)));

        protected void btnUndoInstrument_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUndoAsync("Instrumentation", lblInstrumentStatus, btnUndoInstrument, BtnOpenModalInstrument)));

        protected void btnUndoStat_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUndoAsync("StatOutputPath", lblStatStatus, btnUndoStat, btnOpenModalStat)));

        protected void btnUndoDecl_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUndoAsync("DeclarationPath", lblDeclStatus, btnUndoDecl, btnOpenModalDecl)));

        protected void btnUndoReceipt_Click(object sender, EventArgs e) =>
            RegisterAsyncTask(new PageAsyncTask(() => HandleUndoAsync("ReceiptPath", lblReceiptStatus, btnUndoReceipt, btnOpenModalReceipt)));
    }
}