<%@ Page Title="SimilarityAIWritingCheck" Language="C#" MasterPageFile="~/ProcessorModule/ProcessorMaster.master" AutoEventWireup="true" CodeBehind="SimAiWriCheRemarks.aspx.cs" Inherits="CASApp1.ProcessorModule.SimAiWriCheRemarks" %>

<asp:Content ID="HeadContent4" ContentPlaceHolderID="HeadContent" runat="server">

    <script type="text/javascript">

        function showModal(id) {
            const allModals = document.querySelectorAll('.modal-overlay');
            allModals.forEach(modal => {
                modal.style.display = 'none';
                modal.classList.remove('active');
            });

            var modal = document.getElementById("modal_" + id);
            if (modal) {
                modal.style.display = "block";
                setTimeout(() => {
                    modal.classList.add('active');
                }, 10);
            }
        }

        function closeModal(id) {
            var modal = document.getElementById("modal_" + id);
            if (modal) {
                modal.style.display = "none";
                modal.classList.remove('active');

            }
        }
        document.addEventListener("change", function (e) {
            if (e.target.closest('.template-selector')) {
                const modal = e.target.closest(".modal-box");
                const panel = modal.querySelector("[id$='PanelCertInput']");
                if (panel) panel.style.display = "block";
            }
        });
        function hideModal(btn) {
            var modal = btn.closest('.modal-overlay');
            if (modal) {
                modal.style.display = 'none';
                modal.classList.remove('active');
            }
        }
        function reorderRows() {
            const repeater = document.getElementById('rptData');
            const items = Array.from(repeater.getElementsByClassName('table-row'));

            const flagged = [];
            const highlighted = [];
            const normal = [];
            const disabled = [];

            items.forEach(item => {
                const flagCheckbox = item.querySelector('.flag-checkbox');
                const highlightCheckbox = item.querySelector('.highlight-checkbox');
                const disableCheckbox = item.querySelector('.disable-checkbox');

                const isFlagged = flagCheckbox?.checked;
                const isHighlighted = highlightCheckbox?.checked;
                const isDisabled = disableCheckbox?.checked;

                if (isDisabled) {
                    disabled.push(item);
                } else if (isFlagged) {
                    flagged.push(item);
                } else if (isHighlighted) {
                    highlighted.push(item);
                } else {
                    normal.push(item);
                }
            });

            const container = repeater.parentNode;
            const reordered = [...flagged, ...highlighted, ...normal, ...disabled];

            reordered.forEach(row => container.appendChild(row));
        }



    </script>
    <style>
        body, html {
            height: 100%;
            margin: 0;
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
        }

        .form-control {
            width: 100%;
            padding: 10px;
            margin-bottom: 10px;
            font-size: 14px;
            border: 1px solid #ccc;
            border-radius: 6px;
            resize: vertical;
        }

        .processor-title {
            font-size: 40px;
            font-weight: bold;
            margin-bottom: 10px;
            margin-top: 60px;
            text-align: center;
        }

        table {
            width: 100%;
            border-collapse: collapse;
        }

        td {
            word-break: break-word;
            max-width: 200px;
        }

        th, td {
            border: 1px solid #ccc;
            padding: 3px;
            text-align: center;
            overflow-wrap: break-word;
        }

        th {
            background-color: #0056b3;
            color: white;
            font-size: 16px;
        }

        .download-button {
            display: inline-block;
            padding: 6px 12px;
            background-color: #007BFF;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-size: 13px;
        }

            .download-button:hover {
                background-color: #0056b3;
            }

        .upload-status {
            max-width: 100px;
            overflow: hidden;
            white-space: nowrap;
            text-overflow: ellipsis;
            display: inline-block;
            cursor: default;
        }

        .modal-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.5);
            z-index: 1000;
        }

        .modal-box {
            background-color: white;
            padding: 40px;
            width: 900px;
            max-width: 98vw;
            max-height: 90vh;
            overflow-y: auto;
            border-radius: 10px;
            box-shadow: 0 0 15px rgba(0,0,0,0.3);
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }

        .template-option {
            display: flex;
            flex-direction: column;
            align-items: center;
            margin-right: 20px;
            margin-bottom: 10px;
            min-width: 100px;
            font-size: 14px;
        }

        .close-modal {
            position: absolute;
            top: 10px;
            right: 15px;
            font-size: 20px;
            cursor: pointer;
            color: red;
        }

        .template-selector {
            display: flex;
            justify-content: center;
            gap: 20px;
            margin-top: 20px;
            flex-wrap: wrap;
        }

            .template-selector input[type="radio"] {
                margin-right: 6px;
            }

            .template-selector label {
                display: block;
                font-weight: bold;
                margin-bottom: 6px;
            }

        .rowHeader {
            background-color: #598eee;
        }

        .highlight-row {
            background-color: #fef0c8 !important;
        }

        .disabled-row {
            background-color: #d4e8ff !important;
            opacity: 0.6;
            pointer-events: none;
        }

            .disabled-row .marker-column {
                pointer-events: auto;
                opacity: 1 !important;
            }
    </style>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="MainContent" runat="server">
    <div class="processor-title">Similarity & AI Writing Check</div>
    <div style="margin-bottom: 15px;">
        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" Width="250px" placeholder="Search Title or Author" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
    </div>

    <!-- Search Results Repeater -->
    <asp:Panel ID="pnlSearchResults" runat="server" Visible="false">
        <h4>Search Results:</h4>
        <asp:Repeater ID="rptSearchResults" runat="server" OnItemCommand="rptSearch_ItemCommand" OnItemDataBound="rptSearch_ItemDataBound">
            <HeaderTemplate>
                <table>
                    <tr class="rowHeader">
                        <th colspan="2">Receipt</th>
                        <th>Revision No.</th>
                        <th>Type</th>
                        <th>Course</th>
                        <th>Document</th>
                        <th>Author</th>
                        <th>Title</th>
                        <th colspan="3">Reports</th>
                        <th>Declaration 
                            <br />
                            of
                            <br />
                            AI Use</th>
                        <th>MMSC 
                            <br />
                            Certification</th>
                        <th>Return
                            <br />
                            Certification</th>
                    </tr>
                    <tr class="rowHeader">
                        <th>No.</th>
                        <th>Copy</th>
                        <th></th>
                        <th></th>
                        <th></th>
                        <th></th>
                        <th></th>
                        <th></th>
                        <th>Similarity
                            <br />
                            Report</th>
                        <th>AI Writiing 
                            <br />
                            Report</th>
                        <th>MMSC 
                            <br />
                            Report</th>
                        <th></th>
                        <th></th>
                        <th></th>
                    </tr>
            </HeaderTemplate>

            <ItemTemplate>
                <!-- COPY the exact layout from RepeaterSubmissions here -->
                <!-- You can paste your entire item layout here (e.g., with upload, create, return buttons) -->
                <tr runat="server" id="rowContainer">
                    <%-- OFFICIAL RECEIPT --%>
                    <td><%# Eval("ReceiptNo") %></td>
                    <%-- RECEIPT COPY --%>
                    <td>

                        <asp:HyperLink runat="server"
    NavigateUrl='<%# "~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=CopyFilePath" %>'
    CssClass="download-button"
    ToolTip='<%# !string.IsNullOrEmpty(Eval("CopyFilePath") as string) 
                ? System.IO.Path.GetFileName(Eval("CopyFilePath").ToString()) 
                : "" %>'
    Target="_blank"
    Visible='<%# !string.IsNullOrEmpty(Eval("CopyFilePath") as string) %>'>
    <i class="fa fa-download"></i>
