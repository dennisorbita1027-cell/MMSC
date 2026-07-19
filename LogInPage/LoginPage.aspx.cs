// Decompiled with JetBrains decompiler
// Type: CASApp1.LogInPage.LoginPage
// Assembly: CASApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D01C93EE-CF3A-4F46-ABD3-3907A01835BC
// Assembly location: C:\Users\orbit\OneDrive\Documents\Silver\Project\Project\mmsc1\MmscPublished\bin\CASApp1.dll

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#nullable disable
namespace CASApp1.LogInPage;

public class LoginPage : Page
{
  protected HtmlForm form1;
  protected MultiView mvLogin;
  protected View vwSelectRole;
  protected Button btnAdmin;
  protected Button btnStudent;
  protected View vwLoginForm;
  protected Label lblRole;
  protected Label lblLoginError;
  protected TextBox txtLoginID;
  protected TextBox txtPassword;
  protected Panel pnlStudentOptions;
  protected CheckBox StaticChlk;
  protected CheckBox SimChk;
  protected Button btnLogin;
  protected Button btnBack;
  protected Button btnOpenRegister;
  protected Panel pnlRegister;
  protected Label lblRegister;
  protected TextBox txtFullName;
  protected TextBox txtStudentID;
  protected TextBox txtEmail;
  protected TextBox txtDegreeProgram;
  protected TextBox txtAffiliation;
  protected TextBox txtSpecialization;
  protected TextBox txtRegPassword;
  protected TextBox txtConfirmPassword;
  protected TextBox txtRegistrationCode;
  protected Label lblRegisterError;
  protected Button btnSubmitRegister;
  protected HiddenField hfRegisterRole;
  protected System.Web.UI.ScriptManager ScriptManager1;

  protected void Page_Load(object sender, EventArgs e)
  {
    if (!this.Request.IsSecureConnection)
      this.Response.Redirect(this.Request.Url.ToString().Replace("http://", "https://"));
    if (!this.IsPostBack)
    {
      this.mvLogin.ActiveViewIndex = 0;
      this.txtAffiliation.Visible = false;
      this.txtSpecialization.Visible = false;
    }
    if (this.Request.QueryString["registered"] == "1")
    {
      this.mvLogin.ActiveViewIndex = 1;
      this.lblLoginError.Text = "Registration successful.";
    }
    if (!this.pnlRegister.Visible)
      return;
    this.lblRegister.Text = this.hfRegisterRole.Value == "Admin" ? "Processor Register" : "Student Register";
  }

  protected void btnAdmin_Click(object sender, EventArgs e)
  {
    this.lblRole.Text = "Processor Login";
    this.mvLogin.ActiveViewIndex = 1;
    this.pnlStudentOptions.Visible = false;
    this.hfRegisterRole.Value = "Admin";
    this.lblRegister.Text = "Processor Register";
    this.txtAffiliation.Visible = true;
    this.txtSpecialization.Visible = true;
    this.txtStudentID.Visible = false;
    this.txtDegreeProgram.Visible = false;
  }

  protected void btnStudent_Click(object sender, EventArgs e)
  {
    this.lblRole.Text = "Student Login";
    this.mvLogin.ActiveViewIndex = 1;
    this.pnlStudentOptions.Visible = true;
    this.hfRegisterRole.Value = "Student";
    this.lblRegister.Text = "Student Register";
    this.txtAffiliation.Visible = false;
    this.txtSpecialization.Visible = false;
    this.txtStudentID.Visible = true;
    this.txtDegreeProgram.Visible = true;
  }

  protected void btnBack_Click(object sender, EventArgs e)
  {
    this.mvLogin.ActiveViewIndex = 0;
    this.lblLoginError.Text = "";
    this.txtLoginID.Text = "";
    this.txtPassword.Text = "";
    this.pnlStudentOptions.Visible = false;
  }

