<%@ Page Title="AccountManagement" Language="C#" MasterPageFile="~/ProcessorModule/ProcessorMaster.master" AutoEventWireup="true" CodeBehind="AccountManagement.aspx.cs" Inherits="CASApp1.ProcessorModule.AccountManagement" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>

    <style>
        .hash-text {
            font-family: monospace;
            font-size: 0.85rem;
            word-break: break-all;
        }

        .account-management-container {
            padding-top: 120px;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container mt-4 account-management-container">
        <div class="row g-4">



            <!-- Admin Username -->
            <div class="col-md-6 mt-4">
                <div class="card shadow">
                    <div class="card-header bg-primary text-white fw-bold">
                        Change Admin Username
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label">New Username</label>
                            <asp:TextBox ID="txtNewUsername" runat="server" placeholder="New Username"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Current Password</label>
                            <asp:TextBox ID="txtCurrentPasswordForUsername" runat="server" TextMode="Password" placeholder="Enter current password"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnUpdateUsername" runat="server" Text="Update Username"
                            OnClick="btnUpdateUsername_Click" CssClass="btn btn-primary" />
                        <asp:Label ID="lblUsernameStatus" runat="server" ForeColor="Red" Font-Bold="true" />
                    </div>
                </div>
            </div>

            <!-- Admin Password -->
            <div class="col-md-6">
                <div class="card shadow">
                    <div class="card-header bg-primary text-white fw-bold">
                        Change Admin Password
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label">New Password</label>
                            <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" placeholder="New Password"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Confirm Password</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" placeholder="Confirm Password"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnUpdatePassword" runat="server" Text="Update Password"
                            OnClick="btnUpdatePassword_Click" CssClass="btn btn-primary" />
                        <asp:Label ID="lblPassStatus" runat="server" ForeColor="Red" Font-Bold="true" />


                    </div>
                </div>
            </div>

            <!-- Delete Accounts -->
            <div class="col-md-6">
                <div class="card shadow">
                    <div class="card-header bg-danger text-white fw-bold">
                        Manage Student Accounts
                    </div>
                    <div class="card-body">
                        <p>Select semester(s) to delete all accounts from that period:</p>
                        <asp:PlaceHolder ID="phSemesters" runat="server"></asp:PlaceHolder>

                        <div class="mt-3">
                            <asp:Button ID="btnDeleteSelected" runat="server" CssClass="btn btn-warning"
                                Text="Delete Selected Accounts" OnClick="btnDeleteSelected_Click" />
                            <asp:Label ID="lblDeleteSelectedStatus" runat="server" 
                           ForeColor="Green" Font-Bold="true" />
                        </div>
                    </div>
                    <div class="card-footer text-end">
                        <asp:Button ID="btnDeleteAll" runat="server" CssClass="btn btn-danger"
                            Text="Delete ALL Accounts (Except Head Processor)" OnClick="btnDeleteAll_Click" />
                        <asp:Label ID="lblDeleteAllStatus" runat="server" 
                       ForeColor="Green" Font-Bold="true" />
                    </div>
                </div>
            </div>

        </div>
    </div>

    <!-- Modal Confirmation -->
    <div class="modal fade" id="passwordModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Confirm Admin Password</h5>
                    <button type="button" class="btn-close" data-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfActionType" runat="server" />
                    <label>Enter your admin password to proceed:</label>
                    <asp:TextBox ID="txtAdminPasswordConfirm" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    <asp:Label ID="lblModalError" runat="server" ForeColor="Red" Visible="false"></asp:Label>

                </div>
                <div class="modal-footer">

                    <button type="button" class="btn btn-secondary" data-dismiss="modal" aria-label="Close">Close</button>

                    <asp:Button ID="btnConfirmAction" runat="server" CssClass="btn btn-danger"
                        Text="Confirm" OnClick="btnConfirmAction_Click" />
                </div>
            </div>
        </div>
    </div>


</asp:Content>
