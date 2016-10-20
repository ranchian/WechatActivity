using System.Web.Mvc;

using FJW.Unit;
using FJW.Wechat.WebApp.Base;

namespace FJW.Wechat.WebApp.Controllers
{

    public class TestController : WController
    {


        // GET: Test
        [WAuthorize]
        public ActionResult Index()
        {
            return Content(Request.Cookies.ToJson());
        }
#if DEBUG
        public ActionResult I()
        {
            var i = RedisManager.GetIncrement("Activitiy:Test");
            return Content(i.ToString());
        }

#endif
    }
}