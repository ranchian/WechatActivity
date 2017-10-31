using System;
using System.Web;
using FJW.Unit;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using FJW.Wechat.Activity.Rules;
using FJW.Wechat.Rules;
using FJW.Wechat.WebApp.Providers;
using FJW.Wechat.Wx;
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



            var builder = new ContainerBuilder();

            builder.RegisterType<WxMediaApi>().As<IWxMediaApi>();
            builder.RegisterType<MongoWxAuthenRepository>().As<IWxAuthenRepository>();
            builder.RegisterType<BoxWordRule>().As<ITextRule>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            //JobConfig.Start();


        }


        

        protected void Application_End()
        {
            RedisManager.Disponse();
            //JobConfig.Stop();
        }
    }
}