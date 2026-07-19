<%@ Page Title="Reports" Language="C#" MasterPageFile="~/ProcessorModule/ProcessorMaster.master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="CASApp1.ProcessorModule.Reports" %>

<asp:Content ID="HeadContent4" ContentPlaceHolderID="HeadContent" runat="server">

    <style>
        .module-tab {
            background-color: white;
            color: #007bff;
            border: 2px solid #007bff;
            padding: 10px 20px;
            font-weight: bold;
            cursor: pointer;
            border-radius: 5px;
            margin-right: 10px;
        }

        .active-tab {
            background-color: #007bff !important;
            color: white !important;
        }

        .report-section {
            padding: 20px;
            margin-top: 80px;
        }

        .table-slider {
            overflow-x: auto;
            overflow-y: hidden;
            white-space: nowrap;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 5px;
        }

        .truncate {
            max-width: 120px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            display: inline-block;
            vertical-align: middle;
        }
    </style>


</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="MainContent" runat="server">
    <div class="report-section" style="padding: 20px;">
        <!-- Module Selector Tabs -->
        <div style="margin-bottom: 20px;">
            <asp:Button ID="btnStatCom" runat="server" Text="StatComVal Reports"
                CssClass="module-tab active-tab" OnClick="btnStatCom_Click" />
            <asp:Button ID="btnSimilarity" runat="server" Text="Similarity Reports"
                CssClass="module-tab" OnClick="btnSimilarity_Click" />
        </div>

        <!-- Shared Search Bar -->
        <div class="search-filter-bar" style="margin-bottom: 20px;">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" Placeholder="Search by Author or Title" Width="250px" />
            <asp:Button ID="btnSearch" runat="server" Text="🔍 Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
            <asp:Button ID="btnReset" runat="server" Text="🔄 Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" Style="margin-left: 10px;" />
        </div>


        <asp:Label ID="lblNoResults" runat="server" ForeColor="Red" Font-Bold="true" Visible="false"></asp:Label>

        <!-- StatCom Reports Grid -->
        <div class="table-slider">
            <!-- Remove .table-slider wrapper -->
            <div style="overflow-x: auto; width: 100%;">
                <asp:GridView ID="gvStatComReports" runat="server" CssClass="table table-striped"
                    AllowPaging="true" PageSize="20" AutoGenerateColumns="False" DataKeyNames="ReturnID"
                    OnPageIndexChanging="gvReports_PageIndexChanging" OnRowCommand="gvReports_RowCommand"
                    Visible="false">
                    <Columns>
                        <asp:BoundField HeaderText="#" DataField="RowNumber" />
                        <asp:BoundField HeaderText="Date Sent" DataField="DateSent" DataFormatString="{0:MMM dd, yyyy}" />

                        <%-- Apply truncate --%>
                        <asp:TemplateField HeaderText="Author(s)">
                            <ItemTemplate>
                                <span class="truncate" title='<%# Eval("Author") %>'><%# Eval("Author") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Title">
                            <ItemTemplate>
                                <span class="truncate" title='<%# Eval("Title") %>'><%# Eval("Title") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField HeaderText="Sent By" DataField="ProcessorName" />

                        <asp:TemplateField HeaderText="StatCom Cert">
                            <ItemTemplate>
                                <span class="truncate" title='<%# GetFileName(Eval("StatComCertPath")) %>'>
                                    <%# GetFileName(Eval("StatComCertPath")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:Button runat="server" Text="Download" CommandName="DownloadStatCom"
                                    CommandArgument='<%# Eval("ReturnID") %>' CssClass="btn btn-outline-primary btn-sm" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>


            <!-- 📄 Similarity Reports Grid -->
            <!-- 📄 Similarity Reports Grid -->
            <div style="overflow-x: auto; width: 100%;">
                <asp:GridView ID="gvSimilarityReports" runat="server" CssClass="table table-striped"
                    AllowPaging="true" PageSize="20" AutoGenerateColumns="False" DataKeyNames="ReturnID"
                    OnPageIndexChanging="gvReports_PageIndexChanging" OnRowCommand="gvReports_RowCommand"
                    Visible="false">
                    <Columns>
                        <asp:BoundField HeaderText="#" DataField="RowNumber" />
                        <asp:BoundField HeaderText="Date Sent" DataField="DateSent" DataFormatString="{0:MMM dd, yyyy}" />

                        <%-- Apply truncate to long text --%>
                        <asp:TemplateField HeaderText="Author(s)">
                            <ItemTemplate>
                                <span class="truncate" title='<%# Eval("Author") %>'><%# Eval("Author") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Title">
                            <ItemTemplate>
                                <span class="truncate" title='<%# Eval("Title") %>'><%# Eval("Title") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField HeaderText="Sent By" DataField="ProcessorName" />
                        <asp:BoundField HeaderText="Case #" DataField="CaseNumber" />

                        <asp:TemplateField HeaderText="AI Writing Report">
                            <ItemTemplate>
                                <span class="truncate" title='<%# GetFileName(Eval("AIFilePath")) %>'>
                                    <%# GetFileName(Eval("AIFilePath")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Similarity Report">
                            <ItemTemplate>
                                <span class="truncate" title='<%# GetFileName(Eval("SimilarityFilePath")) %>'>
                                    <%# GetFileName(Eval("SimilarityFilePath")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="MMSC Report">
                            <ItemTemplate>
                                <span class="truncate" title='<%# GetFileName(Eval("MMSCReportFilePath")) %>'>
                                    <%# GetFileName(Eval("MMSCReportFilePath")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="MMSC Cert">
                            <ItemTemplate>
                                <span class="truncate" title='<%# GetFileName(Eval("MMSCCertFilePath")) %>'>
                                    <%# GetFileName(Eval("MMSCCertFilePath")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:Button runat="server" Text="Download All" CommandName="DownloadSimilarity"
                                    CommandArgument='<%# Eval("ReturnID") %>' CssClass="btn btn-outline-primary btn-sm" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

</asp:Content>