</asp:HyperLink>

                    </td>
                    <%-- REVISION --%>
                    <td><%# Eval("RevisionNo") %></td>
                    <%-- TYPE --%>
                    <td><%# Eval("Type") %></td>
                    <%-- COURSE --%>
                    <td><%# Eval("College") %></td>
                    <%-- DOCUMENT --%>
                    <td>

                        <asp:HyperLink runat="server"
    NavigateUrl='<%# "~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=DocumentFilePath" %>'
    CssClass="download-button"
    ToolTip='<%# !string.IsNullOrEmpty(Eval("DocumentFilePath") as string) 
                ? System.IO.Path.GetFileName(Eval("DocumentFilePath").ToString()) 
                : "" %>'
    Target="_blank"
    Visible='<%# !string.IsNullOrEmpty(Eval("DocumentFilePath") as string) %>'>
    <i class="fa fa-download"></i>
</asp:HyperLink>
                    </td>
                    <%-- AUTHORS --%>
                    <td><%# Eval("Author") %></td>
                    <%-- TITLE--%>
                    <td><%# Eval("Title") %></td>
                    <%-- SIMILARITY UPLOAD --%>
                    <td>
                        <asp:FileUpload ID="fuSimilarity1" runat="server" />
                        <asp:Button ID="btnSubmitSimilarity1" Text="📝 Upload" CssClass="download-button"
                            CommandName="SubmitSimilarity"
                            CommandArgument='<%# Eval("SubmissionID") %>' runat="server" />
                        <br />
                        <asp:Label ID="lblSimFile1" runat="server" Font-Size="Small" CssClass="upload-status" />
                        <asp:Label ID="lblSimStatus1" runat="server" Font-Size="Small" CssClass="upload-status" />
                    </td>
                    <%-- AI UPLOAD --%>
                    <td>
                        <asp:FileUpload ID="fuAIWriting1" runat="server" />
                        <asp:Button ID="btnSubmitAI" Text="📝 Upload" CssClass="download-button"
                            CommandName="SubmitAIWriting"
                            CommandArgument='<%# Eval("SubmissionID") %>' runat="server" />
                        <br />
                        <asp:Label ID="lblAIFile1" runat="server" Font-Size="Small" CssClass="upload-status" />
                        <asp:Label ID="lblAIStatus1" runat="server" CssClass="text-success" Visible="false" />
                    </td>
                    <%-- MMSC REPORT COLUMN --%>
                    <td>
                        <asp:UpdatePanel ID="upMMSCModal1" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Button ID="btnMMSCModal1" runat="server" Text="📝 Upload"
                                    CssClass="download-button"
                                    OnClientClick='<%# "showModal(\"mmsc_" + Eval("SubmissionID") + "\"); return false;" %>' />
                                <br />
                                <asp:Label ID="lblMMSCFileName1" runat="server"
    Text='<%# string.IsNullOrEmpty(Eval("MMSCReportPath") as string) ? "" : "Uploaded" %>'
    ToolTip='<%# Eval("MMSCReportPath") != null ? System.IO.Path.GetFileName(Eval("MMSCReportPath").ToString()) : "" %>'
    Font-Size="Small" />

                                <div class="modal-overlay" id='<%# "modal_mmsc_" + Eval("SubmissionID") %>'>
                                    <div class="modal-box">
                                        <asp:HiddenField ID="hfMMSCModalID1" runat="server" Value='<%# "mmsc_" + Eval("SubmissionID") %>' />
                                        <span class="close-modal"
                                            onclick='<%# "closeModal(\"mmsc_" + Eval("SubmissionID") + "\")" %>'>&times;</span>
                                        <!-- CASE SELECTION BLOCK -->
                                        <div style="display: flex; justify-content: center; margin-bottom: 20px;">
                                            <!-- Case 1 -->
                                            <div style="border: 1px solid #ccc; padding: 10px; width: 48%;">
                                                <div style="font-weight: bold; text-align: center;">CASE 1 & 2</div>
                                            </div>
                                        </div>
                                        <!-- ✅ INPUT FIELDS -->
                                        <asp:TextBox ID="txtMMSC_Title1" runat="server" CssClass="form-control" Placeholder="Title" />
                                        <asp:TextBox ID="txtMMSC_Author1" runat="server" CssClass="form-control" Placeholder="Author/s" />
                                        <asp:TextBox ID="txtMMSC_Date1" runat="server" CssClass="form-control" Placeholder="Date ex: January 1, 2000" />
                                        <asp:TextBox ID="txtMMSC_SimRate1" runat="server" CssClass="form-control" Placeholder="Similarity Percentage" />
                                        <asp:TextBox ID="txtMMSC_AIRate1" runat="server" CssClass="form-control" Placeholder="AI Writing Percentage" />
                                        <asp:HiddenField ID="hfMMSCSubmissionID1" runat="server" Value='<%# Eval("SubmissionID") %>' />
                                        <!-- ✅ BUTTON -->
                                        <asp:Button ID="btnGenerateMMSC1" runat="server" CssClass="btn btn-success"
                                            Text="Generate Certificate"
                                            OnClick="btnGenerateReport_Click" />
                                        <!-- ✅ STATUS -->
                                        <asp:Label ID="lblMMSCStatus1" runat="server" Font-Bold="true" />
                                        <!-- ✅ CLOSE -->
                                        <asp:Button ID="btnCloseMMSCModal1" runat="server" Text="Close"
                                            CssClass="btn btn-secondary"
                                            OnClientClick="hideModal(this); return false;" />
                                    </div>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </td>
                    <%-- Declaration of AI Use --%>
                    <td>
                                                                                        <asp:HyperLink runat="server"
    NavigateUrl='<%# "~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=DeclarationAIPath" %>'
    CssClass="download-button"
    ToolTip='<%# !string.IsNullOrEmpty(Eval("DeclarationAIPath") as string) 
                ? System.IO.Path.GetFileName(Eval("DeclarationAIPath").ToString()) 
                : "" %>'
    Target="_blank"
    Visible='<%# !string.IsNullOrEmpty(Eval("DeclarationAIPath") as string) %>'>
    <i class="fa fa-download"></i>
