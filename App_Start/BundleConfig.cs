// Decompiled with JetBrains decompiler
// Type: CASApp1.BundleConfig
// Assembly: CASApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D01C93EE-CF3A-4F46-ABD3-3907A01835BC
// Assembly location: C:\Users\orbit\OneDrive\Documents\Silver\Project\Project\mmsc1\MmscPublished\bin\CASApp1.dll

using System.Web.Optimization;
using System.Web.UI;

#nullable disable
namespace CASApp1;

public class BundleConfig
{
  public static void RegisterBundles(BundleCollection bundles)
  {
    BundleConfig.RegisterJQueryScriptManager();
    bundles.Add(new ScriptBundle("~/bundles/WebFormsJs").Include("~/Scripts/WebForms/WebForms.js", "~/Scripts/WebForms/WebUIValidation.js", "~/Scripts/WebForms/MenuStandards.js", "~/Scripts/WebForms/Focus.js", "~/Scripts/WebForms/GridView.js", "~/Scripts/WebForms/DetailsView.js", "~/Scripts/WebForms/TreeView.js", "~/Scripts/WebForms/WebParts.js"));
    bundles.Add(new ScriptBundle("~/bundles/MsAjaxJs").Include("~/Scripts/WebForms/MsAjax/MicrosoftAjax.js", "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js", "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js", "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"));
    bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));
  }

  public static void RegisterJQueryScriptManager()
  {
    ScriptManager.ScriptResourceMapping.AddDefinition("jquery", new ScriptResourceDefinition()
    {
      Path = "~/scripts/jquery-3.7.0.min.js",
      DebugPath = "~/scripts/jquery-3.7.0.js",
      CdnPath = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.7.0.min.js",
      CdnDebugPath = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.7.0.js"
    });
  }
}
