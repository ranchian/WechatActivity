using FJW.Unit;
using FJW.Wechat.WebApp.Base;
using System.Web.Mvc;

namespace FJW.Wechat.WebApp.Controllers
{
    [WAuthorize]
    public class TestController : WController
    {
        // GET: Test
        public ActionResult Index()
        {
            return Content(Request.Cookies.ToJson());
        }
    }
}