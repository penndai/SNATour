using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SNAtours
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(Object source, EventArgs e)
        {
            if (!Context.Request.IsSecureConnection &&
                !Request.Url.Host.Contains("localhost") &&
                Request.Url.AbsolutePath.Contains("sna/Login"))
            {
                Response.Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
            }

        }
    }
}
