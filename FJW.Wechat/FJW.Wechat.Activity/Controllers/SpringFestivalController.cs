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
            if (orderId == 0)
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "该笔交易无法参与本次活动" });
            }
            var tuple = new SqlDataRepository(SqlConnectString).GetSpringFestivalMultiple(orderId);
            if (tuple.Item1 > 0)
            {
                return Json(new ResponseModel {Data = new {multiple = tuple.Item1, productTypeId = tuple.Item2}});
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
