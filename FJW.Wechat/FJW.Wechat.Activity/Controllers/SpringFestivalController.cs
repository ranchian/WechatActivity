using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FJW.Wechat.Data;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 春节收益翻倍活动
    /// </summary>
    [CrossDomainFilter]
    public class SpringFestivalController : ActivityController
    {
        public ActionResult Exchange(long orderId)
        {
            var n = new SqlDataRepository(SqlConnectString).GetSpringFestivalMutiple(orderId);
            if (n > 0)
            {
                return Json(new ResponseModel {Data = n});
            }
            return Json(new ResponseModel(ErrorCode.Other) {Message = "该笔交易无法参与本次活动"});
        }

        [OutputCache(Duration = 60)]
        public ActionResult Record()
        {
            return Json(new ResponseModel {Data = new SqlDataRepository(SqlConnectString).GetSpringFestivalRows()});
        }
    }
}
