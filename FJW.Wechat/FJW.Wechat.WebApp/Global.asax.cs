using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FJW.Unit;

namespace FJW.Wechat.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RedisManager.Init(Config.RedisConfig.ConnectionString);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            RedisManager.Disponse();
        }
    }
}
