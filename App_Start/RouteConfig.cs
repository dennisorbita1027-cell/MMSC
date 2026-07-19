// Decompiled with JetBrains decompiler
// Type: CASApp1.RouteConfig
// Assembly: CASApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D01C93EE-CF3A-4F46-ABD3-3907A01835BC
// Assembly location: C:\Users\orbit\OneDrive\Documents\Silver\Project\Project\mmsc1\MmscPublished\bin\CASApp1.dll

using Microsoft.AspNet.FriendlyUrls;
using System.Web.Routing;

#nullable disable
namespace CASApp1;

public static class RouteConfig
{
  public static void RegisterRoutes(RouteCollection routes)
  {
    routes.EnableFriendlyUrls(new FriendlyUrlSettings()
    {
      AutoRedirectMode = RedirectMode.Permanent
    });
  }
}