</asp:HyperLink>
                    </td>
                    <%-- MMSC CERTIFICATION COLUMN--%>
                    <td>
                        <asp:UpdatePanel ID="upMMSCCertModal1" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Button ID="btnMMSCCertModal1" runat="server" Text="📝 Upload"
                                    CssClass="download-button"
                                    OnClientClick='<%# "showModal(\"mmsccert_" + Eval("SubmissionID") + "\"); return false;" %>' />
                                <br />
                                <asp:Label ID="lblMMSCCertFile1" runat="server"
    Text='<%# string.IsNullOrEmpty(Eval("MMSCCertificationPath") as string) ? "" : "Uploaded" %>'
    ToolTip='<%# Eval("MMSCCertificationPath") != null ? System.IO.Path.GetFileName(Eval("MMSCCertificationPath").ToString()) : "" %>'
    Font-Size="Small" />

                                <div class="modal-overlay" id='<%# "modal_mmsccert_" + Eval("SubmissionID") %>'>
                                    <div class="modal-box">
                                        <span class="close-modal"
                                            onclick='<%# "closeModal(\"mmsccert_" + Eval("SubmissionID") + "\")" %>'>&times;</span>
                                        <!-- Template Selection -->
                                        <div>
                                            <div style="margin-bottom: 20px;">
                                                <strong>Select Certification Template:</strong><br />
                                            </div>
                                            <div style="margin-bottom: 5px;">
                                                <asp:RadioButton ID="rbCert11" runat="server" GroupName="CertTemplate" Text=" Certification 1 " />
                                            </div>
                                            <div style="margin-bottom: 5px;">
                                                <asp:RadioButton ID="rbCert21" runat="server" GroupName="CertTemplate" Text=" Certification 2 (with Declaration of AI Use)" />
                                            </div>
                                        </div>
                                        <!-- Input Fields -->
                                        <asp:TextBox ID="txtCertTitle1" runat="server" CssClass="form-control" Placeholder="Title" />
                                        <asp:TextBox ID="txtCertAuthor1" runat="server" CssClass="form-control" Placeholder="Author/s" />
                                        <asp:TextBox ID="txtCertDate1" runat="server" CssClass="form-control" Placeholder="Date" />
                                        <asp:TextBox ID="txtCertSimRate1" runat="server" CssClass="form-control" Placeholder="Similarity Score: " />
                                        <asp:TextBox ID="txtCertAIRate1" runat="server" CssClass="form-control" Placeholder="AI Generated Score: (for Certification 2)" />
                                        <asp:TextBox ID="txtCertAIUsed1" runat="server" CssClass="form-control" Placeholder="AI Used (for Certification 2)" />
                                        <asp:HiddenField ID="hfMMSCCertSubmissionID1" runat="server" Value='<%# Eval("SubmissionID") %>' />
                                        <asp:HiddenField ID="hfMMSCCertModalID1" runat="server" Value='<%# "mmsccert_" + Eval("SubmissionID") %>' />
                                        <asp:Button ID="btnSubmitMMSCCert1" runat="server" CssClass="btn btn-success"
                                            Text="Generate Certificate"
                                            OnClick="btnSubmitMMSCCert_Click" />
                                        <asp:Label ID="lblMMSCCertStatus1" runat="server" Font-Bold="true" />
                                        <br /><br />

