using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#nullable disable
namespace CASApp1.ProcessorModule;

public class ProcessorMaster : MasterPage
{
  protected ContentPlaceHolder TitleContent;
  protected ScriptManager ScriptManager1;
  protected HtmlAnchor lnkAccounts;
  protected LinkButton lnkLogout;
  protected ContentPlaceHolder MainContent;
  protected ContentPlaceHolder ContentPlaceHolder1;
  protected ContentPlaceHolder HeadContent;

  protected void Page_Load(object sender, EventArgs e)
  {
    if (this.Session["UserID"] == null)
    {
      this.Response.Redirect("~/LogInPage/LoginPage.aspx", true);
    }
    else
    {
      if (this.IsPostBack || Convert.ToInt32(this.Session["UserID"]) == 1)
        return;
      this.lnkAccounts.Visible = false;
    }
  }

  protected void btnLogout_Click(object sender, EventArgs e)
  {
    this.Session.Abandon();
    this.Response.Redirect("~/LogInPage/LoginPage.aspx");
  }
}
