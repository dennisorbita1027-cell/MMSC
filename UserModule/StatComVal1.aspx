<%@ Page Title="StatComVal" Language="C#" MasterPageFile="~/UserModule/HomeMaster.master" AutoEventWireup="true" CodeBehind="StatComVal1.aspx.cs" Inherits="CASApp1.UserModule.StatComVal1" %>

<asp:Content ID="HeadContent3" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .checklist-title {
            font-size: 40px;
            font-weight: bold;
            margin-bottom: 30px;
            margin-top: 30px;
            text-align: center;
        }

        .container-fluid {
            max-width: 1600px;
            margin: auto;
            background-color: #fff;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            padding: 20px;
        }

        .checklist-table {
            margin: 40px auto;
            border-collapse: collapse;
            width: 80%;
            max-width: 800px;
            font-size: 20px;
        }

            .checklist-table td {
                padding: 12px 16px;
                vertical-align: middle;
            }

                .checklist-table td:first-child {
                    text-align: right;
                    white-space: nowrap;
                    width: 40%;
                }

                .checklist-table td:last-child {
                    text-align: left;
                    margin-left: 10px;
                }

        .checklist-button {
            padding: 4px 10px;
            font-size: 12px;
            border-radius: 3px;
            margin-top: 5px;
            display: block;
        }

        .error-text {
            color: red;
            font-size: 16px;
            padding-left: 10px;
        }

        .submitted {
            background-color: #d4edda;
            padding: 6px 10px;
            font-weight: bold;
        }

        .checklist-title {
            font-size: 32px;
            font-weight: bold;
            text-align: center;
            margin-top: 20px;
            margin-bottom: 30px;
        }

        .checklist-button {
            margin-top: 5px;
            display: block;
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
        }

            .download-button:hover {
                background-color: dimgrey;
                color: white;
            }

        .table-responsive {
            overflow-x: auto;
        }

        .table th {
            white-space: normal !important;
            word-wrap: break-word;
            word-break: break-word;
        }

        @media (max-width: 768px) {
            .checklist-button,
            input[type="file"],
            .submitted,
            .error-text {
                width: 100%;
                margin-bottom: 0.5rem;
                text-align: center;
            }

            .table td {
                font-size: 14px; 
            }
        }

        .modal-dialog {
            max-width: 95%;
            margin: 1.75rem auto;
        }
    </style>

    <link href="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>

    <script>
        function showModal(id) {
            $('#' + id).modal('show');
        }
    </script>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server">
    <div class="checklist-title">Statistics Computation Validation</div>

    <div id ="containerDiv" runat="server" class="container-fluid">
        <asp:Label ID="lblPageDisabled" runat="server" CssClass="error-text" Visible="false"></asp:Label>
        <div class="row gx-3">

            <!-- Left Column: General Instruction -->
            <div class="col-md-3 p-3 border-start border-top border-bottom rounded-start bg-light">
                <h3>📌 Attention Here</h3>
                <h4><strong>General Instruction</strong></h4>
                <ul>
                    <li>Make sure that you have all the required details and documents before accomplishing this form.</li>
                    <li>Download the <strong>Statistician's Checklist</strong> and have your statistician accomplish it.</li>
                    <li>Any missing or unsuccessfully uploaded documents will not be processed or forwarded to the system.</li>
                    <li>The Center can validate only five(5) computation per day and will issue your certification immeadiately after validation.</li>
                </ul>
            </div>


            <div class="col-md-9 p-3 border-top border-end border-bottom rounded-end bg-white">
                <div class="table-responsive">

                    <table class="table table-bordered bg-grey">
                        <thead>
                            <tr>
                                <th style="width: 25%;">Requirement</th>
                                <th style="width: 25%;">File Upload</th>
                                <th style="width: 50%;">Note</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>Raw Data</td>
                                <td>
                                    <asp:FileUpload ID="fuRawData" runat="server" />
                                    <asp:LinkButton ID="btnOpenModalRaw" runat="server" CssClass="btn btn-success checklist-button" OnClientClick="$('#modalRaw').modal('show'); return false;">Submit</asp:LinkButton>
                                    <asp:Button ID="btnUndoRaw" runat="server" Text="Undo" CssClass="btn btn-danger checklist-button" OnClick="btnUndoRaw_Click" Visible="false" />
                                    <asp:Label ID="lblRawStatus" runat="server" CssClass="submitted" Visible="false" Text="✔️ Submitted" />
                                    <asp:Label ID="lblRawError" runat="server" CssClass="error-label" ForeColor="Red" />
                                </td>
                                <td>Upload copy of your Raw Data Matrix in an MS Excel file, ready for analysis. 
                  This should be the same file used by your statistician in addressing the statistical problems or objectives of your study.</td>

                            </tr>

                            <tr>
                                <td>Proposal Copy</td>
                                <td>
                                    <asp:FileUpload ID="fuProposal" runat="server" />
                                    <asp:LinkButton ID="btnOpenModalProposal" runat="server" CssClass="btn btn-success checklist-button" OnClientClick="$('#modalProposal').modal('show'); return false;">Submit</asp:LinkButton>
                                    <asp:Button ID="btnUndoProposal" runat="server" Text="Undo" CssClass="btn btn-danger checklist-button" OnClick="btnUndoProposal_Click" Visible="false" />
                                    <asp:Label ID="lblProposalStatus" runat="server" CssClass="submitted" Visible="false" Text="✔️ Submitted" />
                                    <asp:Label ID="lblProposalError" runat="server" CssClass="error-label" ForeColor="Red" />
                                </td>
                                <td>Upload a copy of your approved proposal containing the title, author(s), problem statement or objectives of the study, the entire Chapter III.</td>
                            </tr>

                            <tr>
                                <td>Instrument Used in Data Gathering</td>
                                <td>
                                    <asp:FileUpload ID="fuInstrument" runat="server" />
                                    <asp:LinkButton ID="BtnOpenModalInstrument" runat="server" CssClass="btn btn-success checklist-button" OnClientClick="$('#modalInstrument').modal('show'); return false;">Submit</asp:LinkButton>
                                    <asp:Button ID="btnUndoInstrument" runat="server" Text="Undo" CssClass="btn btn-danger checklist-button" OnClick="btnUndoInstrument_Click" Visible="false" />
                                    <asp:Label ID="lblInstrumentStatus" runat="server" CssClass="submitted" Visible="false" Text="✔️ Submitted" />
                                    <asp:Label ID="lblInstrumentError" runat="server" CssClass="error-label" ForeColor="Red" />
                                </td>
                                <td>For survey-type studies, upload a copy or copies of all instruments used in your data gathering. 
                    No need to upload for experimental studies or studies utilizing secondary data.</td>
                            </tr>

                            <tr>
                                <td>Statistician Computation or Analyses</td>
                                <td>
                                    <asp:FileUpload ID="fuStatOutput" runat="server" />
                                    <asp:LinkButton ID="btnOpenModalStat" runat="server" CssClass="btn btn-success checklist-button" OnClientClick="$('#modalStat').modal('show'); return false;">Submit</asp:LinkButton>
                                    <asp:Button ID="btnUndoStat" runat="server" Text="Undo" CssClass="btn btn-danger checklist-button" OnClick="btnUndoStat_Click" Visible="false" />
                                    <asp:Label ID="lblStatStatus" runat="server" CssClass="submitted" Visible="false" Text="✔️ Submitted" />
                                    <asp:Label ID="lblStatError" runat="server" CssClass="error-text" ForeColor="Red" />
                                </td>
                                <td>Upload all outputs from your statistician, arranged according to each problem statement or objective of your study. 
                The file will be used for validation, to compare the Center’s computation with your statistician’s results.</td>
                            </tr>

                            <tr>
                                <td>Statistician Certification</td>
                                <td>
                                    <asp:FileUpload ID="fuDeclaration" runat="server" />
                                    <asp:LinkButton ID="btnOpenModalDecl" runat="server" CssClass="btn btn-success checklist-button" OnClientClick="$('#modalDecl').modal('show'); return false;">Submit</asp:LinkButton>
                                    <asp:Button ID="btnUndoDecl" runat="server" Text="Undo" CssClass="btn btn-danger checklist-button" OnClick="btnUndoDecl_Click" Visible="false" />
                                    <asp:Label ID="lblDeclStatus" runat="server" CssClass="submitted" Visible="false" Text="✔️ Submitted" />
                                    <asp:Label ID="lblDeclError" runat="server" CssClass="error-text" ForeColor="Red" />
                                </td>
                                <td>Upload your statistician's certification stating that he/she checked the number of respondents and sampling techniques, 
                assisted the researcher in preparing the raw data matrix, verified the accomplished survey questionnaires, listed the statistical packages used, and affirmed that his/her outputs were based on the analysis of the data provided.
                <a href="/Templates/Statisticians-Certification-Form.docx"
                    download=""
                    class="download-button">Download Statistician's Checklist
                </a>

                                </td>


                                <tr>
                                    <td>Offcial Receipt</td>
                                    <td>
                                        <asp:FileUpload ID="fuReceipt" runat="server" />
                                        <asp:LinkButton ID="btnOpenModalReceipt" runat="server" CssClass="btn btn-success checklist-button" OnClientClick="$('#modalReceipt').modal('show'); return false;">Submit</asp:LinkButton>
                                        <asp:Button ID="btnUndoReceipt" runat="server" Text="Undo" CssClass="btn btn-danger checklist-button" OnClick="btnUndoReceipt_Click" Visible="false" />
                                        <asp:Label ID="lblReceiptStatus" runat="server" CssClass="submitted" Visible="false" Text="✔️ Submitted" />
                                        <asp:Label ID="lblReceiptError" runat="server" CssClass="error-text" ForeColor="Red" />

                                    </td>
                                    <td>Upload a scanned copy of your Cashier’s Receipt. It must be clear and must reflect the researcher’s name (for individual submissions) or the group leader’s name (for group submissions). 
                First, secure an Order of Payment from the Accounting Office or Cashier, and use the following billing details: TF 130A, LBP – Solano Branch, ₱500.00.</td>
                                </tr>
                        </tbody>
                    </table>
                </div>

                <!-- Modals -->

                <!-- Modal for Raw Data -->
                <div class="modal fade" id="modalRaw" tabindex="-1" role="dialog">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-warning">
                                <h5 class="modal-title">Confirm Raw Data Submission</h5>
                                <button type="button" class="close" data-dismiss="modal"><span>&times;</span></button>
                            </div>
                            <div class="modal-body">Submit Raw Data File?</div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                                <asp:Button ID="btnSubmitRaw" runat="server" Text="Yes, Submit" CssClass="btn btn-primary" OnClick="btnSubmitRaw_Click" />
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Modal for Proposal -->
                <div class="modal fade" id="modalProposal" tabindex="-1" role="dialog">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-warning">
                                <h5 class="modal-title">Confirm Proposal Copy Submission</h5>
                                <button type="button" class="close" data-dismiss="modal"><span>&times;</span></button>
                            </div>
                            <div class="modal-body">Submit Proposal Copy?</div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                                <asp:Button ID="btnSubmitProposal" runat="server" Text="Yes, Submit" CssClass="btn btn-primary" OnClick="btnSubmitProposal_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Modal for instrument -->
                <div class="modal fade" id="modalInstrument" tabindex="-1" role="dialog">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-warning">
                                <h5 class="modal-title">Confirm Instrumentation Submission</h5>
                                <button type="button" class="close" data-dismiss="modal"><span>&times;</span></button>
                            </div>
                            <div class="modal-body">Submit Instrumentation?</div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                                <asp:Button ID="btnSubmitInstrument" runat="server" Text="Yes, Submit" CssClass="btn btn-primary" OnClick="btnSubmitInstrument_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Modal for Statistician Output -->
                <div class="modal fade" id="modalStat" tabindex="-1" role="dialog">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-warning">
                                <h5 class="modal-title">Confirm Statistician Output Submission</h5>
                                <button type="button" class="close" data-dismiss="modal"><span>&times;</span></button>
                            </div>
                            <div class="modal-body">Submit Statistician's Output?</div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                                <asp:Button ID="btnSubmitStat" runat="server" Text="Yes, Submit" CssClass="btn btn-primary" OnClick="btnSubmitStat_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Modal for Declaration -->
                <div class="modal fade" id="modalDecl" tabindex="-1" role="dialog">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-warning">
                                <h5 class="modal-title">Confirm Declaration Submission</h5>
                                <button type="button" class="close" data-dismiss="modal"><span>&times;</span></button>
                            </div>
                            <div class="modal-body">Submit Statistician Declaration?</div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                                <asp:Button ID="btnSubmitDecl" runat="server" Text="Yes, Submit" CssClass="btn btn-primary" OnClick="btnSubmitDecl_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Modal for Receipt -->
                <div class="modal fade" id="modalReceipt" tabindex="-1" role="dialog">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-warning">
                                <h5 class="modal-title">Confirm Receipt Submission</h5>
                                <button type="button" class="close" data-dismiss="modal"><span>&times;</span></button>
                            </div>
                            <div class="modal-body">Submit Receipt File?</div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                                <asp:Button ID="btnSubmitReceipt" runat="server" Text="Yes, Submit" CssClass="btn btn-primary" OnClick="btnSubmitReceipt_Click" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>


        </div>
    </div>

</asp:Content>