<!-- NEW Upload Option -->
<asp:FileUpload ID="fuMMSCCert" runat="server" CssClass="form-control" />
<asp:Button ID="btnUploadMMSCCert" runat="server" CssClass="btn btn-primary"
    Text="Upload Certificate"
    OnClick="btnUploadMMSCCert_Click" UseSubmitBehavior="true" />
<asp:Label ID="lblUploadMMSCCertStatus" runat="server" ForeColor="Gray" />
                                        
                                        <asp:Button ID="btnCloseMMSCCertModal1" runat="server" Text="Close"
                                            CssClass="btn btn-secondary"
                                            OnClientClick="hideModal(this); return false;" />
                                    </div>
                                </div>
                            </ContentTemplate>
                            <Triggers>
                            <asp:PostBackTrigger   ControlID="btnUploadMMSCCert" />
                                </Triggers>
                        </asp:UpdatePanel>
                    </td>
                    <%-- RETURN CERTIFICATION COLUMN --%>
                    <td>
                        <asp:Button ID="btnReturnModal1" runat="server" Text="📤 Return"
                            CssClass="download-button"
                            OnClientClick='<%# "showModal(\"return_" + Eval("SubmissionID") + "\"); return false;" %>' />
                        <!-- Modal for Return Certification -->
                        <asp:UpdatePanel ID="upReturnModal1" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="modal-overlay" id='<%# "modal_return_" + Eval("SubmissionID") %>'>
                                    <div class="modal-box">
                                        <span class="close-modal"
                                            onclick='<%# "closeModal(\"return_" + Eval("SubmissionID") + "\")" %>'>&times;</span>
                                        <!-- Inside the modal-box -->
                                        <strong>Select Certification Type:</strong>
                                        <br />
                                        <br />
                                        <!-- RadioButton Options in Rows -->
                                        <div style="margin-bottom: 5px;">
                                            <asp:RadioButton ID="rbReturnCase11" runat="server" GroupName="ReturnCase"
                                                Text=" Case 1 & 2: Similarity, AI, and MMSC Report" />
                                        </div>
                                        
                                        <div style="margin-bottom: 5px;">
                                            <asp:RadioButton ID="rbReturnCase31" runat="server" GroupName="ReturnCase"
                                                Text=" Case 3: AI Writing Report and Declaration of AI Use Template" />
                                        </div>
                                        <div style="margin-bottom: 5px;">
                                            <asp:RadioButton ID="rbReturnCase41" runat="server" GroupName="ReturnCase"
                                                Text=" Case 4: Similarity, AI, and MMSC Certification" />
                                        </div>
                                        <!-- Fixed Email Message Preview -->
                                        <asp:Label ID="lblEmailMessagePreview1" runat="server" Font-Italic="true" ForeColor="Gray" />
                                        <!-- Hidden Field for Modal ID -->
                                        <asp:HiddenField ID="hfReturnSubmissionID1" runat="server" Value='<%# Eval("SubmissionID") %>' />
                                        <asp:HiddenField ID="hfReturnModalID1" runat="server" Value='<%# "return_" + Eval("SubmissionID") %>' />
                                        <asp:Label ID="lblSending1" runat="server" Text="Sending..." CssClass="text-info" Style="display: none;" />
                                        <asp:Button ID="btnSendReturnCert1" runat="server" Text="Send Email"
                                            CssClass="btn btn-success"
                                            OnClick="btnSendReturnCert_Click"
                                            OnClientClick='<%# Eval("SubmissionID") != null ? 
    "document.getElementById(\\\"" + 
    ((Label)Container.FindControl("lblSendingSearch"))?.ClientID + 
    "\\\").style.display = \\\\\"inline\\\\\";" : "" %>' />
                                        <asp:Button ID="btnCloseReturnModal1" runat="server" Text="Close"
                                            CssClass="btn btn-secondary"
                                            OnClientClick="hideModal(this); return false;" />

                                        <asp:Label ID="lblReturnStatus1" runat="server" Font-Bold="true" />
                                    </div>
                                </div>
                            </ContentTemplate>
                            <Triggers>
        
    </Triggers>
                        </asp:UpdatePanel>
                    </td>
                                <%--MARKER COLUMN --%>
            <td class="marker-column">
                <asp:CheckBox ID="chkHighlight1" runat="server" Text="📌" AutoPostBack="true"
                    OnCheckedChanged="chkHighlight_CheckedChanged" />

                <br />
                <asp:CheckBox ID="chkFlagged1" runat="server" Text="🚩" AutoPostBack="true"
    OnCheckedChanged="chkFlagged_CheckedChanged" />
