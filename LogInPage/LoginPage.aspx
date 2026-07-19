<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginPage.aspx.cs" Inherits="CASApp1.LogInPage.LoginPage" %>

    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">

        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <script>

            function openRegisterModal() {
                document.getElementById("pnlRegister").style.display = "block";

            }

            function closeRegisterModal() {
                // Hide the modal
                document.getElementById("pnlRegister").style.display = "none";

                // Clear all text inputs inside the modal
                var inputs = document.querySelectorAll("#pnlRegister input[type='text'], #pnlRegister input[type='password']");
                inputs.forEach(function (input) {
                    input.value = "";
                });
            }


            function submitRegister() {
                // Placeholder - add validation or server-side call
                alert("Registration submitted!");
                closeRegisterModal();
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
                var email = document.getElementById("<%= txtEmail.ClientID %>").value;
                if (!isValidEmail(email)) {
                    alert("Please enter a valid email address.");
                    return false;
                }
                return true;
            }

        </script>

        <title>Login Selection</title>
        <style>

            @media screen and (max-width: 600px) {
    .container { padding-top: 50px; }
    .logo { width: 120px; }
    .button, .textbox { width: 90%; }
    .box { font-size: 14px; }
}

                .container {
    width: 100%;
    max-width: 600px;
    margin: auto;
    padding-top: 150px;
    text-align: center;
    font-family: Arial, sans-serif;
}


                .box {
                    width: 100%;
                    border: 1px solid darkgreen;
                    font-size: 14px;
                    border-radius: 5px;
                    padding: 10px;
                    margin: 5px 0;
                    background-color: forestgreen;
                    text-align: center;
                    color: white;
                }

                h2 {
                    font-size: 20px;
                }

                #vwLoginForm .textbox {
    width: 50%;
    max-width: 250px;
    font-size: 16px;
    padding: 8px;
}
            


    .container {
        width: 600px;
        margin: auto;
        padding-top: 150px;
        text-align: center;
        font-family: Arial, sans-serif;
    }

    .box {
        width: 100%;
        border: 1px solid darkgreen;
        border-radius: 5px;
        padding: 10px;
        margin: 5px 0;
        background-color: forestgreen;
        text-align: center;
        color: white;
    }
    

  

    #vwLoginForm .button {
    width: 50%;
    max-width: 250px;
    font-size: 16px;
}
    .button, .textbox {
    width: 70%;
    max-width: 400px;
}

    .textbox {
        width: 70%;
        padding: 10px;
        font-size: 20px;
        margin: 10px 0;
    }

    /* Responsive styles */
   @media screen and (max-width: 600px) {
    #vwLoginForm .textbox,
    #vwLoginForm .button {
        width: 90%;
        max-width: none;
    }
}

        .container {
            width: 90%;
            padding-top: 50px;
        }

        .box {
            font-size: 14px;
        }

        h2 {
            font-size: 20px;
        }

        .button {
            font-size: 16px;
            padding: 10px 20px;
            width: 90%;
        }

        .textbox {
            width: 90%;
            font-size: 16px;
            padding: 8px;
        }
    

        </style>
    </head>

    <body>
        <form id="form1" runat="server">
            <div class="container">
                <img src="/Images/logo.png" alt="MMSC Logo" class="logo" />
                <asp:MultiView ID="mvLogin" runat="server" ActiveViewIndex="0">
                    
                    <asp:View ID="vwSelectRole" runat="server">
                    
                        <div class="box">
                            
                                <h2>Mathematical Modeling Statistics Center</h2>
                            </div>

                        <asp:Button ID="btnAdmin" runat="server" Text="Processor" CssClass="button" OnClick="btnAdmin_Click" />
                        <asp:Button ID="btnStudent" runat="server" Text="Student" CssClass="button" OnClick="btnStudent_Click" />
                    </asp:View>
                    
                    <asp:View ID="vwLoginForm" runat="server">
                        <div class="box">
                            <h2><asp:Label ID="lblRole" runat="server" /></h2>
                            <div style="margin-bottom: 10px;"></div>
                        </div>
                       
                        <asp:Label ID="lblLoginError" runat="server" ForeColor="Red" Font-Size="Small" />
                        <br />
                        <asp:TextBox ID="txtLoginID" runat="server" CssClass="textbox" Placeholder="Email" /><br />
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="textbox" TextMode="Password" Placeholder="Password" /><br />
                        
                        <asp:Panel ID="pnlStudentOptions" runat="server" Visible="false">
                           <asp:CheckBox ID="StaticChlk" runat="server" Text="Statistical Validation"
                                ClientIDMode="Static"
                                onclick="toggleExclusiveCheckbox(this.id, 'SimChk');" />

                            <asp:CheckBox ID="SimChk" runat="server" Text="Similarity & AI Check"
                                ClientIDMode="Static"
                                onclick="toggleExclusiveCheckbox(this.id, 'StaticChlk');" />
                        </asp:Panel>
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="button" OnClick="btnLogin_Click" /><br />
                        <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="button" OnClick="btnBack_Click" />
                        <asp:Button ID="btnOpenRegister" runat="server" Text="Create Account" CssClass="button" OnClientClick="openRegisterModal(); return false;" />
                    </asp:View>
                </asp:MultiView>

            </div>
       
        <!-- Registration Modal -->
          <asp:Panel ID="pnlRegister" runat="server" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; 
                background-color:rgba(0,0,0,0.5); z-index:1000; text-align:center;">
                <div runat="server" style="background:white; padding:30px; border-radius:10px; width:90%; max-width:600px; 
                    margin:5% auto; position:relative;">

                     <asp:Label ID="lblRegister" runat="server" Font-Size="30px" Font-Bold="True" ForeColor="Black" /><br />
                    
                         <div style="display: flex; flex-wrap: wrap; gap: 20px; justify-content: space-between;">
                            <div style="flex: 1 1 45%;">
                                <asp:TextBox ID="txtFullName" runat="server" CssClass="textbox" placeholder="Full Name" /><br />
                                <asp:TextBox ID="txtStudentID" runat="server" CssClass="textbox" placeholder="ID" /><br />
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox" placeholder="Email" /><br />
                                <asp:TextBox ID="txtDegreeProgram" runat="server" CssClass="textbox" placeholder="College/Course" /><br />
                                <asp:TextBox ID="txtAffiliation" runat="server" CssClass="textbox" Placeholder="Affiliation" /><br />
                                <asp:TextBox ID="txtSpecialization" runat="server" CssClass="textbox" Placeholder="Specialization" /><br />
                            </div>

                            <div style="flex: 1 1 45%;">
                                
                                <asp:TextBox ID="txtRegPassword" runat="server" CssClass="textbox" TextMode="Password" placeholder="Password" autocomplete="off" />
                                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="textbox" TextMode="Password" placeholder="Confirm Password" /><br />
                                <asp:TextBox ID="txtRegistrationCode" runat="server" CssClass="textbox" placeholder="Registration Code" /><br />
                                <p style="color:gray; font-size:12px; font-style:italic;">
  Registration codes are available at the MMSC office, or contact this email for assistance: nvsu.mmsc@nvsu.edu.ph.
</p>


                            </div>
                        </div>
                    <asp:Label ID="lblRegisterError" runat="server" ForeColor="Red" Font-Bold="true" /><br />

                    <asp:Button ID="btnSubmitRegister" runat="server" CssClass="button" Text="Submit"
    OnClick="btnSubmitRegister_Click" OnClientClick="return validateRegisterForm();" />

                    <button type="button" onclick="closeRegisterModal()" class="button" style="background:red;">Cancel</button>
                    
                </div>
            </asp:Panel>
            <asp:HiddenField ID="hfRegisterRole" runat="server" />
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
           </form> 
      </body>
</html>