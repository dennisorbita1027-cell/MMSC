<%@ Page Title="Reports&Certification" Language="C#" MasterPageFile="~/UserModule/HomeMaster.master" AutoEventWireup="true" CodeBehind="CertRemarks.aspx.cs" Inherits="CASApp1.UserModule.CertRemarks" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style runat="server">
        .page-wrapper {
            max-width: 1000px;
            margin: 20px auto;
            padding: 0 15px;
            box-sizing: border-box;
        }

        .processor-title {
            font-size: 24px;
            font-weight: bold;
            margin-top: 20px;
            margin-bottom: 20px;
            text-align: center;
            color: #222;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
            text-align: center;
            border: 1px solid #444;
            font-size: 16px;
            min-width: 600px;
        }

        th, td {
            border: 1px solid #888;
            padding: 10px 10px;
            word-break: break-word;
        }

        th {
            white-space: nowrap;
        }

        .lbl-message {
            display: block;
            margin-bottom: 15px;
            color: red;
            font-weight: bold;
            text-align: center;
        }
        .cert-table {
    width: 100%;
    border-collapse: collapse;
    text-align: center;
    margin-top: 20px;
    border: 1px solid #444;
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

        @media (max-width: 768px) {
            table {
                display: block;
                overflow-x: auto;
                white-space: nowrap;
                font-size: 14px;
            }

            th, td {
                padding: 8px 6px;
            }

            .processor-title {
                font-size: 20px;
                margin-top: 15px;
                margin-bottom: 15px;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-wrapper">
        <div class="processor-title"><strong>Certification Report</strong></div>
        <div style="padding: 20px 50px;">

            <asp:Label ID="lblMessage" runat="server" CssClass="lbl-message" Visible="false" />


            <%-- SIMILARITY MODULE TABLE --%>
            <asp:Repeater ID="rptSim" runat="server">
                <HeaderTemplate>
                    <table class="cert-table">

                        <tr>
                            <th>Date Returned</th>
                            <th>Similarity Report</th>
                            <th>AI Writing Report</th>
                            <th>MMSC Report</th>
                            <th>MMSC Certification</th>
                            <th>Message</th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("DateSent") %></td>

                        <td>
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?returnId=" + Eval("ReturnID") + "&field=SimilarityFilePath") %>'
                                CssClass="download-button"
                                Text="Download Report"
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("SimilarityFilePath") as string) %>' />
                        </td>

                        <td>
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?returnId=" + Eval("ReturnID") + "&field=AIFilePath") %>'
                                CssClass="download-button"
                                Text="Download Report"
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("AIFilePath") as string) %>' />
                        </td>

                        <td>
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?returnId=" + Eval("ReturnID") + "&field=MMSCReportFilePath") %>'
                                CssClass="download-button"
                                Text="Download Report"
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("MMSCReportFilePath") as string) %>' />
                        </td>

                        <td>
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?returnId=" + Eval("ReturnID") + "&field=MMSCCertFilePath") %>'
                                CssClass="download-button"
                                Text="Download Certificate"
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("MMSCCertFilePath") as string) %>' />
                        </td>

                        <td><%# Eval("MessageToStudent") %></td>
                    </tr>

                </ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>

            <%-- STATCOMVAL MODULE TABLE --%>
            <asp:Repeater ID="rptStat" runat="server">
                <HeaderTemplate>
                    <table border="1" style="width: 100%; border-collapse: collapse; text-align: center; margin-top: 30px;">
                        <tr>
                            <th>Date Returned</th>
                            <th>StatComVal Certification</th>
                            <th>Message</th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("DateSent") %></td>
                        <td>
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Download.ashx?returnId=" + Eval("ReturnID") + "&field=StatComCertPath") %>'
                                CssClass="download-button"
                                Text="Download Certificate"
                                Target="_blank"
                                Visible='<%# !string.IsNullOrEmpty(Eval("StatComCertPath") as string) %>' />

                        </td>
                        <td><%# Eval("MessageToStudent") %></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></table></FooterTemplate>
            </asp:Repeater>

        </div>
    </div>

</asp:Content>
