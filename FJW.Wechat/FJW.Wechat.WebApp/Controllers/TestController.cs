using FJW.Unit;
using FJW.Wechat.WebApp.Base;
using System.Web.Mvc;

namespace FJW.Wechat.WebApp.Controllers
{
    //[WAuthorize]
    public class TestController : WController
    {
#if DEBUG
        // GET: Test
        [WAuthorize]
        public ActionResult Index()
        {
            return Content(Request.Cookies.ToJson());
        }

        public ActionResult I()
        {
            var i = RedisManager.GetIncrement("Activitiy:Test");
            return Content(i.ToString());
        }

#endif


    }
}