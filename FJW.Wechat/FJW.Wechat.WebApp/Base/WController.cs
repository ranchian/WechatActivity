using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using FJW.Unit;
using FJW.Wechat.WebApp.Models;

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

        internal void SetLoginInfo(UserInfo info)
        {
            _userInfo = info;
            HttpContext.Session[UserSessionKey] = info;
        }

        internal void RemoveLoginInfo()
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