  protected void btnLogin_Click(object sender, EventArgs e)
  {
    this.lblLoginError.Text = "";
    string str1 = this.lblRole.Text == "Processor Login" ? "Admin" : "Student";
    string str2 = this.txtLoginID.Text.Trim();
    string password = this.txtPassword.Text.Trim();
    if (string.IsNullOrEmpty(str2) || string.IsNullOrEmpty(password))
    {
      this.lblLoginError.Text = "Please enter both login ID and password.";
    }
    else
    {
      string str3 = this.HashPassword(password);
      using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString))
      {
        connection.Open();
        SqlCommand sqlCommand1 = new SqlCommand("SELECT UserID, Role, LoginID, Email FROM Users WHERE LoginID = @LoginID AND Password = @Password AND Role = @Role", connection);
        sqlCommand1.Parameters.AddWithValue("@LoginID", (object) str2);
        sqlCommand1.Parameters.AddWithValue("@Password", (object) str3);
        sqlCommand1.Parameters.AddWithValue("@Role", (object) str1);
        SqlDataReader sqlDataReader1 = sqlCommand1.ExecuteReader();
        if (sqlDataReader1.Read())
        {
          string str4 = sqlDataReader1["UserID"].ToString();
          if (str4 == "1")
          {
            string str5 = sqlDataReader1["Role"].ToString();
            string str6 = sqlDataReader1["LoginID"].ToString();
            this.Session["UserID"] = (object) str4;
            this.Session["Username"] = (object) str6;
            this.Session["LoginID"] = (object) str6;
            this.Session["Role"] = (object) str5;
            this.Session["IsHeadAdmin"] = (object) true;
            this.Response.Redirect("~/ProcessorModule/ProcessorHome.aspx");
            return;
          }
        }
        sqlDataReader1.Close();
        string str7 = str2.Contains("@") ? str2 : str2 + "@gmail.com";
        SqlCommand sqlCommand2 = new SqlCommand("SELECT UserID, Role, LoginID, Email FROM Users WHERE Email = @Email AND Password = @Password AND Role = @Role", connection);
        sqlCommand2.Parameters.AddWithValue("@Email", (object) str7);
        sqlCommand2.Parameters.AddWithValue("@Password", (object) str3);
        sqlCommand2.Parameters.AddWithValue("@Role", (object) str1);
        SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
        if (sqlDataReader2.Read())
        {
          string str8 = sqlDataReader2["UserID"].ToString();
          string str9 = sqlDataReader2["Role"].ToString();
          string str10 = sqlDataReader2["LoginID"].ToString();
          this.Session["UserID"] = (object) str8;
          this.Session["Username"] = (object) str10;
          this.Session["LoginID"] = (object) str10;
          this.Session["Role"] = (object) str9;
          if (Convert.ToInt32(this.Session["UserID"]) == 1)
            this.Session["IsHeadAdmin"] = (object) true;
          switch (str1)
          {
            case "Admin":
              this.Response.Redirect("~/ProcessorModule/ProcessorHome.aspx");
              break;
            case "Student":
              bool flag1 = this.StaticChlk.Checked;
              bool flag2 = this.SimChk.Checked;
              if (!flag1 && !flag2 || flag1 & flag2)
              {
                this.lblLoginError.Text = "Please select only one option.";
                break;
              }
              this.Session["StaticChlk"] = (object) flag1;
              this.Session["SimChk"] = (object) flag2;
              this.Session["StudentType"] = flag1 ? (object) "StatVal" : (object) "Similarity";
              this.Response.Redirect("~/UserModule/Home.aspx");
              break;
          }
        }
        else
          this.lblLoginError.Text = "Invalid login credentials.";
      }
    }
  }

  protected void btnSubmitRegister_Click(object sender, EventArgs e) => this.SubmitRegistration();

  private void SubmitRegistration()
  {
    string str1 = this.txtFullName.Text.Trim();
    string str2 = this.txtStudentID.Text.Trim();
    string input = this.txtEmail.Text.Trim();
    string str3 = this.txtDegreeProgram.Text.Trim();
    string str4 = input.Contains("@") ? input.Substring(0, input.IndexOf("@")) : input;
    string str5 = this.hfRegisterRole.Value == "Admin" ? "Admin" : "Student";
    string password = this.CleanInput(this.txtRegPassword.Text);
    string str6 = this.CleanInput(this.txtConfirmPassword.Text);
    string str7 = this.txtRegistrationCode.Text.Trim();
    if (string.IsNullOrWhiteSpace(str7))
    {
      this.lblRegisterError.ForeColor = Color.Red;
      this.lblRegisterError.Text = "Registration code is required.";
      System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
    }
    else
    {
      string str8 = this.txtAffiliation?.Text.Trim();
      string str9 = this.txtSpecialization?.Text.Trim();
      System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
      if (str4.ToLower() == "admin")
      {
        this.lblRegisterError.ForeColor = Color.Red;
        this.lblRegisterError.Text = "You cannot register as 'admin'.";
        System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
      }
      else if (password != str6)
      {
        this.lblRegisterError.ForeColor = Color.Red;
        this.lblRegisterError.Text = "Passwords do not match.";
        System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
      }
      else if (string.IsNullOrWhiteSpace(str1) || string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(str4) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(str6))
      {
        this.lblRegisterError.ForeColor = Color.Red;
        this.lblRegisterError.Text = "All fields are required.";
        System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
      }
      else if (str5 == "Student" && string.IsNullOrWhiteSpace(str2))
      {
        this.lblRegisterError.ForeColor = Color.Red;
        this.lblRegisterError.Text = "Student ID is required for students.";
        System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
      }
      else
      {
        if (str5 == "Student")
        {
          if (string.IsNullOrWhiteSpace(str3))
          {
            this.lblRegisterError.ForeColor = Color.Red;
            this.lblRegisterError.Text = "Degree Program is required for students.";
            System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
            return;
          }
        }
        else
        {
          if (string.IsNullOrWhiteSpace(str8) || string.IsNullOrWhiteSpace(str9))
          {
            this.lblRegisterError.ForeColor = Color.Red;
            this.lblRegisterError.Text = "Affiliation and Specialization are required for processors.";
            System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
            return;
          }
          str3 = (string) null;
        }
        if (!Regex.IsMatch(input, "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$"))
        {
          this.lblRegisterError.Text = "Invalid email format.";
          this.lblRegisterError.ForeColor = Color.Red;
          System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
        }
        else
        {
          string str10 = this.HashPassword(password);
          using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString))
          {
            SqlCommand sqlCommand1 = new SqlCommand("SELECT COUNT(*) FROM Users WHERE LoginID = @Username", connection);
            sqlCommand1.Parameters.AddWithValue("@Username", (object) str4);
            connection.Open();
            int num1 = (int) sqlCommand1.ExecuteScalar();
            connection.Close();
            bool flag = false;
            SqlCommand sqlCommand2 = new SqlCommand("SELECT COUNT(*) FROM RegistrationCodes WHERE Code = @Code AND Role = @Role", connection);
            sqlCommand2.Parameters.AddWithValue("@Code", (object) str7);
            sqlCommand2.Parameters.AddWithValue("@Role", (object) str5);
            connection.Open();
            int num2 = (int) sqlCommand2.ExecuteScalar();
            connection.Close();
            if (num2 > 0)
              flag = true;
            if (!flag)
            {
              this.lblRegisterError.ForeColor = Color.Red;
              this.lblRegisterError.Text = "Invalid registration code for this role.";
              return;
            }
            if (num1 > 0)
            {
              this.lblRegisterError.ForeColor = Color.Red;
              this.lblRegisterError.Text = "Email already exists.";
              return;
            }
            SqlCommand sqlCommand3 = new SqlCommand("\r\nINSERT INTO Users (FullName, StudentID, Email, College, LoginID, Password, Role, Affiliation, Specialization, DateCreated)\r\nVALUES (@FullName, @StudentID, @Email, @College, @LoginID, @Password, @Role, @Affiliation, @Specialization, @DateCreated)", connection);
            sqlCommand3.Parameters.AddWithValue("@FullName", (object) str1);
            sqlCommand3.Parameters.AddWithValue("@StudentID", (object) str2);
            sqlCommand3.Parameters.AddWithValue("@Email", (object) input);
            sqlCommand3.Parameters.AddWithValue("@College", string.IsNullOrEmpty(str3) ? (object) DBNull.Value : (object) str3);
            sqlCommand3.Parameters.AddWithValue("@LoginID", (object) str4);
            sqlCommand3.Parameters.AddWithValue("@Password", (object) str10);
            sqlCommand3.Parameters.AddWithValue("@Role", (object) str5);
            sqlCommand3.Parameters.AddWithValue("@Affiliation", (object) str8 ?? (object) DBNull.Value);
            sqlCommand3.Parameters.AddWithValue("@Specialization", (object) str9 ?? (object) DBNull.Value);
            sqlCommand3.Parameters.AddWithValue("@DateCreated", (object) DateTime.Now);
            connection.Open();
            sqlCommand3.ExecuteNonQuery();
            connection.Close();
          }
          this.lblRegisterError.ForeColor = Color.Green;
          this.lblRegisterError.Text = "Registration successful!";
          System.Web.UI.ScriptManager.RegisterStartupScript((Page) this, this.GetType(), "ShowModal", "openRegisterModal();", true);
          this.txtFullName.Text = "";
          this.txtStudentID.Text = "";
          this.txtEmail.Text = "";
          this.txtDegreeProgram.Text = "";
          this.txtRegPassword.Text = "";
          this.txtConfirmPassword.Text = "";
          this.txtAffiliation.Text = "";
          this.txtSpecialization.Text = "";
          this.mvLogin.ActiveViewIndex = 1;
        }
      }
    }
  }

  private string HashPassword(string password)
  {
    using (SHA256 shA256 = SHA256.Create())
      return BitConverter.ToString(shA256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower();
  }

  private string CleanInput(string input) => Regex.Replace(input ?? "", "\\p{C}+", "").Trim();
}
