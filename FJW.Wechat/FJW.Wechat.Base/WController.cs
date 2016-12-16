using System;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using FJW.Unit;

namespace FJW.Wechat
{
    public abstract class WController : Controller
    {
        #region User Info

        private const string UserSessionKey = "SessionUserInfo";
        private UserInfo _userInfo;

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInfo UserInfo
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

        public void SetLoginInfo(UserInfo info)
        {
            _userInfo = info;
            HttpContext.Session[UserSessionKey] = info;
        }

        public void RemoveLoginInfo()
        {
            _userInfo = new UserInfo();
            HttpContext.Session[UserSessionKey] = _userInfo;
        }

        #endregion User Info

        #region override
        
        protected override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.IsChildAction && filterContext.Exception != null && !filterContext.ExceptionHandled)
            {
                Logger.Error(filterContext.Exception);
                filterContext.ExceptionHandled = true;
                var enableStr = WebConfigurationManager.AppSettings["CorssDomainFilterEnable"];
                if (enableStr == null || enableStr.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    var origin = filterContext.HttpContext.Request.Headers["Origin"];
                    if (!string.IsNullOrEmpty(origin))
                    {
                        filterContext.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = origin;
                        filterContext.HttpContext.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                        filterContext.HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
                        filterContext.HttpContext.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST";
                    }
                }
                
                if (Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonetResult { Data = new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "error" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    filterContext.Result = RedirectToAction("Error", "Index");
                }
            }
            else
            {
                Logger.Error("IsChildAction:{0},Exception:{1}, ExceptionHandled:{2}", filterContext.IsChildAction, filterContext.Exception.ToJson(), filterContext.ExceptionHandled);
                base.OnException(filterContext);
            }
        }

        #endregion override

        #region json

        protected new JsonetResult Json(object data, JsonRequestBehavior behavior = JsonRequestBehavior.AllowGet)
        {
            return Json(data, "application/json", Encoding.UTF8, behavior);
        }
        protected new JsonetResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        #endregion json
    }
}