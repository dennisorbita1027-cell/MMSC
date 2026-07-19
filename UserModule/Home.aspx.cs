using System;
using System.Web.UI;
using System.Web.UI.WebControls;

#nullable disable
namespace CASApp1.UserModule;

public class Home : Page
{
  protected Label lblWelcome;

  protected void Page_Load(object sender, EventArgs e)
  {
    if (this.Session["UserID"] != null && this.Session["StudentType"] != null)
      return;
    this.Response.Redirect("~/LogInPage/LoginPage.aspx");
  }
}
