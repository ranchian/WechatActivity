using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FJW.Wechat.Data;

namespace FJW.Wechat.Activity.Controllers
{
    public class ProductController: ActivityController
    {
        public ActionResult Lasted(int id)
        {
            var sqlRepository = new SqlDataRepository(SqlConnectString);
            return Json(new ResponseModel
            {
                Data = sqlRepository.GetLasted(id)
            });
        }
    }
}
