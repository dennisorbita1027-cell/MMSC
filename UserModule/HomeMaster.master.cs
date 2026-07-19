using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#nullable disable
namespace CASApp1.UserModule;

public class HomeMaster : MasterPage
{
  protected HtmlForm form1;
  protected ScriptManager ScriptManager1;
  protected Button btnHome;
  protected Button btnStaComval;
  protected Button btnSimAiWriChe;
  protected Button btnCertification;
  protected Button btnStatistician_List;
  protected Button btnLogout;
  protected ContentPlaceHolder HeadContent;
  protected ContentPlaceHolder MainContent;

  protected void Page_Load(object sender, EventArgs e)
  {
    if (this.Session["UserID"] == null || this.Session["StudentType"] == null)
    {
      this.Response.Redirect("~/LogInPage/LoginPage.aspx");
    }
    else
    {
      if (this.IsPostBack)
        return;
      string str = this.Session["Role"] as string;
      bool flag1 = this.Session["StaticChlk"] != null && (bool) this.Session["StaticChlk"];
      bool flag2 = this.Session["SimChk"] != null && (bool) this.Session["SimChk"];
      this.btnSimAiWriChe.Visible = false;
      this.btnStaComval.Visible = false;
      if (str == "Student")
      {
        if (flag1 && !flag2)
          this.btnStaComval.Visible = true;
        else if (!flag1 & flag2)
          this.btnSimAiWriChe.Visible = true;
        else if (flag1 & flag2)
        {
          this.btnStaComval.Visible = true;
          this.btnSimAiWriChe.Visible = true;
        }
        this.btnHome.Visible = true;
        this.btnCertification.Visible = true;
        this.btnStatistician_List.Visible = true;
      }
      else
      {
        this.btnHome.Visible = false;
        this.btnSimAiWriChe.Visible = false;
        this.btnStatistician_List.Visible = false;
      }
    }
  }

  protected void btnHome_Click(object sender, EventArgs e)
  {
    this.Response.Redirect("~/UserModule/Home.aspx");
  }

  protected void btnStatComVal_Click(object sender, EventArgs e)
  {
    this.Response.Redirect("~/UserModule/StatComVal1.aspx");
  }

  protected void btnStatisticianList_Click(object sender, EventArgs e)
  {
    this.Response.Redirect("~/UserModule/Statistician_List.aspx");
  }

  protected void btnSimAiWriChe_Click(object sender, EventArgs e)
  {
    this.Response.Redirect("~/UserModule/SimAIWriChe3.aspx");
  }

  protected void btnCertification_Click(object sender, EventArgs e)
  {
    this.Response.Redirect("~/UserModule/CertRemarks.aspx");
  }

  protected void btnLogout_Click(object sender, EventArgs e)
  {
    this.Session.Clear();
    this.Session.Abandon();
    this.Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1.0);
    this.Response.Redirect("~/LogInPage/LoginPage.aspx");
  }
}
