using System.Web.Mvc;

namespace FJW.Wechat.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return Content("出错了");
        }
    }
}