using System;
using System.Web.Mvc;
using FJW.SDK2Api.Member;
using FJW.Unit;
using FJW.Wechat.Activity.Models;
using FJW.Wechat.Data;

namespace FJW.Wechat.Activity.Controllers
{
    [CrossDomainFilter]
    public class MemberController : ActivityController
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="pswd">登录密码</param>
        /// <returns></returns>
        //[CrossDomainFilter]
        [HttpPost]
        public ActionResult Login(string phone, string pswd)
        {
            ResultModel<string> result = new ResultModel<string>
            {
                Result = "登录失败",
                Success = 0
            };

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
                    
                    var response = AccountApi.Login(phone, pswd);

                    if (response.IsOk )
                    {
                        Logger.Dedug(response.ToJson());
                        var resp = response.Content;
                        

                        //根据token读取ID
                        var mberRepository = new MemberRepository(SqlConnectString);
                        var memberInfo = mberRepository.GetMemberInfo(resp.Token);

                        result.Success = 1;
                        result.Result = "操作成功";

                        if (memberInfo == null)
                        {
                            return Content("无效的登录结果");
                        }

                        UserInfo.Phone = phone;
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
                Logger.Error(ex );
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
        //[CrossDomainFilter]
        [HttpPost]
        public ActionResult Regist(string phone, string code, string pswd, string inviterPhone, string channel)
        {
            var message = "注册成功"; ;
            var success = 0;
            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    message = "手机号码不可为空";
                }
                else if (string.IsNullOrEmpty(code))
                {
                    message = "验证码不可为空";
                }
                else if (string.IsNullOrEmpty(pswd))
                {
                    message = "登录密码不可为空";
                }
                else
                {
                    var result = AccountApi.Regist(phone, pswd, code, inviterPhone, channel);//.Request("Regist", req.ToJSON());
                    if (result.IsOk)
                    {
                        success = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                message = "注册错误，请稍后重试";
                Logger.Error(ex.Message);
            }
            return Json(new {message, success});
        }
    }
}