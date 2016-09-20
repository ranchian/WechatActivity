using FJW.Wechat.WebApp.Base;
using System;

using System.Web.Mvc;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using FJW.Unit;
using FJW.Wechat.Data;
using System.Linq;

namespace FJW.Wechat.WebApp.Controllers
{
    /// <summary>
    /// 微信授权
    /// </summary>
    public class WAuthorizeController : WController
    {
        public ActionResult UserInfoCallback(string code, string state)
        {
            Logger.Log("UserInfoCallback\t {0} \t {1}", code, state);
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }
            if (string.IsNullOrEmpty(state) || state != "4CA8CDEED2F3309F8B987DEEB3C1C1DD")
            {
                return Content("验证失败！请从正规途径进入！");
            }
            var result = OAuthApi.GetAccessToken(Config.WechatConfig.AppId, Config.WechatConfig.AppSecret, code);

            if (result.errcode != Senparc.Weixin.ReturnCode.请求成功)
            {
                return Content("错误：" + result.errmsg);
            }

            Session["OAuthAccessTokenStartTime"] = DateTime.Now;
            Session["OAuthAccessToken"] = result;
            try
            {
                OAuthUserInfo userInfo = OAuthApi.GetUserInfo(result.access_token, result.openid);
                UserInfo.OpenId = userInfo.openid;
                UserInfo.NickName = userInfo.nickname;
                UserInfo.HeadimgUrl = userInfo.headimgurl;
                UserInfo.Sex = userInfo.sex;
                UserInfo.Province = userInfo.province;
                UserInfo.City = userInfo.city;
                UserInfo.Country = userInfo.country;
                UserInfo.UnionId = userInfo.unionid;
                SetLoginInfo(UserInfo);

                var repository = new WeChatRepository("Wechat", Config.WechatConfig.MongoHost);

                var user = repository.Query<WeChatUserModel>(it => it.OpenId == UserInfo.OpenId).FirstOrDefault();
                var isNew = false;
                if (user == null)
                {
                    isNew = true;
                    user = new WeChatUserModel();
                }
                user.AccessToken = result.access_token;
                user.RefreshToken = result.refresh_token;
                user.ExpiresIn = result.expires_in;
                user.LastAuthorizeTime = DateTime.Now;
                user.OpenId = userInfo.openid;
                user.NickName = userInfo.nickname;
                user.HeadimgUrl = userInfo.headimgurl;
                user.Sex = userInfo.sex;
                user.Province = userInfo.province;
                user.City = userInfo.city;
                user.Country = userInfo.country;
                user.UnionId = userInfo.unionid;
                if (isNew)
                {
                    repository.Add(user);
                }
                else
                {
                    repository.Update(user);
                }
            }
            catch (ErrorJsonResultException ex)
            {
                return Content(ex.Message);
            }

            var url =  Session["url"] != null? Session["url"].ToString():"/";
            if (string.IsNullOrEmpty(url))
            {
                url = "/";
            }
            Response.Cookies.Add(new System.Web.HttpCookie("OpenId", UserInfo.OpenId));
            return Redirect(url);
        }

        /// <summary>
        /// 基础认证
        /// <remarks>主要目的获取OpenId </remarks>
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult BaseCallback(string code, string state) 
        {
            Logger.Log("BaseCallback\t {0} \t {1}", code, state);
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }

            if (string.IsNullOrEmpty(state) || state != "4CA8CDEED2F3309F8B987DEEB3C1C1DD")
            {
                return Content("验证失败！请从正规途径进入！");
            }

            var result = OAuthApi.GetAccessToken(Config.WechatConfig.AppId, Config.WechatConfig.AppSecret, code);
            if (result.errcode != Senparc.Weixin.ReturnCode.请求成功)
            {
                return Content("错误：" + result.ToJson());
            }

            Session["OAuthAccessTokenStartTime"] = DateTime.Now;
            Session["OAuthAccessToken"] = result;
            //Logger.Log("OAuthAccessToken:" + result.ToJson());
            UserInfo.OpenId = result.openid;
            SetLoginInfo(UserInfo);

            var url = Session["url"] != null ? Session["url"].ToString() : "/";
            if (string.IsNullOrEmpty(url))
            {
                url = "/";
            }
            return Redirect(url);
        }
         
    }
}