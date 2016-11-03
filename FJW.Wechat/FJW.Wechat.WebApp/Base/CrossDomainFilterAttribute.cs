
using System;
using System.Web.Configuration;
using System.Web.Mvc;

namespace FJW.Wechat.WebApp.Base
{
    /// <summary>
    /// MvcAction跨域支持
    /// </summary>
    public class CrossDomainFilterAttribute: ActionFilterAttribute
    {
 
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var enableStr = WebConfigurationManager.AppSettings["CorssDomainFilterEnable"];
            if (enableStr != null && enableStr.Equals("false", StringComparison.CurrentCultureIgnoreCase))
            {
                base.OnActionExecuting(filterContext);
                return;
            }
            var origin = filterContext.HttpContext.Request.Headers["Origin"];

            if (!string.IsNullOrEmpty(origin))
            {
                filterContext.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = origin;
                filterContext.HttpContext.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                filterContext.HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
                filterContext.HttpContext.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST";
            }
            base.OnActionExecuting(filterContext);
        }
    }
}