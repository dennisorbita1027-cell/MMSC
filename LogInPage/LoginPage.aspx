<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginPage.aspx.cs" Inherits="CASApp1.LogInPage.LoginPage" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>MMSC - Access Gateway</title>
    
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>

    <style>
        :root {
            --primary-green: #228B22;
            --dark-green: #006400; 
            --light-bg: #f8f9fa;
        }

        body {
            background-color: var(--light-bg);
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .auth-container {
            width: 100%;
            max-width: 500px;
            margin: 80px auto;
            padding: 20px;
            text-align: center;
        }

        .logo {
            width: 140px;
            height: auto;
            margin-bottom: 25px;
            object-fit: contain;
        }

        .card-brand-header {
            background-color: var(--primary-green);
            border: 1px solid var(--dark-green);
            color: #ffffff;
            border-radius: 8px;
            padding: 15px;
            margin-bottom: 25px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.05);
        }

        .card-brand-header h2 {
            font-size: 1.25rem;
            margin: 0;
            font-weight: 600;
            letter-spacing: 0.5px;
        }

        .custom-form-control {
            font-size: 1rem;
            padding: 12px;
            margin-bottom: 15px;
            border-radius: 6px;
            border: 1px solid #ced4da;
            width: 100%;
            box-sizing: border-box;
            transition: border-color 0.15s ease-in-out;
        }

        .custom-form-control:focus {
            border-color: var(--primary-green);
            outline: 0;
            box-shadow: 0 0 0 0.25rem rgba(34, 139, 34, 0.25);
        }

        .btn-brand {
            background-color: var(--primary-green);
            color: #ffffff;
            padding: 12px;
            font-size: 1rem;
            font-weight: 500;
            border: 1px solid var(--dark-green);
            border-radius: 6px;
            width: 100%;
            margin-bottom: 12px;
            transition: all 0.2s ease;
        }

        .btn-brand:hover {
            background-color: var(--dark-green);
            color: #ffffff;
            transform: translateY(-1px);
        }

        .btn-brand-secondary {
            background-color: #ffffff;
            color: #333333;
            border: 1px solid #ccc;
        }

        .btn-brand-secondary:hover {
            background-color: #f1f1f1;
            color: #111111;
        }

        .student-options-panel {
            background-color: #ffffff;
            border: 1px solid #e3e3e3;
            border-radius: 6px;
            padding: 15px;
            margin-bottom: 20px;
            text-align: left;
        }

        .registration-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }

        @media (max-width: 768px) {
            .auth-container {
                margin: 40px auto;
                padding: 15px;
            }
            .registration-grid {
                grid-template-columns: 1fr;
                gap: 0;
            }
        }
    </style>

    <script type="text/javascript">
        function openRegisterModal() {
            var myModal = new bootstrap.Modal(document.getElementById('registerModal'));
            myModal.show();
        }

        function toggleExclusiveCheckbox(clickedId, otherId) {
            var clicked = document.getElementById(clickedId);
            var other = document.getElementById(otherId);

            if (clicked.checked) {
                other.checked = false;
                other.disabled = true;
            } else {
                other.disabled = false;
            }
        }

        function isValidEmail(email) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
        }

        function validateRegisterForm() {
            var emailInput = document.getElementById("<%= txtEmail.ClientID %>");
            if (emailInput && !isValidEmail(emailInput.value.trim())) {
                alert("Please enter a valid structural email address.");
                emailInput.focus();
                return false;
            }
            return true;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="auth-container">
            <img src="/Images/logo.png" alt="MMSC Logo" class="logo" />
            
            <asp:MultiView ID="mvLogin" runat="server" ActiveViewIndex="0">
                <!-- VIEW 1: ROLE SELECTION -->
                <asp:View ID="vwSelectRole" runat="server">
                    <div class="card-brand-header">
                        <h2>Mathematical Modeling Statistics Center</h2>
                    </div>
                    <asp:Button ID="btnAdmin" runat="server" Text="Processor Portal" CssClass="btn-brand" OnClick="btnAdmin_Click" />
                    <asp:Button ID="btnStudent" runat="server" Text="Student Portal" CssClass="btn-brand btn-brand-secondary" OnClick="btnStudent_Click" />
                </asp:View>

                <!-- VIEW 2: AUTHENTICATION CHALLENGE FORM -->
                <asp:View ID="vwLoginForm" runat="server">
                    <div class="card-brand-header">
                        <h2><asp:Label ID="lblRole" runat="server" /> Gateway</h2>
                    </div>

                    <asp:Label ID="lblLoginError" runat="server" ForeColor="Red" Font-Size="Small" CssClass="d-block mb-2" />
                    
                    <asp:TextBox ID="txtLoginID" runat="server" CssClass="custom-form-control" Placeholder="Account Email" />
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="custom-form-control" TextMode="Password" Placeholder="Password" />

                    <asp:Panel ID="pnlStudentOptions" runat="server" Visible="false" CssClass="student-options-panel">
                        <span class="text-muted d-block mb-2 small fw-bold">Select Required Routine:</span>
                        <div class="form-check mb-2">
                            <asp:CheckBox ID="StaticChlk" runat="server" Text="Statistical Validation Engine"
                                ClientIDMode="Static" CssClass="form-check-input"
                                onclick="toggleExclusiveCheckbox(this.id, 'SimChk');" />
                        </div>
                        <div class="form-check">
                            <asp:CheckBox ID="SimChk" runat="server" Text="Similarity & AI Check Suite"
                                ClientIDMode="Static" CssClass="form-check-input"
                                onclick="toggleExclusiveCheckbox(this.id, 'StaticChlk');" />
                        </div>
                    </asp:Panel>

                    <asp:Button ID="btnLogin" runat="server" Text="Authorize Session" CssClass="btn-brand" OnClick="btnLogin_Click" />
                    
                    <div class="row g-2 mt-2">
                        <div class="col-6">
                            <asp:Button ID="btnBack" runat="server" Text="Change Role" CssClass="btn-brand btn-brand-secondary py-2 small" OnClick="btnBack_Click" />
                        </div>
                        <div class="col-6">
                            <asp:Button ID="btnOpenRegister" runat="server" Text="Create Account" CssClass="btn-brand btn-brand-secondary py-2 small" OnClientClick="openRegisterModal(); return false;" />
                        </div>
                    </div>
                </asp:View>
            </asp:MultiView>
        </div>

        <!-- NATIVE BOOTSTRAP 5 REGISTRATION MODAL -->
        <div class="modal fade" id="registerModal" tabindex="-1" aria-labelledby="registerModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content text-start">
                    <div class="modal-header" style="background-color: var(--primary-green); color: white;">
                        <h5 class="modal-title" id="registerModalLabel">
                            <asp:Label ID="lblRegister" runat="server" Text="System Access Registration" ForeColor="White" Font-Size="20px" />
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="registration-grid">
                            <!-- Left Column -->
                            <div>
                                <label class="small fw-bold text-muted mb-1">Identity Specifications</label>
                                <asp:TextBox ID="txtFullName" runat="server" CssClass="custom-form-control" placeholder="Full Name (Last, First, M.I.)" />
                                <asp:TextBox ID="txtStudentID" runat="server" CssClass="custom-form-control" placeholder="Institutional Reference ID" />
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="custom-form-control" placeholder="Active Email Address" />
                                <asp:TextBox ID="txtDegreeProgram" runat="server" CssClass="custom-form-control" placeholder="College / Course Track" />
                                <asp:TextBox ID="txtAffiliation" runat="server" CssClass="custom-form-control" Placeholder="Organization Affiliation" />
                                <asp:TextBox ID="txtSpecialization" runat="server" CssClass="custom-form-control" Placeholder="Research Area / Specialization" />
                            </div>

                            <!-- Right Column -->
                            <div>
                                <label class="small fw-bold text-muted mb-1">Security Setup</label>
                                <asp:TextBox ID="txtRegPassword" runat="server" CssClass="custom-form-control" TextMode="Password" placeholder="Password Assignment" autocomplete="off" />
                                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="custom-form-control" TextMode="Password" placeholder="Confirm Password Assignment" />
                                
                                <label class="small fw-bold text-muted mb-1 mt-2">Verification Key</label>
                                <asp:TextBox ID="txtRegistrationCode" runat="server" CssClass="custom-form-control" placeholder="Access Authentication Code" />
                                <div class="alert alert-info py-2 px-3 style-alert structural-note" style="font-size: 12px; line-height: 1.4;">
                                    Registration tokens are issued directly by the MMSC Office admin desk. For clearance or system provisioning validation requests, contact: <strong>nvsu.mmsc@nvsu.edu.ph</strong>.
                                </div>
                            </div>
                        </div>

                        <asp:Label ID="lblRegisterError" runat="server" ForeColor="Red" Font-Bold="true" class="d-block text-center my-2" />
                    </div>
                    <div class="modal-footer bg-light">
                        <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Dismiss</button>
                        <asp:Button ID="btnSubmitRegister" runat="server" CssClass="btn btn-success px-4" style="background-color: var(--primary-green);" Text="Process Sign Up"
                            OnClick="btnSubmitRegister_Click" OnClientClick="return validateRegisterForm();" />
                    </div>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hfRegisterRole" runat="server" />
    </form>
</body>
</html>