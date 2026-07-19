using Novacode;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace CASApp1.ProcessorModule
{
    public class StatComValRemarks : Page
    {
        protected Repeater rptRemarks;
        protected Button btnConfirmReturn;
        protected HiddenField hfReturnArg;
        protected Button btnPrev;
        protected Label lblPageInfo;
        protected Button btnNext;

        private int PageSize => 20;

        private int CurrentPage
        {
            get => ViewState[nameof(CurrentPage)] == null ? 0 : (int)ViewState[nameof(CurrentPage)];
            set => ViewState[nameof(CurrentPage)] = value;
        }

        // Safe whitelist for columns allowed to be modified via dynamic query builder
        private static readonly HashSet<string> AllowedReturnColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "RawDataPath", "ProposalCopyPath", "Instrumentation", "StatOutputPath", "DeclarationPath", "ReceiptPath"
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Register asynchronous task for initial data binding loop
                RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
            }
        }

        private async Task LoadRemarksDataAsync()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    const string query = @"
                        SELECT *
                        FROM StatComVal_Submission s
                        LEFT JOIN Users u ON s.UserID = u.UserID";

                    using (SqlCommand selectCommand = new SqlCommand(query, connection))
                    {s
                        using (SqlDataAdapter adapter = new SqlDataAdapter(selectCommand))
                        {
                            DataTable rawDataTable = new DataTable();
                            adapter.Fill(rawDataTable);

                            if (!rawDataTable.Columns.Contains("CertificationExists"))
                            {
                                rawDataTable.Columns.Add("CertificationExists", typeof(bool));
                            }

                            // Evaluate file existence checks locally
                            foreach (DataRow row in rawDataTable.Rows)
                            {
                                string certPath = Server.MapPath($"~/App_Data/CertReports/StatComCert/Certification_{row["SubmissionID"]}.docx");
                                row["CertificationExists"] = File.Exists(certPath);
                            }

                            Func<string, bool> hasValue = val => !string.IsNullOrWhiteSpace(val);

                            Func<DataRow, bool> isComplete = r =>
                                hasValue(r.Field<string>("RawDataPath")) &&
                                hasValue(r.Field<string>("ProposalCopyPath")) &&
                                hasValue(r.Field<string>("Instrumentation")) &&
                                hasValue(r.Field<string>("StatOutputPath")) &&
                                hasValue(r.Field<string>("DeclarationPath")) &&
                                hasValue(r.Field<string>("ReceiptPath"));

                            Func<DataRow, bool> isFullyReturned = r =>
                                !hasValue(r.Field<string>("RawDataPath")) &&
                                !hasValue(r.Field<string>("ProposalCopyPath")) &&
                                !hasValue(r.Field<string>("Instrumentation")) &&
                                !hasValue(r.Field<string>("StatOutputPath")) &&
                                !hasValue(r.Field<string>("DeclarationPath")) &&
                                !hasValue(r.Field<string>("ReceiptPath"));

                            // Apply conditional filtering loops
                            var filteredRows = rawDataTable.AsEnumerable()
                                .Where(r => r.Field<bool?>("IsHighlighted").GetValueOrDefault() || !isFullyReturned(r));

                            DataTable filteredTable = filteredRows.Any() ? filteredRows.CopyToDataTable() : rawDataTable.Clone();

                            // Maintain identical historical priority sorts
                            var sortedRows = filteredTable.AsEnumerable()
                                .OrderBy(r => r.Field<bool?>("IsDisabled").GetValueOrDefault() ? 1 : 0)
                                .ThenBy(r => !r.Field<bool?>("IsHighlighted").GetValueOrDefault() ? 1 : 0)
                                .ThenBy(r => !isComplete(r) ? 1 : 0)
                                .ThenByDescending(CountUploadedFiles)
                                .ThenByDescending(r => r.Field<DateTime?>("SubmissionDate") ?? DateTime.MinValue)
                                .ThenByDescending(r => r.Field<int?>("SubmissionID").GetValueOrDefault());

                            DataTable sortedTable = sortedRows.Any() ? sortedRows.CopyToDataTable() : filteredTable.Clone();

                            int totalRows = sortedTable.Rows.Count;
                            int totalPages = (int)Math.Ceiling((double)totalRows / PageSize);
                            int startRowIndex = CurrentPage * PageSize;

                            DataTable pagedTable = sortedTable.Clone();
                            for (int i = startRowIndex; i < startRowIndex + PageSize && i < totalRows; i++)
                            {
                                pagedTable.ImportRow(sortedTable.Rows[i]);
                            }

                            rptRemarks.DataSource = pagedTable;
                            rptRemarks.DataBind();

                            btnPrev.Enabled = CurrentPage > 0;
                            btnNext.Enabled = CurrentPage < totalPages - 1;
                            lblPageInfo.Text = totalPages > 0 ? $"Page {Math.Min(CurrentPage + 1, totalPages)} of {totalPages}" : "Page 0 of 0";

                            if (ViewState["OpenModalID"] != null)
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "OpenModalScript", $"showModal({ViewState["OpenModalID"]});", true);
                                ViewState["OpenModalID"] = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('Error loading data. Please contact admin.');", true);
            }
        }

        protected void rptRemarks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string submissionIdString = e.CommandArgument.ToString();

            if (e.CommandName == "SubmitRemarks")
            {
                FileUpload fuRemarksUpload = (FileUpload)e.Item.FindControl("fuRemarksUpload");
                Label lblStatus = (Label)e.Item.FindControl("lblStatus");

                if (fuRemarksUpload != null && fuRemarksUpload.HasFile)
                {
                    string virtualPath = $"~/App_Data/CertReports/StatComCert/Certification_{submissionIdString}.docx";
                    string physicalPath = Server.MapPath(virtualPath);
                    fuRemarksUpload.SaveAs(physicalPath);

                    RegisterAsyncTask(new PageAsyncTask(async () =>
                    {
                        string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
                        using (SqlConnection connection = new SqlConnection(connString))
                        {
                            const string updateQuery = "UPDATE StatComVal_Submission SET Remarks = @Remarks, IsRemarksSent = 1 WHERE SubmissionID = @ID";
                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                            {
                                command.Parameters.AddWithValue("@Remarks", virtualPath);
                                command.Parameters.AddWithValue("@ID", submissionIdString);
                                await connection.OpenAsync();
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        Button btnSubmitRemarks = (Button)e.Item.FindControl("Button1");
                        if (btnSubmitRemarks != null)
                        {
                            btnSubmitRemarks.Text = "Sent";
                            btnSubmitRemarks.ForeColor = Color.Black;
                            btnSubmitRemarks.BackColor = Color.LightGray;
                        }

                        if (lblStatus != null)
                        {
                            lblStatus.Text = "";
                            lblStatus.ForeColor = Color.Black;
                        }
                    }));
                    return;
                }

                if (lblStatus != null)
                {
                    lblStatus.Text = "⚠️ Please choose a file.";
                    lblStatus.ForeColor = Color.Red;
                }
                RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
            }
            else if (e.CommandName == "ReturnCertification")
            {
                Label lblReturnStatus = (Label)e.Item.FindControl("lblReturnStatus");
                if (lblReturnStatus != null)
                {
                    lblReturnStatus.Text = "Sending...";
                    lblReturnStatus.ForeColor = Color.Orange;
                }

                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
                    string relativePath = "";
                    string studentEmail = "";

                    using (SqlConnection connection = new SqlConnection(connString))
                    {
                        const string selectQuery = @"
                            SELECT s.Remarks, u.Email
                            FROM StatComVal_Submission s
                            INNER JOIN Users u ON s.UserID = u.UserID
                            WHERE s.SubmissionID = @ID";

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@ID", submissionIdString);
                            await connection.OpenAsync();
                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    relativePath = reader["Remarks"]?.ToString();
                                    studentEmail = reader["Email"]?.ToString();
                                }
                            }
                        }
                    }

                    if (lblReturnStatus == null) return;

                    if (!string.IsNullOrEmpty(relativePath) && !string.IsNullOrEmpty(studentEmail))
                    {
                        string fullPath = Server.MapPath(relativePath);
                        if (File.Exists(fullPath))
                        {
                            bool emailSent = await SendEmailWithAttachmentAsync(studentEmail, "Certification", "Please find attached your certification remarks.", fullPath);
                            if (emailSent)
                            {
                                lblReturnStatus.Text = "✔️ Sent";
                                lblReturnStatus.ForeColor = Color.Green;
                            }
                            else
                            {
                                lblReturnStatus.Text = "❌ Failed to send/recipient not found.";
                                lblReturnStatus.ForeColor = Color.Red;
                            }
                        }
                        else
                        {
                            lblReturnStatus.Text = "❌ File not found.";
                            lblReturnStatus.ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        lblReturnStatus.Text = "❌ Missing file or email.";
                        lblReturnStatus.ForeColor = Color.Red;
                    }
                }));
            }
            else if (e.CommandName == "ReturnFile")
            {
                string[] arguments = e.CommandArgument.ToString().Split(';');
                if (arguments.Length == 2)
                {
                    RegisterAsyncTask(new PageAsyncTask(() => ReturnFileAsync(arguments[0], arguments[1])));
                }
            }
            else if (e.CommandName == "GenerateCertificate")
            {
                HiddenField hiddenId = (HiddenField)e.Item.FindControl("HiddenSubmissionID");
                if (hiddenId != null)
                {
                    ViewState["OpenCertFormSubmissionID"] = hiddenId.Value;
                }
                RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
            }
        }

        protected void rptRemarks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            CheckBox chkDisable = (CheckBox)e.Item.FindControl("chkDisable");
            HtmlTableRow tableRow = (HtmlTableRow)e.Item.FindControl("Tr1");

            if (tableRow == null) return;

            DataRowView dataItem = (DataRowView)e.Item.DataItem;
            bool isDisabled = dataItem["IsDisabled"] != DBNull.Value && Convert.ToBoolean(dataItem["IsDisabled"]);
            bool isHighlighted = dataItem["IsHighlighted"] != DBNull.Value && Convert.ToBoolean(dataItem["IsHighlighted"]);

            if (isDisabled)
            {
                tableRow.Attributes["style"] = "background-color: #e2e2e2; color: white;";
            }
            else if (isHighlighted)
            {
                tableRow.Attributes["style"] = "background-color: #fffbb6;";
            }

            if (chkDisable != null && chkDisable.Checked)
            {
                DisableControlsRecursiveExclude(tableRow, true, chkDisable);
            }
        }

        protected void btnGenerateCert_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
        }

        protected void btnSubmitCert_Click(object sender, EventArgs e)
        {
            RepeaterItem containerItem = (RepeaterItem)((Control)sender).NamingContainer;
            int submissionId = int.Parse(((HiddenField)containerItem.FindControl("HiddenSubmissionID")).Value);
            string authorNameText = ((TextBox)containerItem.FindControl("txtAuthors")).Text.Trim();
            string projectTitleText = ((TextBox)containerItem.FindControl("txtTitle")).Text.Trim();
            string statisticianNameText = ((TextBox)containerItem.FindControl("txtStatistician")).Text.Trim();
            string certificateDateText = ((TextBox)containerItem.FindControl("txtCertDate")).Text.Trim();

            // Sanitize file tokens safely
            string safeAuthorToken = new string(Regex.Replace(authorNameText, $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "_").Take(20).ToArray());
            string timeStampString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string certificateFileName = $"Certification_{submissionId}_{safeAuthorToken}_{timeStampString}.docx";
            string relativeCertificatePath = $"~/App_Data/CertReports/StatComCert/{certificateFileName}";
            string physicalCertificatePath = Server.MapPath(relativeCertificatePath);

            using (DocX templateDoc = DocX.Load(Server.MapPath("~/Templates/StatComAnalyses.docx")))
            {
                templateDoc.ReplaceText("{{Author/s}}", authorNameText);
                templateDoc.ReplaceText("{{Title}}", projectTitleText);
                templateDoc.ReplaceText("{{Statistician}}", statisticianNameText);
                templateDoc.ReplaceText("{{Date}}", certificateDateText);
                templateDoc.SaveAs(physicalCertificatePath);
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
                int studentUserId = await GetUserIDFromSubmissionAsync(submissionId);
                string destinationEmail = "";

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    await connection.OpenAsync();

                    const string updateQuery = "UPDATE StatComVal_Submission SET Remarks = @Remarks, IsRemarksSent = 1 WHERE SubmissionID = @ID";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Remarks", relativeCertificatePath);
                        updateCmd.Parameters.AddWithValue("@ID", submissionId);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    const string selectEmailQuery = "SELECT u.Email FROM StatComVal_Submission s INNER JOIN Users u ON s.UserID = u.UserID WHERE s.SubmissionID = @ID";
                    using (SqlCommand selectEmailCmd = new SqlCommand(selectEmailQuery, connection))
                    {
                        selectEmailCmd.Parameters.AddWithValue("@ID", submissionId);
                        object result = await selectEmailCmd.ExecuteScalarAsync();
                        destinationEmail = result?.ToString() ?? "";
                    }
                }

                bool emailSentStatus = false;
                const string systemMessage = "Your Certification has been generated and sent via email. For further assistance or concerns, please reach out at nvsu.mmsc@nvsu.edu.ph.";

                if (!string.IsNullOrEmpty(destinationEmail))
                {
                    emailSentStatus = await SendEmailWithAttachmentAsync(destinationEmail, "Certification", systemMessage, physicalCertificatePath);
                }

                if (emailSentStatus)
                {
                    using (SqlConnection connection = new SqlConnection(connString))
                    {
                        int dynamicProcessorId = Convert.ToInt32(Session["UserID"]);
                        const string logInsertQuery = @"
                            INSERT INTO ProcessorReturnedReports 
                            (SubmissionID, UserID, ModuleType, StatComCertPath, MessageToStudent, DateSent, Author, Title, SentByProcessorID, FilesSent)
                            VALUES (@SubmissionID, @UserID, 'StatComVal', @StatComCertPath, @Message, GETDATE(), @Author, @Title, @ProcessorID, @FilesSent)";

                        using (SqlCommand insertCmd = new SqlCommand(logInsertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@SubmissionID", submissionId);
                            insertCmd.Parameters.AddWithValue("@UserID", studentUserId);
                            insertCmd.Parameters.AddWithValue("@StatComCertPath", relativeCertificatePath);
                            insertCmd.Parameters.AddWithValue("@Message", systemMessage);
                            insertCmd.Parameters.AddWithValue("@Author", authorNameText);
                            insertCmd.Parameters.AddWithValue("@Title", projectTitleText);
                            insertCmd.Parameters.AddWithValue("@ProcessorID", dynamicProcessorId);
                            insertCmd.Parameters.AddWithValue("@FilesSent", Path.GetFileName(relativeCertificatePath));

                            await connection.OpenAsync();
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                Button btnGenerateCert = (Button)containerItem.FindControl("btnGenerateCert");
                if (btnGenerateCert != null)
                {
                    btnGenerateCert.Text = "📝 Create Certificate";
                    btnGenerateCert.Enabled = true;
                }

                Label lblUploading = (Label)containerItem.FindControl("lblUploading");
                if (lblUploading != null)
                {
                    lblUploading.Style["display"] = "none";
                }

                Label lblStatus = (Label)containerItem.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    lblStatus.Text = "Uploaded ✔️";
                    lblStatus.ToolTip = certificateFileName;
                    lblStatus.ForeColor = Color.Green;
                    lblStatus.Attributes.Add("style", "display:inline-block;margin-left:6px;font-weight:bold;");
                }

                await LoadRemarksDataAsync();
            }));
        }

        protected void btnUploadCert_Click(object sender, EventArgs e)
        {
            RepeaterItem containerItem = (RepeaterItem)((Control)sender).NamingContainer;
            int submissionId = int.Parse(((HiddenField)containerItem.FindControl("HiddenSubmissionID")).Value);
            FileUpload fuCertUpload = (FileUpload)containerItem.FindControl("fuCertUpload");

            if (fuCertUpload.HasFile)
            {
                string fileExtension = Path.GetExtension(fuCertUpload.FileName).ToLower();
                if (fileExtension != ".docx" && fileExtension != ".pdf")
                    return;

                string sanitizedBaseName = Regex.Replace(fuCertUpload.FileName, $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "_");
                string uniqueTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                // Added a short unique sequence to prevent overwrites on fast concurrent executions
                string distinctIdSuffix = Guid.NewGuid().ToString("N").Substring(0, 4);
                string outputFileName = $"{Path.GetFileNameWithoutExtension(sanitizedBaseName)}_{uniqueTimestamp}_{distinctIdSuffix}{fileExtension}";
                string relativeCertOutputPath = $"~/App_Data/CertReports/StatComCert/{outputFileName}";
                string physicalCertOutputPath = Server.MapPath(relativeCertOutputPath);

                fuCertUpload.SaveAs(physicalCertOutputPath);

                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
                    int studentUserId = await GetUserIDFromSubmissionAsync(submissionId);
                    string targetEmail = "";

                    using (SqlConnection connection = new SqlConnection(connString))
                    {
                        await connection.OpenAsync();

                        const string updateQuery = "UPDATE StatComVal_Submission SET Remarks = @Remarks, IsRemarksSent = 1 WHERE SubmissionID = @ID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@Remarks", relativeCertOutputPath);
                            updateCmd.Parameters.AddWithValue("@ID", submissionId);
                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        const string selectEmailQuery = "SELECT u.Email FROM StatComVal_Submission s INNER JOIN Users u ON s.UserID = u.UserID WHERE s.SubmissionID = @ID";
                        using (SqlCommand selectEmailCmd = new SqlCommand(selectEmailQuery, connection))
                        {
                            selectEmailCmd.Parameters.AddWithValue("@ID", submissionId);
                            object emailObject = await selectEmailCmd.ExecuteScalarAsync();
                            targetEmail = emailObject?.ToString() ?? "";
                        }
                    }

                    bool processedEmailFlag = false;
                    const string genericMessageText = "Your Certification has been generated and sent via email. For further assistance or concerns, please reach out at nvsu.mmsc@nvsu.edu.ph.";

                    if (!string.IsNullOrEmpty(targetEmail))
                    {
                        processedEmailFlag = await SendEmailWithAttachmentAsync(targetEmail, "Certification", genericMessageText, physicalCertOutputPath);
                    }

                    if (processedEmailFlag)
                    {
                        using (SqlConnection connection = new SqlConnection(connString))
                        {
                            int processorUserIdToken = Convert.ToInt32(Session["UserID"]);
                            const string archiveInsertQuery = @"
                                INSERT INTO ProcessorReturnedReports 
                                (SubmissionID, UserID, ModuleType, StatComCertPath, MessageToStudent, DateSent, Author, Title, SentByProcessorID, FilesSent)
                                VALUES (@SubmissionID, @UserID, 'StatComVal', @StatComCertPath, @Message, GETDATE(), NULL, NULL, @ProcessorID, @FilesSent)";

                            using (SqlCommand logInsertCmd = new SqlCommand(archiveInsertQuery, connection))
                            {
                                logInsertCmd.Parameters.AddWithValue("@SubmissionID", submissionId);
                                logInsertCmd.Parameters.AddWithValue("@UserID", studentUserId);
                                logInsertCmd.Parameters.AddWithValue("@StatComCertPath", relativeCertOutputPath);
                                logInsertCmd.Parameters.AddWithValue("@Message", genericMessageText);
                                logInsertCmd.Parameters.AddWithValue("@ProcessorID", processorUserIdToken);
                                logInsertCmd.Parameters.AddWithValue("@FilesSent", Path.GetFileName(relativeCertOutputPath));

                                await connection.OpenAsync();
                                await logInsertCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    Label lblStatus = (Label)containerItem.FindControl("lblStatus");
                    if (lblStatus != null)
                    {
                        lblStatus.Text = "Uploaded ✔️";
                        lblStatus.ToolTip = outputFileName;
                        lblStatus.ForeColor = Color.Green;
                        lblStatus.Attributes.Add("style", "display:inline-block;margin-left:6px;font-weight:bold;");
                    }

                    await LoadRemarksDataAsync();
                }));
            }
        }

        protected void ConfirmReturn_Click(object sender, EventArgs e)
        {
            string payloadString = hfReturnArg.Value;
            if (!string.IsNullOrEmpty(payloadString))
            {
                string[] separateTokens = payloadString.Split(';');
                if (separateTokens.Length == 2)
                {
                    RegisterAsyncTask(new PageAsyncTask(() => ReturnFileAsync(separateTokens[0], separateTokens[1])));
                    return;
                }
            }
            RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
        }

        private async Task ReturnFileAsync(string submissionId, string columnToUpdate)
        {
            // Security Fix: Prevent SQL injection by validating dynamic string tokens against an established whitelist block
            if (!AllowedReturnColumns.Contains(columnToUpdate))
            {
                throw new InvalidOperationException($"Unauthorized field modification processing request flagged: {columnToUpdate}");
            }

            string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connString))
            {
                string purgeQuery = $"UPDATE StatComVal_Submission SET {columnToUpdate} = NULL WHERE SubmissionID = @ID";
                using (SqlCommand purgeCommand = new SqlCommand(purgeQuery, connection))
                {
                    purgeCommand.Parameters.AddWithValue("@ID", submissionId);
                    await connection.OpenAsync();
                    await purgeCommand.ExecuteNonQueryAsync();
                }
            }
            await LoadRemarksDataAsync();
        }

        protected void highlight_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkHighlight = (CheckBox)sender;
            string submissionIdValue = ((HiddenField)chkHighlight.NamingContainer.FindControl("HiddenSubmissionID")).Value;
            bool highLightStateChecked = chkHighlight.Checked;

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    const string updateQuery = "UPDATE StatComVal_Submission SET IsHighlighted = @IsHighlighted WHERE SubmissionID = @SubmissionID";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IsHighlighted", highLightStateChecked);
                        command.Parameters.AddWithValue("@SubmissionID", submissionIdValue);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await LoadRemarksDataAsync();
            }));
        }

        protected void Disable_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkDisable = (CheckBox)sender;
            HiddenField hiddenId = (HiddenField)chkDisable.NamingContainer.FindControl("HiddenSubmissionID");

            if (hiddenId == null) return;

            int targetSubmissionId = int.Parse(hiddenId.Value);
            bool disableStateChecked = chkDisable.Checked;

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    const string updateQuery = "UPDATE StatComVal_Submission SET IsDisabled = @IsDisabled WHERE SubmissionID = @ID";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@IsDisabled", disableStateChecked);
                        command.Parameters.AddWithValue("@ID", targetSubmissionId);
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await LoadRemarksDataAsync();
            }));
        }

        private void DisableControlsRecursiveExclude(Control parentControl, bool isDisabled, Control excludeControl)
        {
            foreach (Control internalControl in parentControl.Controls)
            {
                if (internalControl == excludeControl) continue;

                if (internalControl is WebControl interactiveWebControl)
                {
                    interactiveWebControl.Enabled = !isDisabled;
                }

                if (internalControl.HasControls())
                {
                    DisableControlsRecursiveExclude(internalControl, isDisabled, excludeControl);
                }
            }
        }

        private async Task<int> GetUserIDFromSubmissionAsync(int submissionId)
        {
            string connString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connString))
            {
                const string query = "SELECT UserID FROM StatComVal_Submission WHERE SubmissionID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", submissionId);
                    await connection.OpenAsync();
                    object scalarResult = await command.ExecuteScalarAsync();
                    return scalarResult != null ? Convert.ToInt32(scalarResult) : 0;
                }
            }
        }

        private async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string emailSubject, string emailBody, string fileAttachmentPath)
        {
            try
            {
                if (!File.Exists(fileAttachmentPath)) return false;

                using (MailMessage mailMessage = new MailMessage())
                {
                    string systemSenderEmail = ConfigurationManager.AppSettings["SmtpEmail"];
                    mailMessage.From = new MailAddress(systemSenderEmail);
                    mailMessage.To.Add(toEmail);
                    mailMessage.Subject = emailSubject;
                    mailMessage.Body = emailBody;
                    mailMessage.Attachments.Add(new Attachment(fileAttachmentPath));

                    // Fixed open logic lifecycle bug by leveraging explicit using blocks for SmtpClient context
                    using (SmtpClient mailClient = new SmtpClient("smtp.gmail.com", 587))
                    {
                        string systemSenderPassword = ConfigurationManager.AppSettings["SmtpPassword"];
                        mailClient.Credentials = new NetworkCredential(systemSenderEmail, systemSenderPassword);
                        mailClient.EnableSsl = true;

                        await mailClient.SendMailAsync(mailMessage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (CurrentPage <= 0) return;
            CurrentPage--;
            RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            RegisterAsyncTask(new PageAsyncTask(LoadRemarksDataAsync));
        }

        private int CountUploadedFiles(DataRow dataRow)
        {
            string[] fileVerificationColumns = { "RawDataPath", "ProposalCopyPath", "Instrumentation", "StatOutputPath", "DeclarationPath", "ReceiptPath" };
            return fileVerificationColumns.Count(columnName =>
                dataRow.Table.Columns.Contains(columnName) &&
                !string.IsNullOrEmpty(dataRow[columnName]?.ToString()));
        }

        public static class Logger
        {
            public static void LogError(Exception exception)
            {
                try
                {
                    string trackingLogDirectory = HttpContext.Current.Server.MapPath("~/App_Data/ErrorLog.txt");
                    using (StreamWriter logWriter = new StreamWriter(trackingLogDirectory, true))
                    {
                        logWriter.WriteLine($"=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                        logWriter.WriteLine($"Message: {exception.Message}");
                        logWriter.WriteLine($"Stack Trace: {exception.StackTrace}");
                        if (exception.InnerException != null)
                        {
                            logWriter.WriteLine($"Inner: {exception.InnerException.Message}");
                        }
                        logWriter.WriteLine();
                    }
                }
                catch
                {
                    // Fail silently to prevent recursive crash cascades within local diagnostics handler
                }
            }
        }
    }
}