<asp:HiddenField ID="hfSubmissionIDFlagged" runat="server" Value='<%# Eval("SubmissionID") %>' />
                <br />

                <asp:CheckBox ID="chkDisable1" runat="server" Text="✔️" AutoPostBack="true"
                    OnCheckedChanged="chkDisable_CheckedChanged" />
                <asp:HiddenField ID="hfSubmissionID" runat="server" Value='<%# Eval("SubmissionID") %>' />
            </td>
                </tr>
                </table>
            </ItemTemplate>
        </asp:Repeater>

        <hr />
    </asp:Panel>

    <asp:Repeater ID="rptSimAICheck" runat="server" OnItemCommand="rptSimAICheck_ItemCommand" OnItemDataBound="rptSimAICheck_ItemDataBound">
        <HeaderTemplate>
            <table>
                <tr class="rowHeader">
                    <th colspan="2">Receipt</th>
                    <th>Revision No.</th>
                    <th>Type</th>
                    <th>Course</th>
                    <th>Document</th>
                    <th>Author</th>
                    <th>Title</th>
                    <th colspan="3">Reports</th>
                    <th>Declaration 
                        <br />of
                        <br />
                        AI Use</th>
                    <th>MMSC
                        <br />
                        Certification</th>
                    <th>Return
                        <br />
                        Certification</th>
                </tr>
                <tr class="rowHeader">
                    <th>No.</th>
                    <th>Copy</th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th>Similarity
                        <br />
                        Report</th>
                    <th>AI Writing
                        <br />
                        Report</th>
                    <th>MMSC
                        <br />
                        Report</th>
                    <th></th>
                    <th></th>
                    <th></th>
                </tr>
        </HeaderTemplate>

        <ItemTemplate>
            <tr runat="server" id="rowContainer">

                <%-- OFFICIAL RECEIPT --%>
                <td><%# Eval("ReceiptNo") %></td>

                <%-- RECEIPT COPY --%>
                <td>

                                            <asp:HyperLink runat="server"
    NavigateUrl='<%# "~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=CopyFilePath" %>'
    CssClass="download-button"
    ToolTip='<%# !string.IsNullOrEmpty(Eval("CopyFilePath") as string) 
                ? System.IO.Path.GetFileName(Eval("CopyFilePath").ToString()) 
                : "" %>'
    Target="_blank"
    Visible='<%# !string.IsNullOrEmpty(Eval("CopyFilePath") as string) %>'>
    <i class="fa fa-download"></i>
