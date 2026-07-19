// Decompiled with JetBrains decompiler
// Type: CASApp1.ViewSwitcher
// Assembly: CASApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D01C93EE-CF3A-4F46-ABD3-3907A01835BC
// Assembly location: C:\Users\orbit\OneDrive\Documents\Silver\Project\Project\mmsc1\MmscPublished\bin\CASApp1.dll

using Microsoft.AspNet.FriendlyUrls.Resolvers;
using System;
using System.Web;
using System.Web.Routing;
using System.Web.UI;

#nullable disable
namespace CASApp1;

public class ViewSwitcher : UserControl
{
  protected string CurrentView { get; private set; }

  protected string AlternateView { get; private set; }

  protected string SwitchUrl { get; private set; }

  protected void Page_Load(object sender, EventArgs e)
  {
    bool flag = WebFormsFriendlyUrlResolver.IsMobileView((HttpContextBase) new HttpContextWrapper(this.Context));
    this.CurrentView = flag ? "Mobile" : "Desktop";
    this.AlternateView = flag ? "Desktop" : "Mobile";
    string str = "AspNet.FriendlyUrls.SwitchView";
    if (RouteTable.Routes[str] == null)
      this.Visible = false;
    else
      this.SwitchUrl = $"{this.GetRouteUrl(str, (object) new
      {
        view = this.AlternateView,
        __FriendlyUrls_SwitchViews = true
      })}?ReturnUrl={HttpUtility.UrlEncode(this.Request.RawUrl)}";
  }
}
