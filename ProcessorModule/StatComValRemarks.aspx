<%@ Page Title="StatisticsComputationalValidation" Language="C#" MasterPageFile="~/ProcessorModule/ProcessorMaster.master" AutoEventWireup="true" CodeBehind="StatComValRemarks.aspx.cs" Inherits="CASApp1.ProcessorModule.StatComValRemarks" %>


<asp:Content ID="HeadContent2" ContentPlaceHolderID="HeadContent" runat="server">

    <script type="text/javascript">
        function showModal(id) {
            document.getElementById('modal_' + id).style.display = 'block';
        }

        function closeModal(id) {
            document.getElementById('modal_' + id).style.display = 'none';
        }

        function showSendingStatus(btn) {
            var label = btn.closest('td').querySelector('.return-status');
            if (label) {
                label.innerText = 'Sending...';
            }
        }
        function showConfirmModal(arg) {
            document.getElementById('<%= hfReturnArg.ClientID %>').value = arg;
            document.getElementById('confirmModal').style.display = 'block';
        }
        function hideConfirmModal() {
            document.getElementById('confirmModal').style.display = 'none';
        }

    </script>

    <style>
        .page-wrapper {
            padding: 20px 40px;
            width: 100%;
            box-sizing: border-box;
        }

        .processor-title {
            font-size: 40px;
            font-weight: bold;
            margin-bottom: 10px;
            margin-top: 30px;
            text-align: center;
        }

        table {
            background-color: white;
            width: 100%;
            border-collapse: collapse;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }

        th, td {
            border: 1px solid #ccc;
            padding: 3px;
            text-align: center;
            word-break: break-word;
            overflow-wrap: break-word;
        }

        th {
            background-color: #0056b3;
            color: white;
            font-size: 16px;
        }

        .remarks-box {
            width: 100%;
            height: 60px;
        }

        .remarks-box {
            width: 100%;
            height: 60px;
        }

        .download-button, .cert-button {
            display: inline-block;
            padding: 6px 12px;
            background-color: #007BFF;
            color: aliceblue;
            text-decoration: none;
            border-radius: 4px;
            font-size: 13px;
            border: none;
            cursor: pointer;
            transition: all 0.2s ease-in-out;
        }

            .download-button:hover, .cert-button:hover {
                background-color: #0056b3;
            }

        .disabled-row {
            background-color: #e2e2e2 !important;
            color: white !important;
        }

        .highlighted-row {
            background-color: #fffbb6 !important;
        }

        .download-button:hover {
            background-color: #0056b3;
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
            padding: 30px;
            width: 500px;
            max-width: 90%;
            border-radius: 10px;
            box-shadow: 0 0 15px rgba(0,0,0,0.3);
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }

        .close-modal {
            position: absolute;
            top: 10px;
            right: 15px;
            font-size: 20px;
            cursor: pointer;
            color: red;
        }

        .modal-box input, .modal-box textarea {
            width: 100%;
            margin-bottom: 15px;
            padding: 8px;
            box-sizing: border-box;
        }

        .file-cell {
            max-width: 150px; 
            white-space: nowrap; 
            overflow: hidden; 
            text-overflow: ellipsis;
            vertical-align: middle;
        }

        .file-name {
            display: block;
            margin-bottom: 4px;
            cursor: default;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-wrapper">
        <div class="processor-title">Statistics Computation Validation</div>
        <!-- 🔶 Manual 7-column table with spacing around -->
        <div style="padding: 20px 5px;">
            <asp:Repeater ID="rptRemarks" runat="server"
                OnItemCommand="rptRemarks_ItemCommand"
                OnItemDataBound="rptRemarks_ItemDataBound">

                <HeaderTemplate>
                    <table border="1" style="width: 100%; border-collapse: collapse; text-align: center; margin-top: 10px;">
                        <tr>
                            <th>User</th>
                            <th>Raw Data</th>
                            <th>Proposal Copy</th>
                            <th>Instrument Used
                                <br />
                                in
                                <br />
                                Data Gathering</th>
                            <th>Statistician's
                                <br />
                                Computation 
                                <br />
                                or
                                <br />
                                Analyses</th>
                            <th>Statistician's
                                <br />
                                Certification</th>
                            <th>Official Receipt</th>
                            <th>Certification</th>
                            <th>Return Certification</th>


                        </tr>
                </HeaderTemplate>

                <ItemTemplate>

                    <tr id="Tr1" runat="server">
                        <td><%# Eval("FullName") %></td>


                        <td class="file-cell">
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=RawDataPath") %>'
                                CssClass="download-button"
                                Text="Download"
                                ToolTip='<%# System.IO.Path.GetFileName(Eval("RawDataPath").ToString()) %>'
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("RawDataPath") as string) %>' />

                            <asp:Button ID="Button1" runat="server"
                                Text="⏎"
                                CssClass="download-button"
                                BackColor="#dc3545"
                                ForeColor="White"
                                Visible='<%# !string.IsNullOrEmpty(Eval("RawDataPath") as string) %>'
                                OnClientClick='<%# "showConfirmModal(\"" + Eval("SubmissionID") + ";RawDataPath\"); return false;" %>' />


                        </td>

                        <td class="file-cell">

                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=ProposalCopyPath") %>'
                                CssClass="download-button"
                                Text="Download"
                                ToolTip='<%# System.IO.Path.GetFileName(Eval("ProposalCopyPath").ToString()) %>'
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("ProposalCopyPath") as string) %>' />
                            <asp:Button ID="Button6" runat="server"
                                Text="⏎"
                                CssClass="download-button"
                                BackColor="#dc3545"
                                ForeColor="White"
                                Visible='<%# !string.IsNullOrEmpty(Eval("ProposalCopyPath") as string) %>'
                                OnClientClick='<%# "showConfirmModal(\"" + Eval("SubmissionID") + ";ProposalCopyPath\"); return false;" %>' />

                        </td>

                        <td class="file-cell">

                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=Instrumentation") %>'
                                CssClass="download-button"
                                Text="Download"
                                ToolTip='<%# System.IO.Path.GetFileName(Eval("Instrumentation").ToString()) %>'
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("Instrumentation") as string) %>' />

                            <asp:Button ID="Button2" runat="server"
                                Text="⏎"
                                CssClass="download-button"
                                BackColor="#dc3545"
                                ForeColor="White"
                                Visible='<%# !string.IsNullOrEmpty(Eval("Instrumentation") as string) %>'
                                OnClientClick='<%# "showConfirmModal(\"" + Eval("SubmissionID") + ";Instrumentation\"); return false;" %>' />

                        </td>

                        <td class="file-cell">

                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=StatOutputPath") %>'
                                CssClass="download-button"
                                Text="Download"
                                ToolTip='<%# System.IO.Path.GetFileName(Eval("StatOutputPath").ToString()) %>'
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("StatOutputPath") as string) %>' />

                            <asp:Button ID="Button3" runat="server"
                                Text="⏎"
                                CssClass="download-button"
                                BackColor="#dc3545"
                                ForeColor="White"
                                Visible='<%# !string.IsNullOrEmpty(Eval("StatOutputPath") as string) %>'
                                OnClientClick='<%# "showConfirmModal(\"" + Eval("SubmissionID") + ";StatOutputPath\"); return false;" %>' />

                        </td>

                        <td class="file-cell">

                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=DeclarationPath") %>'
                                CssClass="download-button"
                                Text="Download"
                                ToolTip='<%# System.IO.Path.GetFileName(Eval("DeclarationPath").ToString()) %>'
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("DeclarationPath") as string) %>' />

                            <asp:Button ID="Button4" runat="server"
                                Text="⏎"
                                CssClass="download-button"
                                BackColor="#dc3545"
                                ForeColor="White"
                                Visible='<%# !string.IsNullOrEmpty(Eval("DeclarationPath") as string) %>'
                                OnClientClick='<%# "showConfirmModal(\"" + Eval("SubmissionID") + ";DeclarationPath\"); return false;" %>' />

                        </td>

                        <td class="file-cell">

                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?submissionId=" + Eval("SubmissionID") + "&field=ReceiptPath") %>'
                                CssClass="download-button"
                                Text="Download"
                                ToolTip='<%# System.IO.Path.GetFileName(Eval("ReceiptPath").ToString()) %>'
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("ReceiptPath") as string) %>' />
                            <asp:Button ID="Button5" runat="server"
                                Text="⏎"
                                CssClass="download-button"
                                BackColor="#dc3545"
                                ForeColor="White"
                                Visible='<%# !string.IsNullOrEmpty(Eval("ReceiptPath") as string) %>'
                                OnClientClick='<%# "showConfirmModal(\"" + Eval("SubmissionID") + ";ReceiptPath\"); return false;" %>' />

                        </td>


                        <td>
                            <asp:Button ID="btnGenerateCert" runat="server"
                                Text="📝 Create Certificate"
                                OnClientClick='<%# $"showModal({Eval("SubmissionID")}); return false;" %>'
                                CssClass="cert-button"
                                UseSubmitBehavior="false" />
                            <span class="file-name cert-uploaded"
                                title='<%# Eval("Remarks") != DBNull.Value ? System.IO.Path.GetFileName(Eval("Remarks").ToString()) : "" %>'>
                                <%# Eval("Remarks") != DBNull.Value && !string.IsNullOrEmpty(Eval("Remarks").ToString()) ? "Uploaded" : "" %>
                            </span>


                            <div class="modal-overlay" id="modal_<%# Eval("SubmissionID") %>">
                                <div class="modal-box">
                                    <span class="close-modal" onclick="closeModal('<%# Eval("SubmissionID") %>')">&times;</span>
                                    <asp:Label Text="Author/s:" runat="server" /><br />
                                    <asp:TextBox ID="txtAuthors" runat="server" /><br />
                                    <asp:Label Text="Title:" runat="server" /><br />
                                    <asp:TextBox ID="txtTitle" runat="server" TextMode="MultiLine" Rows="2" /><br />
                                    <asp:Label Text="Statistician Name:" runat="server" /><br />
                                    <asp:TextBox ID="txtStatistician" runat="server" /><br />
                                    <asp:Label Text="Date (e.g., 1st day of January 2025):" runat="server" /><br />
                                    <asp:TextBox ID="txtCertDate" runat="server" /><br />
                                    <asp:Button ID="btnSubmitCert" runat="server" Text="Submit"
                                        CssClass="cert-button"
                                        OnClick="btnSubmitCert_Click"
                                        OnClientClick='<%# "document.getElementById(\"" 
        + ((Label)Container.FindControl("lblUploading")).ClientID 
        + "\").style.display = \"inline\";" %>' />
                                    
                                    <asp:Label ID="lblUploading" runat="server"
    Text="Uploading..."
    Style="display: none; margin-left: 6px;" />

                                    <div style="text-align: center; margin: 10px 0; font-weight: bold;">or</div>

                                    <asp:FileUpload ID="fuCertUpload" runat="server" CssClass="form-control" />