</asp:HyperLink>
                </td>

                <%-- REVISION --%>
                <td><%# Eval("RevisionNo") %></td>

                <%-- TYPE --%>
                <td><%# Eval("Type") %></td>

                <%-- COURSE --%>
                <td><%# Eval("College") %></td>

                <%-- DOCUMENT --%>
                <td>
                                            <asp:HyperLink runat="server"
    NavigateUrl='<%# "~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=DocumentFilePath" %>'
    CssClass="download-button"
    ToolTip='<%# !string.IsNullOrEmpty(Eval("DocumentFilePath") as string) 
                ? System.IO.Path.GetFileName(Eval("DocumentFilePath").ToString()) 
                : "" %>'
    Target="_blank"
    Visible='<%# !string.IsNullOrEmpty(Eval("DocumentFilePath") as string) %>'>
    <i class="fa fa-download"></i>
</asp:HyperLink>
                </td>

                <%-- AUTHORS --%>
                <td><%# Eval("Author") %></td>

                <%-- TITLE--%>
                <td><%# Eval("Title") %></td>

                <%-- SIMILARITY UPLOAD --%>
                <td>
                    <asp:FileUpload ID="fuSimilarity" runat="server" />
                    <asp:Button ID="btnSubmitSimilarity" Text="📝 Upload" CssClass="download-button"
                        CommandName="SubmitSimilarity"
                        CommandArgument='<%# Eval("SubmissionID") %>' runat="server" />
                    <br />
                    <asp:Label ID="lblSimFile" runat="server" Font-Size="Small" CssClass="upload-status" />
                    <asp:Label ID="lblSimStatus" runat="server" Font-Size="Small" CssClass="upload-status" />
                </td>

                <%-- AI UPLOAD --%>
                <td>
                    <asp:FileUpload ID="fuAIWriting" runat="server" />
                    <asp:Button ID="btnSubmitAI" Text="📝 Upload" CssClass="download-button"
                        CommandName="SubmitAIWriting"
                        CommandArgument='<%# Eval("SubmissionID") %>' runat="server" />
                    <br />
                    <asp:Label ID="lblAIFile" runat="server" Font-Size="Small" CssClass="upload-status" />
                    <asp:Label ID="lblAIStatus" runat="server" CssClass="text-success" Visible="false" />
                </td>

                <%-- MMSC REPORT COLUMN --%>
                <td>
                    <asp:UpdatePanel ID="upMMSCModal" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Button ID="btnMMSCModal" runat="server" Text="📝 Upload"
                                CssClass="download-button"
                                OnClientClick='<%# "showModal(\"mmsc_" + Eval("SubmissionID") + "\"); return false;" %>' />
                            <br />
                           
                            <asp:Label ID="lblMMSCFileName" runat="server"
    Text='<%# string.IsNullOrEmpty(Eval("MMSCReportPath") as string) ? "" : "Uploaded" %>'
    ToolTip='<%# Eval("MMSCReportPath") != null ? System.IO.Path.GetFileName(Eval("MMSCReportPath").ToString()) : "" %>'
    Font-Size="Small" />

                            <div class="modal-overlay" id='<%# "modal_mmsc_" + Eval("SubmissionID") %>'>
                                <div class="modal-box">
                                    <asp:HiddenField ID="hfMMSCModalID" runat="server" Value='<%# "mmsc_" + Eval("SubmissionID") %>' />
                                    <span class="close-modal"
                                        onclick='<%# "closeModal(\"mmsc_" + Eval("SubmissionID") + "\")" %>'>&times;</span>
                                    <!-- CASE SELECTION BLOCK -->
                                    <div style="display: flex; justify-content: center; margin-bottom: 20px;">
                                        <!-- Case 1 -->
                                        <div style="border: 1px solid #ccc; padding: 10px; width: 48%;">
                                            <div style="font-weight: bold; text-align: center;">CASE 1 & 2</div>
                                        </div>
                                    </div>
                                    <!-- INPUT FIELDS -->
                                    <asp:TextBox ID="txtMMSC_Title" runat="server" CssClass="form-control" Placeholder="Title" />
                                    <asp:TextBox ID="txtMMSC_Author" runat="server" CssClass="form-control" Placeholder="Author/s" />
                                    <asp:TextBox ID="txtMMSC_Date" runat="server" CssClass="form-control" Placeholder="Date (ex: January 1, 2000)" />
                                    <asp:TextBox ID="txtMMSC_SimRate" runat="server" CssClass="form-control" Placeholder="Similarity Percentage" />
                                    <asp:TextBox ID="txtMMSC_AIRate" runat="server" CssClass="form-control" Placeholder="AI Writing Percentage" />
                                    <asp:HiddenField ID="hfMMSCSubmissionID" runat="server" Value='<%# Eval("SubmissionID") %>' />
                                    <!-- BUTTON -->
                                    <asp:Button ID="btnGenerateMMSC" runat="server" CssClass="btn btn-success"
                                        Text="Generate Certificate"
                                        OnClick="btnGenerateReport_Click" />
                                    <!-- STATUS -->
                                    <asp:Label ID="lblMMSCStatus" runat="server" Font-Bold="true" />
                                    <!-- CLOSE -->
                                    <asp:Button ID="btnCloseMMSCModal" runat="server" Text="Close"
                                        CssClass="btn btn-secondary"
                                        OnClientClick="hideModal(this); return false;" />
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>

                <%-- Declaration of AI Use --%>
                <td>

                                                                <asp:HyperLink runat="server"
    NavigateUrl='<%# "~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=DeclarationAIPath" %>'
    CssClass="download-button"
    ToolTip='<%# !string.IsNullOrEmpty(Eval("DeclarationAIPath") as string) 
                ? System.IO.Path.GetFileName(Eval("DeclarationAIPath").ToString()) 
                : "" %>'
    Target="_blank"
    Visible='<%# !string.IsNullOrEmpty(Eval("DeclarationAIPath") as string) %>'>
    <i class="fa fa-download"></i>
