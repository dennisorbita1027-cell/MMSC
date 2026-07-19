using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

#nullable disable
namespace CASApp1.UserModule;

public class Statistician_list : Page
{
  private string connectionString = ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString;
  protected GridView gvStatisticians;

  protected void Page_Load(object sender, EventArgs e)
  {
    if (this.Session["UserID"] == null)
    {
      this.Response.Redirect("~/LogInPage/LoginPage.aspx");
    }
    else
    {
      if (this.IsPostBack)
        return;
      this.BindStatisticians();
    }
  }

  private void BindStatisticians()
  {
    DataTable dataTable = new DataTable();
    using (SqlConnection connection = new SqlConnection(this.connectionString))
    {
      using (SqlCommand selectCommand = new SqlCommand("SELECT FullName, Email, Affiliation, Specialization\r\nFROM Users\r\nWHERE Role = 'Admin' AND UserID <> 1\r\n", connection))
      {
        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand))
          sqlDataAdapter.Fill(dataTable);
      }
    }
    this.gvStatisticians.DataSource = (object) dataTable;
    this.gvStatisticians.DataBind();
  }
}
