using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data.Model.RDBS;

namespace FJW.Wechat.Activity.Controllers
{
    [CrossDomainFilter]
    public class SpringDragonController : ActivityController
    {
        private static SpringDragonConfig GetConfig()
        {
            return JsonConfig.GetJson<SpringDragonConfig>("config/activity.springdragon.json");
        }
        [HttpGet]
        public ActionResult Ranking()
        {
            var repository = new SqlDataRepository(SqlConnectString);
            var rankingDatas = repository.GetSpringDragonRanking(GetConfig().ProductId);
            return Json(rankingDatas);
        } 
    }
}