</asp:HyperLink>
                </td>

                <%-- MMSC CERTIFICATION COLUMN--%>
                <td>
                    <asp:UpdatePanel ID="upMMSCCertModal" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Button ID="btnMMSCCertModal" runat="server" Text="📝 Upload"
                                CssClass="download-button"
                                OnClientClick='<%# "showModal(\"mmsccert_" + Eval("SubmissionID") + "\"); return false;" %>' />
                            <br />
                            <asp:Label ID="lblMMSCCertFile" runat="server"
    Text='<%# string.IsNullOrEmpty(Eval("MMSCCertificationPath") as string) ? "" : "Uploaded" %>'
    ToolTip='<%# Eval("MMSCCertificationPath") != null ? System.IO.Path.GetFileName(Eval("MMSCCertificationPath").ToString()) : "" %>'
    Font-Size="Small" />

                            <div class="modal-overlay" id='<%# "modal_mmsccert_" + Eval("SubmissionID") %>'>
                                <div class="modal-box">
                                    <span class="close-modal"
                                        onclick='<%# "closeModal(\"mmsccert_" + Eval("SubmissionID") + "\")" %>'>&times;</span>

                                    <!-- Template Selection -->
                                    <div>
                                        <div style="margin-bottom: 20px;">
                                            <strong>Select Certification Template:</strong><br />
                                        </div>
                                        <div style="margin-bottom: 5px;">
                                            <asp:RadioButton ID="rbCert1" runat="server" GroupName="CertTemplate" Text=" Certification 1 " />
                                        </div>
                                        <div style="margin-bottom: 5px;">
                                            <asp:RadioButton ID="rbCert2" runat="server" GroupName="CertTemplate" Text=" Certification 2 (with Declaration of AI Use)" />
                                        </div>
                                    </div>

                                    <!-- Input Fields -->
                                    <asp:TextBox ID="txtCertTitle" runat="server" CssClass="form-control" Placeholder="Title" />
                                    <asp:TextBox ID="txtCertAuthor" runat="server" CssClass="form-control" Placeholder="Author/s" />
                                    <asp:TextBox ID="txtCertDate" runat="server" CssClass="form-control" Placeholder="Date (ex: January 1, 2000)" />
                                    <asp:TextBox ID="txtCertSimRate" runat="server" CssClass="form-control" Placeholder="Similarity Score: " />
                                    <asp:TextBox ID="txtCertAIRate" runat="server" CssClass="form-control" Placeholder="AI Generated Score: (for Certification 2)" />
                                    <asp:TextBox ID="txtCertAIUsed" runat="server" CssClass="form-control" Placeholder="AI Used (for Certification 2)" />
                                    <asp:HiddenField ID="hfMMSCCertSubmissionID" runat="server" Value='<%# Eval("SubmissionID") %>' />
                                    <asp:HiddenField ID="hfMMSCCertModalID" runat="server" Value='<%# "mmsccert_" + Eval("SubmissionID") %>' />
                                    <asp:Button ID="btnSubmitMMSCCert" runat="server" CssClass="btn btn-success"
                                        Text="Generate Certificate"
                                        OnClick="btnSubmitMMSCCert_Click" />
                                    <asp:Label ID="lblMMSCCertStatus" runat="server" Font-Bold="true" />
                                                                            <br /><br />

<!-- NEW Upload Option -->
<asp:FileUpload ID="fuMMSCCert1" runat="server" CssClass="form-control" />
<asp:Button ID="btnUploadMMSCCert1" runat="server" CssClass="btn btn-primary"
    Text="Upload Certificate"
    OnClick="btnUploadMMSCCert_Click" UseSubmitBehavior="true" />
