<%@ Page Title="UserList" Language="C#" MasterPageFile="~/ProcessorModule/ProcessorMaster.master" AutoEventWireup="true" CodeBehind="UserList.aspx.cs" Inherits="CASApp1.ProcessorModule.UserList" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">

    <script>
        function openDeleteModal(userId) {
            document.getElementById('<%= hfDeleteUserId.ClientID %>').value = userId;
            var modal = new bootstrap.Modal(document.getElementById('deleteModal'));
            modal.show();
        }


        function openResetModal(userId) {
            document.getElementById('<%= hfResetUserId.ClientID %>').value = userId;
            var modal = new bootstrap.Modal(document.getElementById('resetModal'));
            modal.show();
        }
    </script>


    <style>
        .container-users {
            background-color: #fff;
            padding: 30px;
            border-radius: 25px;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            max-width: 1000px;
            margin: 40px auto;
        }

        .title {
            font-size: 32px;
            font-weight: bold;
            text-align: center;
            color: #2c3e50;
        }

        .styled-gridview {
            width: 100%;
            border-collapse: collapse;
            font-size: 18px;
            margin-top: 10px;
        }

            .styled-gridview th, .styled-gridview td {
                padding: 12px;
                text-align: left;
                border-bottom: 1px solid #ddd;
            }

            .styled-gridview tr:hover {
                background-color: #f2f2f2;
            }

            .styled-gridview th {
                background-color: #f8f9fa;
                color: black;
            }

        .toggle-buttons {
            text-align: center;
            margin-top: 20px;
        }

        .search-bar {
            text-align: center;
            margin: 20px 0;
        }

        .codelabel {
            font-size: 18px;
            font-weight: bold;
        }
    </style>

</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-users">
        <!-- Top Header + Registration Code Panel -->
        <div class="d-flex justify-content-between align-items-start mb-4">
            <div class="title">👥 List of Users</div>

            <!-- Registration Code Panels -->
            <!-- Student Registration Code -->
            <asp:Panel ID="pnlStudentCode" runat="server" Visible="false" CssClass="text-end">
                <div class="d-flex align-items-center justify-content-end gap-2">
                    <asp:Label ID="lblRegistrationStu" runat="server" Text="Registration Code:" CssClass="fw-bold" />
                    <!-- Label for showing generated code -->
                    <asp:Label ID="lblStudentCode" runat="server" CssClass="codelabel" Visible="false"></asp:Label>
                </div>
                <div>
                    <asp:Button ID="btnGenerateStudentCode" runat="server"
                        Text="Generate Registration Code"
                        OnClick="btnGenerateStudentCode_Click" />
                </div>
            </asp:Panel>


            <!-- Processor Registration Code -->
            <asp:Panel ID="pnlProcessorCode" runat="server" Visible="false" CssClass="text-end">
                <div class="d-flex align-items-center justify-content-end gap-2">
                    <asp:Label ID="lblRegistrationProc" runat="server" Text="Registration Code:" CssClass="fw-bold" />
                    <!-- Label for showing generated code -->
                    <asp:Label ID="lblProcessorCode" runat="server" CssClass="codelabel" Visible="false"></asp:Label>
                </div>
                <div>
                    <asp:Button ID="btnGenerateProcessorCode" runat="server"
                        Text="Generate Registration Code"
                        OnClick="btnGenerateProcessorCode_Click" />
                </div>
            </asp:Panel>        
        </div>

        <div class="toggle-buttons">
            <asp:Button ID="btnShowStudents" runat="server" Text="Students" CssClass="btn btn-primary" OnClick="btnShowStudents_Click" />
            <asp:Button ID="btnShowProcessors" runat="server" Text="Processors" CssClass="btn btn-outline-secondary" OnClick="btnShowProcessors_Click" />
        </div>

        <div class="search-bar">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control d-inline-block" Width="300px" placeholder="Search by full name..." />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-info ms-2" OnClick="btnSearch_Click" />
            <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary ms-2" OnClick="btnReset_Click" />
        </div>

        <!-- Student Table -->
        <asp:Panel ID="pnlStudents" runat="server">
            <asp:GridView ID="gvStudents" runat="server" AutoGenerateColumns="false" CssClass="styled-gridview"
                AllowPaging="true" PageSize="20" OnPageIndexChanging="gvStudents_PageIndexChanging"
                OnRowCommand="gvStudents_RowCommand"
                OnRowDataBound="gvStudents_RowDataBound"
                DataKeyNames="UserID">
                <Columns>
                    <asp:BoundField DataField="StudentID" HeaderText="Student ID" />
                    <asp:BoundField DataField="FullName" HeaderText="Full Name" />
                    <asp:BoundField DataField="College" HeaderText="College" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnDeleteStudent" runat="server" Text="Delete"
    CssClass="btn btn-danger btn-sm"
    OnClientClick='<%# Eval("UserID", "openDeleteModal(\"{0}\"); return false;") %>' />
                            <span id="spnDeleteStatus" runat="server" class="ms-2 fw-bold"></span>


                            <asp:Button ID="btnResetStudent" runat="server" Text="Reset"
            CssClass="btn btn-warning btn-sm ms-1"
            OnClientClick='<%# Eval("UserID", "openResetModal(\"{0}\"); return false;") %>' />
                            <span id="spnResetStatus" runat="server" class="ms-2 fw-bold"></span>

                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <div class="modal fade" id="resetModal" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content rounded-3 shadow">
      <div class="modal-header bg-warning">
        <h5 class="modal-title">Reset User Data</h5>
        <button type="button" class="btn-close" data-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <p>Select a Module:</p>
        <div class="form-check">
          <input class="form-check-input" type="checkbox" id="chkStatComVal" runat="server" />
          <label class="form-check-label">Statistics Computational Validation</label>
        </div>
        <div class="form-check">
          <input class="form-check-input" type="checkbox" id="chkSimAiWri" runat="server" />
          <label class="form-check-label">Similarity & AI Writing Check</label>
        </div>
        <hr />
        <label for="txtConfirmPassword">Enter your password to confirm:</label>
        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" />
      </div>
      <div class="modal-footer">
        <asp:Button ID="btnConfirmReset" runat="server" CssClass="btn btn-warning" Text="Reset Now" OnClick="btnConfirmReset_Click" UseSubmitBehavior="false" />
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
      </div>
    </div>
  </div>
