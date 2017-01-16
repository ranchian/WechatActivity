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
        public JsonetResult Exchange(long orderId)
        {
            var n = new SqlDataRepository(SqlConnectString).GetSpringFestivalMutiple(orderId);
            return Json(new {n});
        }
    }
}
