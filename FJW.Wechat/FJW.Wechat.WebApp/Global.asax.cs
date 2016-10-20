using FJW.Unit;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Senparc.Weixin.MP.Containers;

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

            MvcHandler.DisableMvcResponseHeader = true;
            
            //全局只需注册一次
            AccessTokenContainer.Register(Config.WechatConfig.AppId, Config.WechatConfig.AppSecret);

            //全局只需注册一次
            JsApiTicketContainer.Register(Config.WechatConfig.AppId, Config.WechatConfig.AppSecret);
        }

        protected void Application_End()
        {
            RedisManager.Disponse();
        }
    }
}