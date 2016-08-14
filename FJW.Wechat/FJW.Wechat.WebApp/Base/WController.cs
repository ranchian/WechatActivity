using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FJW.Wechat.WebApp.Base
{
    public abstract class WController : Controller
    {


        #region User Info

        private const string UserSessionKey = "SessionUserInfo";
        private UserInfo _userInfo;

        /// <summary>
        /// 用户信息
        /// </summary>
        internal UserInfo UserInfo
        {
            get
            {
                if (_userInfo == null)
                {
                    _userInfo = HttpContext.Session[UserSessionKey] as UserInfo ?? new UserInfo();
                }
                return _userInfo;
            }
        }

        protected void SetLoginInfo(UserInfo info)
        {
            _userInfo = info;
            HttpContext.Session[UserSessionKey] = info;
        }

        protected void RemoveLoginInfo()
        {
            _userInfo = new UserInfo();
            HttpContext.Session[UserSessionKey] = _userInfo;
        }

        #endregion

        #region

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Logger.Debug("OnActionExecuting:" + Newtonsoft.Json.JsonConvert.SerializeObject(filterContext.HttpContext.Session[UserSessionKey]));
            base.OnActionExecuting(filterContext);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                Logger.Error(filterContext.Exception);
                filterContext.ExceptionHandled = true;
            }
            base.OnException(filterContext);
        }


        #endregion


        #region json

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior) {
            return new JsonetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }
        #endregion
    }
}