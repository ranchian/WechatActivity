using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 二月红
    /// </summary>
    public class FebruaryController: ActivityController
    {
        private const string GameKey = "FebRed";

        private static FebruaryConfig GetConfig()
        {
            return JsonConfig.GetJson<FebruaryConfig>("config/activity.february.json");
        }

        /// <summary>
        /// 排行榜
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = 60)]
        public ActionResult Rank()
        {
            var config = GetConfig();
            return Json(new ResponseModel()
            {
                Data = new SqlDataRepository(SqlConnectString).ProductBuyRanking(config.StartTime, config.EndTime)
            });
        }

        [HttpPost]
        public ActionResult Exchange()
        {
            return null;
        }


    }
}
