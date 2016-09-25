using System;
using System.Web.Mvc;
using System.Collections.Specialized;

using FJW.Unit.Helper;
using FJW.Wechat.Data;
using FJW.CommonLib.Utils;
using FJW.CommonLib.XService;
using FJW.CommonLib.ExtensionMethod;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    public class MemberController : ActivityController
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="pswd">登录密码</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(string phone, string pswd)
        {
            Models.ResultModel<string> result = new Models.ResultModel<string>();
            result.Result = "登录失败";
            result.Success = 0;

            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    result.Result = "手机号码不可为空";
                }
                else if (string.IsNullOrEmpty(pswd))
                {
                    result.Result = "登录密码不可为空";
                }
                else
                {
                    var req = new
                    {
                        Phone = phone,
                        Pswd = pswd
                    };
                    ServiceResult response = ServiceEngine.Request("Login", req, 0, 60000);
                    if (response.Status == 0)
                    {
                        var resp = JsonHelper.JsonDeserialize<MemberModel>(response.Content);

                        //设置cookie缓存
                        NameValueCollection nvc = new NameValueCollection();
                        nvc.Add("token", JsonHelper.JsonSerializer(resp));
                        CookieHelper.WriteCookie("fangjinnet.com", nvc, 0);

                        //根据token读取ID
                        var mberRepository = new MemberRepository(SqlConnectString);
                        var memberInfo = mberRepository.GetMemberInfo(resp.Token);

                        result.Success = 1;
                        result.Result = "操作成功";

                        if (memberInfo == null)
                        {
                            return Content("无效的登录结果");
                        }
                        UserInfo.Id = memberInfo.MemberId;
                        UserInfo.Token = memberInfo.Token;
                        SetLoginInfo(UserInfo);
                    }
                    else
                    {
                        result.Success = 0;
                        result.Result = response.ExceptionMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return Json(result);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="code">验证码</param>
        /// <param name="pswd">密码</param>
        /// <param name="inviterPhone">邀请人手机号码</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Regist(string phone, string code, string pswd, string inviterPhone, string channel)
        {
            Models.ResultModel<string> model = new Models.ResultModel<string>();
            model.Result = "注册失败";
            model.Success = 0;

            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    model.Result = "手机号码不可为空";
                }
                else if (string.IsNullOrEmpty(code))
                {
                    model.Result = "验证码不可为空";
                }
                else if (string.IsNullOrEmpty(pswd))
                {
                    model.Result = "登录密码不可为空";
                }
                else
                {
                    var req = new
                    {
                        Phone = phone,
                        VCode = code,
                        Pswd = pswd,
                        FriendPhone = inviterPhone,
                        Channel = channel
                    };
                    ServiceResult result = ServiceEngine.Request("Regist", req.ToJSON());
                    if (result.Status == 0)
                    {
                        model.Result = "登录成功";
                        model.Success = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return View();
        }
    }
}