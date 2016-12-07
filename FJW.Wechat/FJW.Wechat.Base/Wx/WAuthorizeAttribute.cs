using System;
using System.Web;
using System.Web.Mvc;
using FJW.Unit;

namespace FJW.Wechat.Wx
{
    /// <summary>
    /// 微信授权 过滤器
    /// <remark>只对WController起作用 </remark>
    /// </summary>
    public class WAuthorizeAttribute : FilterAttribute, IActionFilter
    {
        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public virtual void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public virtual void OnResultExecuted(ResultExecutedContext filterContext)
        {

        }
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var wcontroller = filterContext.Controller as WController;
            if (wcontroller == null)
            {
                return;
            }
            var u = wcontroller.UserInfo;
            var req = filterContext.RequestContext.HttpContext.Request;
            var appid = Config.WechatConfig.AppId;
            if (req.Url == null)
            {
                return;
            }
            if (filterContext.HttpContext.Session != null)
                 filterContext.HttpContext.Session["url"] = req.Url.PathAndQuery;

            Logger.Dedug("Cookies" + req.Cookies.ToJson());

            if (string.IsNullOrEmpty(u.OpenId))
            {
                //1.首先 确定 OpenId
                var callbackurl = string.Format("{0}/WAuthorize/BaseCallback", req.Url.AbsoluteUri.Replace(req.Url.PathAndQuery, string.Empty));
                var url = GetAuthorizeUrl(appid, callbackurl, "4CA8CDEED2F3309F8B987DEEB3C1C1DD", OAuthScope.Base);
                filterContext.Result = new RedirectResult(url);
                Logger.Dedug("BaseCallback:{0}", url);
                return;
            }
            if (u.WxUserInfo == null )
            {
                //Logger.Dedug("OpenId:" + u.OpenId);
                
                var wxUserInfo = GetAuthorizeInfo( u.OpenId);
                if (wxUserInfo == null)
                {
                    //2.如果 库中不存在OpenId, 提示授权
                    var callbackurl = string.Format("{0}/WAuthorize/UserInfoCallback", req.Url.AbsoluteUri.Replace(req.Url.PathAndQuery, string.Empty));
                    var url = GetAuthorizeUrl(appid, callbackurl, "4CA8CDEED2F3309F8B987DEEB3C1C1DD",  OAuthScope.UserInfo);
                    filterContext.Result = new RedirectResult(url);
                    Logger.Dedug("UserInfoCallback:{0}", url);
                }
                else
                {
                    u.WxUserInfo = wxUserInfo;
                    u.Id = wxUserInfo.MemberId;
                    wcontroller.SetLoginInfo(u);
                }
            }
        }

        private static string GetAuthorizeUrl(string appId, string redirectUrl, string state , string scope, string responseType = "code", bool addConnectRedirect = true)
        {
            return string.Format(
                    "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type={2}&scope={3}&state={4}{5}#wechat_redirect",
                    AsUrlData(appId),
                    AsUrlData(redirectUrl),
                    AsUrlData(responseType),
                    AsUrlData(scope),
                    AsUrlData(state),
                    addConnectRedirect ? "&connect_redirect=1" : ""
                );
        }

        private static string AsUrlData(string data)
        {
            return Uri.EscapeDataString(data);
        }

        private WxUserInfo GetAuthorizeInfo(string openId)
        {
            var repository = DependencyResolver.Current.GetService<IWxAuthenRepository>();
            return repository.GetUserInfo(openId);
        }
    }

    public struct OAuthScope
    {
        /// <summary>
        /// 不弹出授权页面，直接跳转，只能获取用户openid
        /// </summary>
        public const string Base = "snsapi_base";


        /// <summary>
        /// 弹出授权页面，可通过openid拿到昵称、性别、所在地。并且，即使在未关注的情况下，只要用户授权，也能获取其信息
        /// </summary>
        public const string UserInfo = "snsapi_userinfo";
    }
}