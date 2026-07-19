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
using Novacode;

namespace CASApp1.ProcessorModule
{
    public class SimAiWriCheRemarks : Page
    {
        private static readonly string ConnStr = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;

        protected TextBox txtSearch;
        protected Button btnSearch;
        protected Button btnClear;
        protected Panel pnlSearchResults;
        protected Repeater rptSearchResults;
        protected Repeater rptSimAICheck;
        protected Button btnPrev;
        protected Label lblPageInfo;
        protected Button btnNext;

        private int PageSize => 20;

        private int CurrentPage
        {
            get => ViewState[nameof(CurrentPage)] == null ? 0 : (int)ViewState[nameof(CurrentPage)];
            set => ViewState[nameof(CurrentPage)] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/LogInPage/LoginPage.aspx");
                return;
            }

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(() => LoadRemarksDataAsync()));
            }
        }

        private async Task LoadSearchResultsAsync(string keyword)
        {
            using (var connection = new SqlConnection(ConnStr))
            {
                const string query = @"
                    SELECT * FROM SimilarityAICheck_Submission
                    WHERE Title LIKE @keyword OR Author LIKE @keyword
                    ORDER BY SubmissionDate DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@keyword", $"%{keyword}%");
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        rptSimAICheck.DataSource = dataTable;
                        rptSimAICheck.DataBind();
                    }
                }
            }
        }

        private async Task LoadRemarksDataAsync()
        {
            using (var connection = new SqlConnection(ConnStr))
            {
                const string query = @"
                    SELECT SubmissionID, Title, Author, ReceiptNo, CopyFilePath, DocumentFilePath, RevisionNo, SubmissionDate, 
                           SimilarityRemarks, AIRemarks, MMSCReportPath, UserID, IsSubmitted, MMSCCertificationPath, 
                           IsReportsEmailed, DeclarationAIPath, IsHighlighted, IsFlagged, IsDisabled, IsMarked, College, 
                           DeclarationTemplatePath, Type
                    FROM SimilarityAICheck_Submission
                    ORDER BY
                        CASE 
                            WHEN IsFlagged = 1 THEN 0         
                            WHEN IsHighlighted = 1 THEN 1     
                            WHEN IsDisabled = 1 THEN 3        
                            ELSE 2                                            
                        END,
                        RevisionNo ASC,
                        SubmissionDate DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);

                        int totalRows = dataTable.Rows.Count;
                        int totalPages = (int)Math.Ceiling((double)totalRows / PageSize);
                        int startIndex = CurrentPage * PageSize;

                        var pageTable = dataTable.Clone();
                        for (int i = startIndex; i < startIndex + PageSize && i < totalRows; i++)
                        {
                            pageTable.ImportRow(dataTable.Rows[i]);
                        }

                        rptSimAICheck.DataSource = pageTable;
                        rptSimAICheck.DataBind();

                        btnPrev.Enabled = CurrentPage > 0;
                        btnNext.Enabled = CurrentPage < totalPages - 1;
                        lblPageInfo.Text = $"Page {CurrentPage + 1} of {Math.Max(1, totalPages)}";
                    }
                }
            }
        }

        protected void rptSearch_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            rptSimAICheck_ItemCommand(source, e);
        }

        protected void rptSimAICheck_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument?.ToString(), out int submissionId))
                return;

            if (e.CommandName == "SubmitSimilarity")
            {
                var fuSimilarity = (FileUpload)e.Item.FindControl("fuSimilarity");
                if (fuSimilarity != null && fuSimilarity.HasFile)
                {
                    RegisterAsyncTask(new PageAsyncTask(() => HandleFileUploadAsync(submissionId, fuSimilarity, "SimilarityReport", "SimilarityRemarks", "LastUploadedSim")));
                }
            }
            else if (e.CommandName == "SubmitAIWriting")
            {
                var fuAIWriting = (FileUpload)e.Item.FindControl("fuAIWriting");
                if (fuAIWriting != null && fuAIWriting.HasFile)
                {
                    RegisterAsyncTask(new PageAsyncTask(() => HandleFileUploadAsync(submissionId, fuAIWriting, "AIReport", "AIRemarks", "LastUploadedAI")));
                }
            }
        }

        private async Task HandleFileUploadAsync(int submissionId, FileUpload fileUpload, string prefix, string dbColumn, string viewStateKey)
        {
            string authorName = await GetAuthorFromSubmissionIdAsync(submissionId);
            string savedPath = SaveFile(fileUpload, authorName, prefix);

            await UpdateRemarksFilePathAsync(submissionId, dbColumn, savedPath);
            ViewState[viewStateKey] = submissionId;

            if (Session["SearchKeyword"] is string keyword && !string.IsNullOrEmpty(keyword))
            {
                await LoadSearchResultsAsync(keyword);
            }
            else
            {
                await LoadRemarksDataAsync();
            }
        }

        protected void rptSearch_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            rptSimAICheck_ItemDataBound(sender, e);
        }

        protected void rptSimAICheck_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var dataItem = (DataRowView)e.Item.DataItem;
            int submissionId = Convert.ToInt32(dataItem["SubmissionID"]);

            // Bind textual descriptions safely
            BindTextBoxValue(e.Item, "txtMMSC_Title", dataItem["Title"]);
            BindTextBoxValue(e.Item, "txtMMSC_Author", dataItem["Author"]);
            BindTextBoxValue(e.Item, "txtCertTitle", dataItem["Title"]);
            BindTextBoxValue(e.Item, "txtCertAuthor", dataItem["Author"]);

            // Dynamic layout formatting states
            EvaluateUploadStatus(e.Item, "lblSimFile", "lblSimStatus", dataItem["SimilarityRemarks"]?.ToString(), "LastUploadedSim", submissionId);
            EvaluateUploadStatus(e.Item, "lblAIFile", "lblAIStatus", dataItem["AIRemarks"]?.ToString(), "LastUploadedAI", submissionId);

            var btnMMSCModal = (Button)e.Item.FindControl("btnMMSCModal");
            if (btnMMSCModal != null)
            {
                btnMMSCModal.Text = "📝 Upload";
                btnMMSCModal.Enabled = true;
            }

            var rblTemplates = (RadioButtonList)e.Item.FindControl("rblTemplates");
            if (rblTemplates != null)
            {
                rblTemplates.RepeatLayout = RepeatLayout.Table;
                rblTemplates.RepeatDirection = RepeatDirection.Horizontal;
                rblTemplates.CssClass = "template-selector";
            }

            var rowContainer = (HtmlTableRow)e.Item.FindControl("rowContainer");
            var chkHighlight = (CheckBox)e.Item.FindControl("chkHighlight");
            var chkDisable = (CheckBox)e.Item.FindControl("chkDisable");
            var chkFlagged = (CheckBox)e.Item.FindControl("chkFlagged");

            bool isFlagged = dataItem["IsFlagged"] != DBNull.Value && Convert.ToBoolean(dataItem["IsFlagged"]);
            if (chkFlagged != null) chkFlagged.Checked = isFlagged;

            if (chkHighlight != null && dataItem["IsHighlighted"] != DBNull.Value)
                chkHighlight.Checked = Convert.ToBoolean(dataItem["IsHighlighted"]);

            if (chkDisable != null && dataItem["IsDisabled"] != DBNull.Value)
                chkDisable.Checked = Convert.ToBoolean(dataItem["IsDisabled"]);

            string rowClass = "";
            if (rowContainer != null)
            {
                if (isFlagged)
                {
                    rowContainer.Style["background-color"] = "LightBlue";
                }
                else
                {
                    if (chkDisable != null && chkDisable.Checked) rowClass = "disabled-row";
                    else if (chkHighlight != null && chkHighlight.Checked) rowClass = "highlight-row";
                    rowContainer.Attributes["class"] = rowClass;
                }
            }

            if (btnMMSCModal != null && chkDisable != null)
            {
                btnMMSCModal.Enabled = !chkDisable.Checked;
            }
        }

        private void BindTextBoxValue(RepeaterItem item, string controlId, object value)
        {
            if (item.FindControl(controlId) is TextBox textBox && value != DBNull.Value)
            {
                textBox.Text = value.ToString();
            }
        }

        private void EvaluateUploadStatus(RepeaterItem item, string labelId, string statusId, string filePath, string viewStateKey, int submissionId)
        {
            var lblFile = (Label)item.FindControl(labelId);
            var lblStatus = (Label)item.FindControl(statusId);

            if (lblFile == null || lblStatus == null) return;

            if (!string.IsNullOrEmpty(filePath))
            {
                lblFile.CssClass = "upload-status";
                lblFile.Text = "Uploaded";
                lblFile.ToolTip = Path.GetFileName(filePath);
                lblFile.Visible = true;

                if (ViewState[viewStateKey] != null && Convert.ToInt32(ViewState[viewStateKey]) == submissionId)
                {
                    lblStatus.Text = "✔️";
                    lblStatus.Visible = true;
                }
                else
                {
                    lblStatus.Text = string.Empty;
                    lblStatus.Visible = false;
                }
            }
            else
            {
                lblFile.Visible = false;
                lblStatus.Text = string.Empty;
                lblStatus.Visible = false;
            }
        }

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            var item = (RepeaterItem)((Control)sender).NamingContainer;
            var txtTitle = (TextBox)item.FindControl("txtMMSC_Title");
            var txtAuthor = (TextBox)item.FindControl("txtMMSC_Author");
            var txtDate = (TextBox)item.FindControl("txtMMSC_Date");
            var txtSimRate = (TextBox)item.FindControl("txtMMSC_SimRate");
            var txtAiRate = (TextBox)item.FindControl("txtMMSC_AIRate");
            var hfModalId = (HiddenField)item.FindControl("hfMMSCModalID");
            var hfSubmissionId = (HiddenField)item.FindControl("hfMMSCSubmissionID");
            var lblStatus = (Label)item.FindControl("lblMMSCStatus");

            int submissionId = int.Parse(hfSubmissionId.Value);
            string modalId = hfModalId?.Value;

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    string cleanTitle = txtTitle.Text.Trim();
                    string cleanAuthor = Regex.Replace(txtAuthor.Text.Trim(), $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "_");
                    string shortAuthor = new string(cleanAuthor.Take(10).ToArray());

                    string templatePath = Server.MapPath("~/Templates/MMSC_Report.docx");
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string outputDirectory = Server.MapPath("~/App_Data/CertReports/SimAIRep");

                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    string outFileName = $"MMSC_Report_{shortAuthor}_{timestamp}.docx";
                    string fullOutputPath = Path.Combine(outputDirectory, outFileName);

                    using (DocX doc = DocX.Load(templatePath))
                    {
                        doc.ReplaceText("{{Title}}", cleanTitle);
                        doc.ReplaceText("{{Author}}", cleanAuthor);
                        doc.ReplaceText("{{Date}}", txtDate.Text.Trim());
                        doc.ReplaceText("{{SimRate}}", txtSimRate.Text.Trim());
                        doc.ReplaceText("{{AIRate}}", txtAiRate.Text.Trim());
                        doc.SaveAs(fullOutputPath);
                    }

                    string dbRelativePath = "~/App_Data/CertReports/SimAIRep/" + outFileName;

                    using (var connection = new SqlConnection(ConnStr))
                    {
                        const string updateQuery = @"
                            UPDATE SimilarityAICheck_Submission 
                            SET Title = @Title, Author = @Author, MMSCReportPath = @Path 
                            WHERE SubmissionID = @SubmissionID";

                        using (var command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Title", cleanTitle);
                            command.Parameters.AddWithValue("@Author", cleanAuthor);
                            command.Parameters.AddWithValue("@Path", dbRelativePath);
                            command.Parameters.AddWithValue("@SubmissionID", submissionId);
                            await connection.OpenAsync();
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    if (item.FindControl("lblMMSCFileName") is Label lblFileName)
                    {
                        lblFileName.Text = "Uploaded";
                        lblFileName.ToolTip = outFileName;
                    }

                    string script = $"Sys.Application.add_load(function () {{ showModal('{modalId}'); }});";
                    ScriptManager.RegisterStartupScript(this, GetType(), $"ShowModal_{modalId}_MMSCReport", script, true);

                    lblStatus.Text = "Uploaded✔️.";
                    lblStatus.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ Error: " + ex.Message;
                    lblStatus.ForeColor = Color.Red;
                    ReopenModal(modalId);
                }
            }));
        }

        protected void btnSubmitMMSCCert_Click(object sender, EventArgs e)
        {
            var item = (RepeaterItem)((Control)sender).NamingContainer;
            var txtTitle = (TextBox)item.FindControl("txtCertTitle");
            var txtAuthor = (TextBox)item.FindControl("txtCertAuthor");
            var txtDate = (TextBox)item.FindControl("txtCertDate");
            var txtSimRate = (TextBox)item.FindControl("txtCertSimRate");
            var txtAiRate = (TextBox)item.FindControl("txtCertAIRate");
            var txtAiUsed = (TextBox)item.FindControl("txtCertAIUsed");
            var hfSubmissionId = (HiddenField)item.FindControl("hfMMSCCertSubmissionID");
            var hfModalId = (HiddenField)item.FindControl("hfMMSCCertModalID");
            var lblStatus = (Label)item.FindControl("lblMMSCCertStatus");
            var rbCert1 = (RadioButton)item.FindControl("rbCert1");
            var rbCert2 = (RadioButton)item.FindControl("rbCert2");

            int submissionId = int.Parse(hfSubmissionId.Value);
            string modalId = hfModalId?.Value;

            string selectedTemplate = null;
            if (rbCert1.Checked) selectedTemplate = "MMSC_Certification1.docx";
            else if (rbCert2.Checked) selectedTemplate = "MMSC_Certification2.docx";

            if (string.IsNullOrEmpty(selectedTemplate))
            {
                lblStatus.Text = "❌ Please select a certificate template.";
                lblStatus.ForeColor = Color.Red;
                ReopenModal(modalId);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    string templatePath = Server.MapPath("~/Templates/" + selectedTemplate);
                    string targetDirectory = Server.MapPath("~/App_Data/CertReports/SimAIRep/");

                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    string safeAuthor = new string(Regex.Replace(txtAuthor.Text.Trim(), $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "_").Take(10).ToArray());
                    string outFileName = $"MMSC_Cert_{safeAuthor}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                    string fullOutputPath = Path.Combine(targetDirectory, outFileName);

                    using (DocX doc = DocX.Load(templatePath))
                    {
                        doc.ReplaceText("{{Title}}", txtTitle.Text.Trim());
                        doc.ReplaceText("{{Author}}", txtAuthor.Text.Trim());
                        doc.ReplaceText("{{Date}}", txtDate.Text.Trim());
                        doc.ReplaceText("{{SimRate}}", txtSimRate.Text.Trim());
                        doc.ReplaceText("{{AIRate}}", txtAiRate.Text.Trim());

                        if (selectedTemplate == "MMSC_Certification2.docx")
                        {
                            doc.ReplaceText("{{AIUsed}}", txtAiUsed.Text.Trim());
                        }
                        doc.SaveAs(fullOutputPath);
                    }

                    string dbRelativePath = "~/App_Data/CertReports/SimAIRep/" + outFileName;

                    using (var connection = new SqlConnection(ConnStr))
                    {
                        const string query = @"
                            UPDATE SimilarityAICheck_Submission
                            SET Author = @Author, Title = @Title, MMSCCertificationPath = @Path
                            WHERE SubmissionID = @SubmissionID";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Author", txtAuthor.Text.Trim());
                            command.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                            command.Parameters.AddWithValue("@Path", dbRelativePath);
                            command.Parameters.AddWithValue("@SubmissionID", submissionId);
                            await connection.OpenAsync();
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    lblStatus.Text = "Uploaded✔️";
                    lblStatus.ForeColor = Color.Green;

                    if (item.FindControl("lblMMSCCertFile") is Label lblCertFile)
                    {
                        lblCertFile.Text = "Uploaded";
                        lblCertFile.ToolTip = outFileName;
                    }

                    string script = $"Sys.Application.add_load(function () {{ showModal('{modalId}'); }});";
                    ScriptManager.RegisterStartupScript(this, GetType(), $"ShowModal_{modalId}_MMSCCert", script, true);
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ Error: " + ex.Message;
                    lblStatus.ForeColor = Color.Red;
                    ReopenModal(modalId);
                }
            }));
        }

        protected void btnUploadMMSCCert_Click(object sender, EventArgs e)
        {
            var item = (RepeaterItem)((Control)sender).NamingContainer;
            var fileUpload = (item.FindControl("fuMMSCCert") ?? item.FindControl("fuMMSCCert1")) as FileUpload;
            var lblStatus = (item.FindControl("lblUploadMMSCCertStatus") ?? item.FindControl("lblUploadMMSCCertStatus1")) as Label;
            var hfSubmissionId = (item.FindControl("hfMMSCCertSubmissionID") ?? item.FindControl("hfMMSCCertSubmissionID1")) as HiddenField;

            if (fileUpload == null || lblStatus == null || hfSubmissionId == null) return;

            string modalId = "mmsccert_" + hfSubmissionId.Value;

            if (!fileUpload.HasFile)
            {
                lblStatus.Text = "❌ No file selected.";
                lblStatus.ForeColor = Color.Red;
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal", $"showModal('{modalId}');", true);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    string targetDirectory = Server.MapPath("~/App_Data/CertReports/SimAIRep/");
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    string sanitizedRawName = string.Join("_", Path.GetFileNameWithoutExtension(fileUpload.FileName).Split(Path.GetInvalidFileNameChars()));
                    string outFileName = $"MMSC_{sanitizedRawName}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(fileUpload.FileName)}";
                    string fullPath = Path.Combine(targetDirectory, outFileName);

                    fileUpload.SaveAs(fullPath);
                    string dbRelativePath = "~/App_Data/CertReports/SimAIRep/" + outFileName;

                    using (var connection = new SqlConnection(ConnStr))
                    {
                        const string query = "UPDATE SimilarityAICheck_Submission SET MMSCCertificationPath=@Path WHERE SubmissionID=@ID";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Path", dbRelativePath);
                            command.Parameters.AddWithValue("@ID", hfSubmissionId.Value);
                            await connection.OpenAsync();
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    if (item.FindControl("lblMMSCCertFile") is Label lblCertFile)
                    {
                        lblCertFile.Text = "Uploaded";
                        lblCertFile.ToolTip = outFileName;
                    }

                    lblStatus.Text = "✔️ File uploaded successfully.";
                    lblStatus.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ Upload failed: " + ex.Message;
                    lblStatus.ForeColor = Color.Red;
                }
                finally
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showModal", $"showModal('{modalId}');", true);
                }
            }));
        }

        protected void btnSendReturnCert_Click(object sender, EventArgs e)
        {
            var item = (RepeaterItem)((Control)sender).NamingContainer;
            var hfModalId = (HiddenField)item.FindControl("hfReturnModalID");
            var hfSubmissionId = (HiddenField)item.FindControl("hfReturnSubmissionID");
            var lblSending = (Label)item.FindControl("lblSending");
            var lblStatus = (Label)item.FindControl("lblReturnStatus");

            var rbCase1 = (RadioButton)item.FindControl("rbReturnCase1");
            var rbCase3 = (RadioButton)item.FindControl("rbReturnCase3");
            var rbCase4 = (RadioButton)item.FindControl("rbReturnCase4");

            string modalId = hfModalId?.Value;
            int submissionId = int.Parse(hfSubmissionId.Value);

            if (lblSending != null)
            {
                lblSending.Text = "📤 Sending...";
                lblSending.Visible = true;
            }

            string caseType = null;
            if (rbCase1.Checked) caseType = "Case1_2";
            else if (rbCase3.Checked) caseType = "Case3";
            else if (rbCase4.Checked) caseType = "Case4";

            if (string.IsNullOrEmpty(caseType))
            {
                if (lblSending != null) lblSending.Visible = false;
                lblStatus.Text = "❌ Please select a case.";
                lblStatus.ForeColor = Color.Red;
                ReopenModal(modalId);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    string simRemarks = "", aiRemarks = "", mmscReport = "", mmscCert = "", declAi = "", studentEmail = "", dbUserId = "";

                    using (var connection = new SqlConnection(ConnStr))
                    {
                        const string selectQuery = @"
                            SELECT s.SimilarityRemarks, s.AIRemarks, s.MMSCReportPath, s.MMSCCertificationPath,
                                   s.DeclarationAIPath, s.UserID, u.Email
                            FROM SimilarityAICheck_Submission s
                            INNER JOIN Users u ON s.UserID = u.UserID
                            WHERE s.SubmissionID = @ID";

                        using (var command = new SqlCommand(selectQuery, connection))
                        {
                            command.Parameters.AddWithValue("@ID", submissionId);
                            await connection.OpenAsync();
                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    simRemarks = reader["SimilarityRemarks"]?.ToString();
                                    aiRemarks = reader["AIRemarks"]?.ToString();
                                    mmscReport = reader["MMSCReportPath"]?.ToString();
                                    mmscCert = reader["MMSCCertificationPath"]?.ToString();
                                    declAi = reader["DeclarationAIPath"]?.ToString();
                                    studentEmail = reader["Email"]?.ToString();
                                    dbUserId = reader["UserID"]?.ToString();
                                }
                            }
                        }
                    }

                    var filesToVerify = new List<string>();
                    string emailBody = "";
                    bool isProcessValid = true;

                    switch (caseType)
                    {
                        case "Case1_2":
                            if (string.IsNullOrEmpty(simRemarks) || string.IsNullOrEmpty(aiRemarks) || string.IsNullOrEmpty(mmscReport))
                                isProcessValid = false;
                            else
                            {
                                filesToVerify.AddRange(new[] { simRemarks, aiRemarks, mmscReport });
                                emailBody = "Your Similarity and AI results with MMSC Report have been returned.";
                            }
                            break;

                        case "Case3":
                            if (string.IsNullOrEmpty(aiRemarks))
                                isProcessValid = false;
                            else
                            {
                                filesToVerify.Add(aiRemarks);
                                filesToVerify.Add("~/Templates/AIDeclaration.docx");
                                emailBody = "Your paper has passed the plagiarism test but the AI score is high. Please complete the attached Declaration of AI use.";
                            }
                            break;

                        case "Case4":
                            if (string.IsNullOrEmpty(simRemarks) || string.IsNullOrEmpty(aiRemarks) || string.IsNullOrEmpty(mmscCert))
                                isProcessValid = false;
                            else
                            {
                                filesToVerify.AddRange(new[] { simRemarks, aiRemarks, mmscCert });
                                if (!string.IsNullOrEmpty(declAi)) filesToVerify.Add(declAi);
                                emailBody = "Your Certification has been returned. Please review the attached documents. For further assistance or concerns, please reach out at nvsu.mmsc@nvsu.edu.ph\r\n.";
                            }
                            break;
                    }

                    if (!isProcessValid)
                    {
                        if (lblSending != null) lblSending.Visible = false;
                        lblStatus.Text = "❌ Cannot send. One or more required file fields are missing in the database.";
                        lblStatus.ForeColor = Color.Red;
                        ReopenModal(modalId);
                        return;
                    }

                    var exactFilePathsOnDisk = new List<string>();
                    foreach (string rawPath in filesToVerify)
                    {
                        if (string.IsNullOrWhiteSpace(rawPath)) continue;
                        string physicalPath = Server.MapPath(rawPath);
                        if (File.Exists(physicalPath) && new FileInfo(physicalPath).Length > 0)
                        {
                            exactFilePathsOnDisk.Add(physicalPath);
                        }
                    }

                    var fileNamesOnly = exactFilePathsOnDisk.Select(Path.GetFileName).ToList();

                    if (SendEmailWithMultipleAttachments(studentEmail, "Similarity & AI Certification Report", emailBody, exactFilePathsOnDisk.ToArray()))
                    {
                        string targetTitle = "";
                        string targetAuthor = "";

                        using (var connection = new SqlConnection(ConnStr))
                        {
                            const string infoQuery = "SELECT Title, Author FROM SimilarityAICheck_Submission WHERE SubmissionID = @SubmissionID";
                            using (var command = new SqlCommand(infoQuery, connection))
                            {
                                command.Parameters.AddWithValue("@SubmissionID", submissionId);
                                await connection.OpenAsync();
                                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                                {
                                    if (await reader.ReadAsync())
                                    {
                                        targetTitle = reader["Title"]?.ToString();
                                        targetAuthor = reader["Author"]?.ToString();
                                    }
                                }
                            }
                        }

                        using (var connection = new SqlConnection(ConnStr))
                        {
                            const string insertQuery = @"
                                INSERT INTO ProcessorReturnedReports
                                (SubmissionID, UserID, ModuleType, FilesSent, SimilarityFilePath, AIFilePath,
                                 MMSCReportFilePath, MMSCCertFilePath, DeclarationFilePath, MessageToStudent, DateSent,
                                 Author, Title, SentByProcessorID, CaseNumber)
                                VALUES (@SubmissionID, @UserID, 'Similarity & AI Writing', @Files,
                                        @SimPath, @AIPath, @MmscPath, @MmscCert, @DeclPath, @Msg,
                                        GETDATE(), @Author, @Title, @SentByProcessorID, @CaseNumber)";

                            using (var command = new SqlCommand(insertQuery, connection))
                            {
                                command.Parameters.AddWithValue("@SubmissionID", submissionId);
                                command.Parameters.AddWithValue("@UserID", dbUserId);
                                command.Parameters.AddWithValue("@Files", string.Join(";", fileNamesOnly));
                                command.Parameters.AddWithValue("@SimPath", simRemarks ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@AIPath", aiRemarks ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@MmscPath", mmscReport ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@MmscCert", mmscCert ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@DeclPath", caseType == "Case3" ? "~/Templates/AIDeclaration.docx" : (declAi ?? (object)DBNull.Value));
                                command.Parameters.AddWithValue("@Msg", emailBody);
                                command.Parameters.AddWithValue("@Author", targetAuthor);
                                command.Parameters.AddWithValue("@Title", targetTitle);
                                command.Parameters.AddWithValue("@SentByProcessorID", Session["UserID"]?.ToString() ?? "Unknown");
                                command.Parameters.AddWithValue("@CaseNumber", caseType);

                                await connection.OpenAsync();
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        using (var connection = new SqlConnection(ConnStr))
                        {
                            const string updateQuery = "UPDATE SimilarityAICheck_Submission SET IsReportsEmailed = 1 WHERE SubmissionID = @ID";
                            using (var command = new SqlCommand(updateQuery, connection))
                            {
                                command.Parameters.AddWithValue("@ID", submissionId);
                                await connection.OpenAsync();
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        if (lblSending != null) lblSending.Visible = false;
                        lblStatus.Text = "✔️ Email dispatched and records updated successfully.";
                        lblStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        if (lblSending != null) lblSending.Visible = false;
                        lblStatus.Text = "❌ SMTP transmission failed.";
                        lblStatus.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex)
                {
                    if (lblSending != null) lblSending.Visible = false;
                    lblStatus.Text = "❌ System Error: " + ex.Message;
                    lblStatus.ForeColor = Color.Red;
                }
                finally
                {
                    ReopenModal(modalId);
                }
            }));
        }

        private void ReopenModal(string modalId)
        {
            if (string.IsNullOrEmpty(modalId)) return;
            string script = $"Sys.Application.add_load(function () {{ showModal('{modalId}'); }});";
            ScriptManager.RegisterStartupScript(this, GetType(), $"ReopenModal_{Guid.NewGuid()}", script, true);
        }

        private async Task<string> GetAuthorFromSubmissionIdAsync(int submissionId)
        {
            using (var connection = new SqlConnection(ConnStr))
            {
                const string query = "SELECT Author FROM SimilarityAICheck_Submission WHERE SubmissionID = @ID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", submissionId);
                    await connection.OpenAsync();
                    return (await command.ExecuteScalarAsync())?.ToString() ?? "Unknown";
                }
            }
        }

        private async Task UpdateRemarksFilePathAsync(int submissionId, string columnName, string filePath)
        {
            using (var connection = new SqlConnection(ConnStr))
            {
                string query = $"UPDATE SimilarityAICheck_Submission SET {columnName} = @Path WHERE SubmissionID = @ID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Path", filePath);
                    command.Parameters.AddWithValue("@ID", submissionId);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private string SaveFile(FileUpload fileUpload, string authorName, string prefix)
        {
            string cleanAuthor = Regex.Replace(authorName, $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "_");
            string directoryPath = Server.MapPath("~/App_Data/UploadedRemarks/");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string targetFileName = $"{prefix}_{cleanAuthor}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(fileUpload.FileName)}";
            string combinedPhysicalPath = Path.Combine(directoryPath, targetFileName);

            fileUpload.SaveAs(combinedPhysicalPath);
            return "~/App_Data/UploadedRemarks/" + targetFileName;
        }

        private bool SendEmailWithMultipleAttachments(string toEmail, string subject, string body, string[] attachments)
        {
            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(ConfigurationManager.AppSettings["AdminEmail"] ?? "no-reply@nvsu.edu.ph");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    foreach (string file in attachments)
                    {
                        if (File.Exists(file))
                        {
                            mail.Attachments.Add(new Attachment(file));
                        }
                    }

                    using (var smtp = new SmtpClient())
                    {
                        // Configuration configurations are extracted automatically from Web.config <system.net> settings block
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            Session["SearchKeyword"] = keyword;
            CurrentPage = 0;
            RegisterAsyncTask(new PageAsyncTask(() => LoadSearchResultsAsync(keyword)));
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            Session["SearchKeyword"] = null;
            CurrentPage = 0;
            RegisterAsyncTask(new PageAsyncTask(() => LoadRemarksDataAsync()));
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
                NavigatePage();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            NavigatePage();
        }

        private void NavigatePage()
        {
            if (Session["SearchKeyword"] is string keyword && !string.IsNullOrEmpty(keyword))
            {
                RegisterAsyncTask(new PageAsyncTask(() => LoadSearchResultsAsync(keyword)));
            }
            else
            {
                RegisterAsyncTask(new PageAsyncTask(() => LoadRemarksDataAsync()));
            }
        }
    }
}