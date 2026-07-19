// Decompiled with JetBrains decompiler
// Type: CASApp1.Download
// Assembly: CASApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D01C93EE-CF3A-4F46-ABD3-3907A01835BC
// Assembly location: C:\Users\orbit\OneDrive\Documents\Silver\Project\Project\mmsc1\MmscPublished\bin\CASApp1.dll

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.SessionState;

#nullable disable
namespace CASApp1;

public class Download : IHttpHandler, IRequiresSessionState
{
  public void ProcessRequest(HttpContext context)
  {
    try
    {
      string s1 = context.Request.QueryString["returnId"];
      string s2 = context.Request.QueryString["submissionId"];
      string field = context.Request.QueryString["field"];
      int result1 = 0;
      int result2 = 0;
      if (!string.IsNullOrEmpty(s1))
        int.TryParse(s1, out result1);
      if (!string.IsNullOrEmpty(s2))
        int.TryParse(s2, out result2);
      if (string.IsNullOrEmpty(field))
      {
        context.Response.StatusCode = 400;
        context.Response.Write("Missing field.");
      }
      else if (Array.IndexOf<string>(new string[14]
      {
        "RawDataPath",
        "ProposalCopyPath",
        "Instrumentation",
        "StatOutputPath",
        "DeclarationPath",
        "ReceiptPath",
        "CopyFilePath",
        "DocumentFilePath",
        "DeclarationAIPath",
        "StatComCertPath",
        "MMSCCertFilePath",
        "SimilarityFilePath",
        "AIFilePath",
        "MMSCReportFilePath"
      }, field) < 0)
      {
        context.Response.StatusCode = 403;
      }
      else
      {
        string path = (string) null;
        string str1 = context.Session["Role"] as string;
        string s3 = context.Session["UserID"] as string;
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MMSCConnection"].ConnectionString))
        {
          connection.Open();
          if (str1 == "Processor" || str1 == "Admin")
          {
            string sqlByField = this.GetSqlByField(field);
            if (string.IsNullOrEmpty(sqlByField))
            {
              context.Response.StatusCode = 400;
              return;
            }
            SqlCommand sqlCommand = new SqlCommand(sqlByField, connection);
            sqlCommand.Parameters.AddWithValue("@ID", (object) result2);
            object obj = sqlCommand.ExecuteScalar();
            if (obj == null || string.IsNullOrEmpty(obj.ToString()))
            {
              context.Response.StatusCode = 404;
              return;
            }
            path = obj.ToString().Trim();
          }
          if (str1 == "Student")
          {
            int result3;
            if (!int.TryParse(s3, out result3))
            {
              context.Response.StatusCode = 401;
              context.Response.Write("Session expired. Please log in again.");
              return;
            }
            if (field == "StatComCertPath" || field == "MMSCCertFilePath" || field == "MMSCReportFilePath" || field == "AIFilePath" || field == "SimilarityFilePath")
            {
              SqlCommand sqlCommand;
              if (result1 > 0)
              {
                sqlCommand = new SqlCommand($"SELECT {field} FROM ProcessorReturnedReports WHERE ReturnID=@RID AND UserID=@UID AND {field} IS NOT NULL", connection);
                sqlCommand.Parameters.AddWithValue("@RID", (object) result1);
              }
              else
              {
                sqlCommand = new SqlCommand($"SELECT TOP 1 {field} FROM ProcessorReturnedReports WHERE SubmissionID=@Sub AND UserID=@UID AND {field} IS NOT NULL ORDER BY DateSent DESC", connection);
                sqlCommand.Parameters.AddWithValue("@Sub", (object) result2);
              }
              sqlCommand.Parameters.AddWithValue("@UID", (object) result3);
              object obj = sqlCommand.ExecuteScalar();
              if (obj == null || string.IsNullOrEmpty(obj.ToString()))
              {
                context.Response.StatusCode = 404;
                context.Response.Write("File not available.");
                return;
              }
              path = obj.ToString().Trim();
            }
            else
            {
              context.Response.StatusCode = 403;
              context.Response.Write("You cannot download this type of file.");
              return;
            }
          }
          if (string.IsNullOrEmpty(path))
          {
            context.Response.StatusCode = 404;
          }
          else
          {
            if (!path.StartsWith("~"))
              path = path.StartsWith("/") || path.StartsWith("\\") ? "~" + path : "~/" + path;
            string str2 = context.Server.MapPath(path);
            string fileName = Path.GetFileName(str2);
            string str3 = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(str2) || !File.Exists(str2))
            {
              context.Response.StatusCode = 404;
            }
            else
            {
              string str4 = MimeMapping.GetMimeMapping(fileName);
              string lower = str3.ToLower();
              if (lower != null)
              {
                switch (lower.Length)
                {
                  case 4:
                    switch (lower[2])
                    {
                      case 'd':
                        if (lower == ".pdf")
                        {
                          str4 = "application/pdf";
                          goto label_51;
                        }
                        goto label_49;
                      case 'n':
                        if (lower == ".png")
                        {
                          str4 = "image/png";
                          goto label_51;
                        }
                        goto label_49;
                      case 'p':
                        if (lower == ".jpg")
                          break;
                        goto label_49;
                      default:
                        goto label_49;
                    }
                    break;
                  case 5:
                    switch (lower[1])
                    {
                      case 'd':
                        if (lower == ".docx")
                        {
                          str4 = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                          goto label_51;
                        }
                        goto label_49;
                      case 'j':
                        if (lower == ".jpeg")
                          break;
                        goto label_49;
                      case 'p':
                        if (lower == ".pptx")
                        {
                          str4 = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                          goto label_51;
                        }
                        goto label_49;
                      case 'x':
                        if (lower == ".xlsx")
                        {
                          str4 = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                          goto label_51;
                        }
                        goto label_49;
                      default:
                        goto label_49;
                    }
                    break;
                  default:
                    goto label_49;
                }
                str4 = "image/jpeg";
                goto label_51;
              }
label_49:
              if (string.IsNullOrEmpty(str4))
                str4 = "application/octet-stream";
label_51:
              context.Response.Clear();
              context.Response.ContentType = str4;
              context.Response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
              context.Response.BufferOutput = false;
              context.Response.TransmitFile(str2);
              context.ApplicationInstance.CompleteRequest();
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      context.Response.StatusCode = 500;
      context.Response.Write("An unexpected error occurred.");
    }
  }

  private string GetSqlByField(string field)
  {
    switch (field)
    {
      case "CopyFilePath":
      case "DocumentFilePath":
      case "DeclarationAIPath":
        return $"SELECT {field} FROM SimilarityAICheck_Submission WHERE SubmissionID = @ID";
      case "RawDataPath":
      case "ProposalCopyPath":
      case "Instrumentation":
      case "StatOutputPath":
      case "DeclarationPath":
      case "ReceiptPath":
        return $"SELECT {field} FROM StatComVal_Submission WHERE SubmissionID = @ID";
      case "StatComCertPath":
      case "MMSCCertFilePath":
      case "MMSCReportFilePath":
      case "SimilarityFilePath":
      case "AIFilePath":
        return $"SELECT {field} FROM ProcessorReturnedReports WHERE SubmissionID = @ID";
      default:
        return (string) null;
    }
  }

  public bool IsReusable => false;
}
