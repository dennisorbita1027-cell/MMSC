// Decompiled with JetBrains decompiler
// Type: CASApp1.Global
// Assembly: CASApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D01C93EE-CF3A-4F46-ABD3-3907A01835BC
// Assembly location: C:\Users\orbit\OneDrive\Documents\Silver\Project\Project\mmsc1\MmscPublished\bin\CASApp1.dll

using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

#nullable disable
namespace CASApp1;

public class Global : HttpApplication
{
  private void Application_Start(object sender, EventArgs e)
  {
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);
  }
}
