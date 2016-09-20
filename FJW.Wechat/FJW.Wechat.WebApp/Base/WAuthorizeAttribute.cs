using FJW.Unit;
using FJW.Wechat.Data;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FJW.Wechat.WebApp.Base
{
    /// <summary>
    /// 微信授权 过滤器
    /// <remark>只对WController起作用 </remark>
    /// </summary>
    public class WAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var wcontroller = filterContext.Controller as WController;
            if (wcontroller == null)
            {
                return;
            }
            var u = wcontroller.UserInfo;
            var req = filterContext.RequestContext.HttpContext.Request;
            var appid = Config.WechatConfig.AppId;
            if (filterContext.HttpContext.Session != null)
                filterContext.HttpContext.Session["url"] = req.Url.PathAndQuery;

            Logger.Log("Cookies" + req.Cookies.ToJson());

            if (string.IsNullOrEmpty(u.OpenId))
            {
                Logger.Log("OpenId is null");
                //1.首先 确定 OpenId
                var callbackurl = string.Format("{0}/WAuthorize/BaseCallback", req.Url.AbsoluteUri.Replace(req.Url.PathAndQuery, string.Empty));
                var url = OAuthApi.GetAuthorizeUrl(appid, callbackurl, "4CA8CDEED2F3309F8B987DEEB3C1C1DD", Senparc.Weixin.MP.OAuthScope.snsapi_base);
                filterContext.Result = new RedirectResult(url);
            }
            else
            {
                Logger.Log("OpenId:"+ u.OpenId);
                var repository = new WeChatRepository("Wechat", Config.ActivityConfig.MongoHost);
                var wxUserInfo = repository.Query<WeChatUserModel>(it => it.OpenId == u.OpenId).FirstOrDefault();
                if (wxUserInfo == null)
                {
                    //2.如果 库中不存在OpenId, 提示授权    
                    var callbackurl = string.Format("{0}/WAuthorize/UserInfoCallback", req.Url.AbsoluteUri.Replace(req.Url.PathAndQuery, string.Empty));
                    var url = OAuthApi.GetAuthorizeUrl(appid, callbackurl, "4CA8CDEED2F3309F8B987DEEB3C1C1DD", Senparc.Weixin.MP.OAuthScope.snsapi_userinfo);
                    filterContext.Result = new RedirectResult(url);
                }
                else
                {
                    u.Sex = wxUserInfo.Sex;
                    u.City = wxUserInfo.City;
                    u.Country = wxUserInfo.Country;
                    u.Province = wxUserInfo.Province;
                    u.OpenId = wxUserInfo.OpenId;
                    u.NickName = wxUserInfo.NickName;
                    u.HeadimgUrl = wxUserInfo.HeadimgUrl;
                    u.Id = wxUserInfo.MemberId;
                    u.Privilege = wxUserInfo.Privilege;
                    wcontroller.SetLoginInfo(u);
                }
            }
        }
    }


}