<asp:Label ID="lblUploadMMSCCertStatus1" runat="server" ForeColor="Gray" />
                                    
                                    <asp:Button ID="btnCloseMMSCCertModal" runat="server" Text="Close"
                                        CssClass="btn btn-secondary"
                                        OnClientClick="hideModal(this); return false;" />
                                </div>
                            </div>
                        </ContentTemplate>
                        <Triggers>
        <asp:PostBackTrigger  ControlID="btnUploadMMSCCert1" />
    </Triggers>
                    </asp:UpdatePanel>
                </td>

                <%-- RETURN CERTIFICATION COLUMN --%>
                <td>
                    <asp:Button ID="btnReturnModal" runat="server" Text="📤 Return"
                        CssClass="download-button"
                        OnClientClick='<%# "showModal(\"return_" + Eval("SubmissionID") + "\"); return false;" %>' />
                    <!-- Modal for Return Certification -->
                    <asp:UpdatePanel ID="upReturnModal" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="modal-overlay" id='<%# "modal_return_" + Eval("SubmissionID") %>'>
                                <div class="modal-box">
                                    <span class="close-modal"
                                        onclick='<%# "closeModal(\"return_" + Eval("SubmissionID") + "\")" %>'>&times;</span>
                                    <!-- Inside the modal-box -->
                                    <strong>Select Return Certification Type:</strong>
                                    <br />
                                    <br />
                                    <!-- RadioButton Options in Rows -->
                                    <div style="margin-bottom: 5px;">
                                        <asp:RadioButton ID="rbReturnCase1" runat="server" GroupName="ReturnCase"
                                            Text=" Case 1 & 2: Similarity, AI Writing, and MMSC Report" />
                                    </div>

                                    <div style="margin-bottom: 5px;">
                                        <asp:RadioButton ID="rbReturnCase3" runat="server" GroupName="ReturnCase"
                                            Text=" Case 3: AI Writing and Declaration of AI Use Template" />
                                    </div>
                                    <div style="margin-bottom: 5px;">
                                        <asp:RadioButton ID="rbReturnCase4" runat="server" GroupName="ReturnCase"
                                            Text=" Case 4: Similarity, AI Writing, and MMSC Certification" />
                                    </div>
                                    <br />
                                    <br />
                                    <asp:Label ID="lblEmailMessagePreview" runat="server" Font-Italic="true" ForeColor="Gray" />

                                    <!-- Hidden Field for Modal ID -->
                                    <asp:HiddenField ID="hfReturnSubmissionID" runat="server" Value='<%# Eval("SubmissionID") %>' />
                                    <asp:HiddenField ID="hfReturnModalID" runat="server" Value='<%# "return_" + Eval("SubmissionID") %>' />
                                    <asp:Label ID="lblSending" runat="server" Text="Sending..." Style="display: none;" />
                                    <asp:Button ID="btnSendReturnCert" runat="server" Text="Send Email"
                                        CssClass="btn btn-success"
                                        OnClick="btnSendReturnCert_Click"
                                        OnClientClick='<%# "document.getElementById(\"" + ((Label)Container.FindControl("lblSending")).ClientID + "\").style.display = \"inline\";" %>' />

                                    <asp:Button ID="btnCloseReturnModal" runat="server" Text="Close"
                                        CssClass="btn btn-secondary"
                                        OnClientClick="hideModal(this); return false;" />
                                    <br />
                                    <br />
                                    <asp:Label ID="lblReturnStatus" runat="server" Font-Bold="true" />
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <%--MARKER COLUMN --%>
                <td class="marker-column">
                    <asp:CheckBox ID="chkHighlight" runat="server" Text="📌" AutoPostBack="true"
                        OnCheckedChanged="chkHighlight_CheckedChanged" />

                    <br />
                    <asp:CheckBox ID="chkFlagged" runat="server" Text="🚩" AutoPostBack="true"
        OnCheckedChanged="chkFlagged_CheckedChanged" />
    <asp:HiddenField ID="hfSubmissionIDFlagged" runat="server" Value='<%# Eval("SubmissionID") %>' />
                    <br />

                    <asp:CheckBox ID="chkDisable" runat="server" Text="✔️" AutoPostBack="true"
                        OnCheckedChanged="chkDisable_CheckedChanged" />
                    <asp:HiddenField ID="hfSubmissionID" runat="server" Value='<%# Eval("SubmissionID") %>' />
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <div style="text-align: center; margin-top: 20px;">
        <asp:Button ID="btnPrev" runat="server" Text="« Prev" OnClick="btnPrev_Click" />
        <asp:Label ID="lblPageInfo" runat="server" Text="" Style="margin: 0 10px;" />
        <asp:Button ID="btnNext" runat="server" Text="Next »" OnClick="btnNext_Click" />
    </div>



</asp:Content>