<asp:Button ID="btnUploadCert" runat="server" Text="Upload Certificate"
    CssClass="cert-button"
    OnClick="btnUploadCert_Click"
    OnClientClick='<%# "document.getElementById(\"" 
        + ((Label)Container.FindControl("lblUploadingUpload")).ClientID 
        + "\").style.display = \"inline\";" %>' />

<asp:Label ID="lblUploadingUpload" runat="server"
    Text="Uploading..."
    Style="display: none; margin-left: 6px;" />

                                    

                                </div>
                            </div>
                            <asp:Label ID="lblStatus" runat="server" ForeColor="Green" />
                            <asp:HiddenField ID="HiddenSubmissionID" runat="server" Value='<%# Eval("SubmissionID") %>' />
                            <asp:Label ID="lblLoginID" runat="server" Text='<%# Eval("LoginID") %>' Visible="false" />
                        </td>
                        <td>
                            <asp:Button ID="btnReturnCertification" runat="server"
                                CommandName="ReturnCertification"
                                CommandArgument='<%# Eval("SubmissionID") %>'
                                Text="Send" CssClass="btn btn-primary btn-sm"
                                OnClientClick="showSendingStatus(this);" />

                            <asp:Label ID="lblReturnStatus" runat="server"
                                CssClass="return-status"
                                Style="display: block; margin-top: 4px; min-height: 18px;" />

                        </td>
                        <td>
                            <asp:CheckBox
                                ID="highlight"
                                runat="server"
                                Text="📌"
                                AutoPostBack="true"
                                OnCheckedChanged="highlight_CheckedChanged"
                                Checked='<%# Eval("IsHighlighted") != DBNull.Value && Convert.ToBoolean(Eval("IsHighlighted")) %>' />
                            <asp:HiddenField ID="HiddenField1" runat="server" Value='<%# Eval("SubmissionID") %>' />
                            </br>
                           <asp:CheckBox ID="chkDisable" runat="server"
                               AutoPostBack="true"
                               Text="✔️"
                               OnCheckedChanged="Disable_CheckedChanged"
                               Checked='<%# Eval("IsDisabled").ToString() == "True" %>' />
                            <asp:HiddenField ID="HiddenField2" runat="server" Value='<%# Eval("SubmissionID") %>' />
                        </td>

                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>

            <div id="confirmModal" class="modal-overlay">
                <div class="modal-box" style="max-width: 400px; text-align: center;">
                    <span class="close-modal" onclick="hideConfirmModal()">&times;</span>
                    <h3 style="margin-bottom: 20px;">Confirm Return</h3>
                    <p>Are you sure you want to return this file?</p>
                    <asp:Button ID="btnConfirmReturn" runat="server"
                        Text="Yes, Return File"
                        CssClass="cert-button"
                        OnClick="ConfirmReturn_Click" />
                    <button type="button" class="cert-button"
                        style="background-color: gray; margin-left: 10px;"
                        onclick="hideConfirmModal()">
                        Cancel</button>
                    <!-- hidden field to store argument -->
                    <asp:HiddenField ID="hfReturnArg" runat="server" />
                </div>
            </div>

            <div style="text-align: center; margin-top: 20px;">
                <asp:Button ID="btnPrev" runat="server" Text="« Prev" OnClick="btnPrev_Click" />
                <asp:Label ID="lblPageInfo" runat="server" Text="" Style="margin: 0 10px;" />
                <asp:Button ID="btnNext" runat="server" Text="Next »" OnClick="btnNext_Click" />
            </div>

        </div>
    </div>
</asp:Content>