</div>

<asp:HiddenField ID="hfResetUserId" runat="server" />


        <!-- Processor Table -->
        <asp:Panel ID="pnlProcessors" runat="server">
            <asp:GridView ID="gvProcessors" runat="server" AutoGenerateColumns="false" CssClass="styled-gridview"
                OnRowCommand="gvProcessors_RowCommand"
                OnRowDataBound="gvProcessors_RowDataBound"
                DataKeyNames="UserID">
                <Columns>
                    <asp:BoundField DataField="FullName" HeaderText="Full Name" />
                    <asp:BoundField DataField="Affiliation" HeaderText="Affiliation" />
                    <asp:BoundField DataField="Specialization" HeaderText="Specialization" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnDeleteProcessor" runat="server" Text="Delete"
    CssClass="btn btn-danger btn-sm"
    OnClientClick='<%# Eval("UserID", "openDeleteModal(\"{0}\"); return false;") %>' />
                            <span id="spnDeleteStatus" runat="server" class="ms-2 fw-bold"></span>


                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>
        <asp:HiddenField ID="hfDeleteUserId" runat="server" />

<!-- Delete Confirmation Modal -->
<div class="modal fade" id="deleteModal" tabindex="-1" role="dialog">
  <div class="modal-dialog modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Confirm Delete</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body">
        <p>Enter your password to confirm deletion:</p>
        <asp:TextBox ID="txtDeletePassword" runat="server" TextMode="Password" CssClass="form-control" />
      </div>
      <div class="modal-footer">
        <asp:Button ID="btnConfirmDelete" runat="server" Text="Confirm Delete" CssClass="btn btn-danger" OnClick="btnConfirmDelete_Click" />
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
      </div>
    </div>
  </div>
</div>



    </div>
</asp:Content>
