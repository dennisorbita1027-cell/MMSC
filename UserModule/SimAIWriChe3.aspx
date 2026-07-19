<%@ Page Title="SimAIWriChe" Language="C#" MasterPageFile="~/UserModule/HomeMaster.master" AutoEventWireup="true" CodeBehind="SimAIWriChe3.aspx.cs" Inherits="CASApp1.UserModule.SimAIWriChe3" %>


<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .checklist-title {
            font-size: 32px;
            font-weight: bold;
            text-align: center;
            margin: 20px 0 30px 0;
        }

        .container-fluid {
            display: flex;
            max-width: 1600px;
            margin: auto;
            background-color: #fff;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            padding: 20px;
            gap: 20px;
            flex-wrap: wrap;
        }

        .instruction-panel {
            flex: 1 1 300px;
            background-color: #f8f9fa;
            padding: 20px;
            border-right: 1px solid #ccc;
            box-sizing: border-box;
            min-width: 280px;
        }

            .instruction-panel h3 {
                padding-bottom: 10px;
            }

        .form-panel {
            flex: 2 1 600px;
            padding: 20px;
            box-sizing: border-box;
            min-width: 280px;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 16px;
            table-layout: fixed;
            word-wrap: break-word;
        }

        th, td {
            border: 1px solid #ccc;
            padding: 12px 16px;
            vertical-align: top;
            word-wrap: break-word;
        }

        th {
            background-color: #f1f1f1;
            text-align: left;
        }

        input[type="text"],
        input[type="file"] {
            width: 100%;
            padding: 8px;
            box-sizing: border-box;
            font-size: 14px;
        }

        .checklist-button {
            margin-top: 20px;
            padding: 10px 20px;
            font-size: 16px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            display: inline-block;
            min-width: 120px;
            text-align: center;
            transition: background-color 0.3s ease;
        }

            .checklist-button:hover {
                background-color: #0056b3;
            }

        .checkbox-group label,
        .form-panel label {
            display: flex;
            align-items: center;
            margin-bottom: 8px;
            cursor: pointer;
            font-size: 14px;
        }

            .checkbox-group label input[type="checkbox"],
            .form-panel input[type="checkbox"] {
                margin-right: 10px;
                transform: scale(1.2);
                cursor: pointer;
            }

        .text-success {
            color: green;
            margin-top: 10px;
            font-size: 16px;
        }

                .error-text {
    color: black;
    font-size: medium;
    margin-top: 20px; 
    font-weight: bold;
    font-style: italic;
    text-align: center; 
    display: block;
}

        .download-button {
            display: inline-block;
            margin-top: 10px;
            padding: 6px 12px;
            background-color: #16a300;
            color: white;
            font-weight: bold;
            text-decoration: none;
            border-radius: 5px;
            font-size: 14px;
            transition: background-color 0.3s ease;
        }

            .download-button:hover {
                background-color: dimgrey;
                color: white;
            }

        @media (max-width: 768px) {
            .container-fluid {
                flex-direction: column;
                padding: 10px;
            }

            .instruction-panel,
            .form-panel {
                width: 100% !important;
                border-right: none;
                padding: 10px 0;
                min-width: auto;
            }

            table {
                font-size: 14px;
                display: block;
                overflow-x: auto;
                white-space: nowrap;
                -webkit-overflow-scrolling: touch; /* smooth scrolling on iOS */
            }

            th, td {
                padding: 8px 10px;
            }

            input[type="text"],
            input[type="file"] {
                font-size: 14px;
            }

            .checklist-title {
                font-size: 24px;
            }

            .checklist-button {
                width: 100%;
                font-size: 16px;
                padding: 12px;
                min-width: unset;
            }

            .download-button {
                display: block;
                text-align: center;
                width: 100%;
                margin-top: 15px;
            }

            .checkbox-group label,
            .form-panel input[type="checkbox"] {
                transform: scale(1.3);
                margin-right: 8px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="checklist-title">Similarity & AI Writing Check</div>

    <div class="container-fluid">
        <asp:Label ID="lblPageStatus" runat="server" CssClass="error-text" />
        <!-- Left Column -->
        <div class="instruction-panel">
            <h3>📌 Attention Here</h3>
            <h4><strong>General Instruction</strong></h4>
            <ul>
                <li>Prepare all required details and documents before accomplishing this form. Incomplete submissions will not be processed.</li>
                <li>The Center can process only 30 papers per day, and all submissions will be handled on a first-come, first-served basis.</li>
                <li>Follow-ups are not necessary. Results will be sent directly to your registered email address once available.</li>
                <li>Do not send documents multiple times. For group submissions, only the group leader should upload the file.</li>
                <li>If you have already submitted your document, wait for the testing to be completed before resubmitting.
                        Re-submissions should only be made after feedback is received.</li>
                <li>Do not change accounts. Whoever submitted the document for the initial testing should also submit for all succeeding revisions. 
                        The same account and name must be used throughout the process</li>
            </ul>
        </div>

        <!-- Right Column -->
        <div class="form-panel">
            <table class="table checklist-table table-bordered">
                <thead>
                    <tr>
                        <th>Requirement</th>
                        <th>File Upload</th>
                        <th>Note</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>Title</td>
                        <td>
                            <asp:TextBox ID="txtTitle" runat="server" CssClass="textbox" />
                        </td>
                        <td>Encode the title of your study. Make sure it is correct,
                                as this will appear in your certification. Be particular with capitalization and italicization of letters and words.</td>
                    </tr>
                    <tr>
                        <td>Author/s</td>
                        <td>
                            <asp:TextBox ID="txtAuthors" runat="server" CssClass="textbox" />
                        </td>
                        <td>Encode the author(s) following this format: Author1, Author2, Author3, 
                                ... Each author's name should follow this format: First Name MI. Last Name. Example: Juan A. Dela Cruz, Maria B. Santos,
                                Carlo C. Reyes</td>
                    </tr>
                    <tr>
                        <td>Official Receipt</td>
                        <td>
                            <asp:TextBox ID="txtReceiptNo" runat="server" CssClass="textbox" Style="flex: 1;" />
                            <span>Copy</span>
                            <asp:FileUpload ID="fuCopy" runat="server" />
                        </td>
                        <td>Encode the receipt number and Amount paid. Upload a scanned copy of your Cashier’s Receipt.
                                It must be clear and must reflect the researcher’s name (for individual submissions) or the group leader’s name 
                                (for group submissions). First, secure an Order of Payment from the Accounting Office or Cashier, and use the following
                                billing details: TF 130B, LBP – Solano Branch</td>
                    </tr>
                    <tr>
                        <td>Upload Document</td>
                        <td>
                            <asp:FileUpload ID="fuDocument" runat="server" /></td>
                        <td>Upload a single file, preferably a document (*.docx), containing only the following: title, author(s), Chapters I to V, and 
                                Literature Cited/References. Remove all other parts. Do not include headers, footers, tables and table titles, or figures and figure titles. </td>
                    </tr>
                    <tr>
                        <td>Course</td>
                        <td>
                            <asp:TextBox ID="txtCollege" runat="server" CssClass="textbox" /></td>
                        <td>Encode your degree exactly as it appears on your ID</td>
                    </tr>
                    <tr>
                        <td>Type</td>
                        <td>
        <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Vertical">
            <asp:ListItem Text="Dissertation" Value="Dissertation" />
            <asp:ListItem Text="Master's Thesis" Value="Masters_thesis" />
            <asp:ListItem Text="Undergraduate Thesis" Value="Undergraduate_Thesis" />
            <asp:ListItem Text="Narratives" Value="Narratives" />
        </asp:RadioButtonList>
        <label>Others:</label>
        <asp:TextBox ID="txtOthers" runat="server" Width="150px" placeholder="Specify here..." />
    </td>
                        <td>Select one</td>
                    </tr>
                    <tr>

                        <td>Revision No.</td>
                        <td>
                            <asp:TextBox ID="txtRevNo" runat="server" CssClass="textbox" /></td>
                        <td>Encode the revision number. Use 0 for initial testing, 1 for first revision, 2 for second revision, 3 for third revision, and so on.</td>

                    </tr>

                </tbody>
            </table>
            <asp:Button ID="btnSubmit" runat="server" Text="Submit" CssClass="checklist-button" OnClick="btnSubmit_Click" />
            <asp:Label ID="lblMessage" runat="server" CssClass="text-success" />

            <br />

            <a href="/Templates/AIDeclaration.docx"
                download=""
                class="download-button">Download Declaration of AI Use Template
            </a>

            <h3>▼ Upload Declaration of AI Use ▼</h3>
            <asp:FileUpload ID="fuDeclaration" runat="server" CssClass="form-control" />
            <asp:Button ID="btnUploadDeclaration" runat="server" Text="Upload Declaration" CssClass="checklist-button" OnClick="btnUploadDeclaration_Click" />
            <asp:Label ID="lblUploadStatus" runat="server" ForeColor="Green" />
        </div>
    </div>

</asp:Content>


