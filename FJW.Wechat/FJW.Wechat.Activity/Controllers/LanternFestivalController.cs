using System;
using System.Linq;
using System.Web.Mvc;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Activity.Controllers
{
    public class LanternFestivalController : ActivityController
    {
        private static LanternFestivalConfig GetConfig()
        {
            return JsonConfig.GetJson<LanternFestivalConfig>("config/activity.lanternfestival.json");
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        private ResponseModel Verify()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return new ResponseModel { ErrorCode = ErrorCode.NotLogged };
            }
            var config = GetConfig();

            var now = DateTime.Now;
            if (now < config.StartTime || now > config.EndTime)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "活动未开始或已过期" };

            var count = new ActivityRepository(DbName, MongoHost).Query<LuckdrawModel>(it => it.Key == config.ActivityKey && it.MemberId == userId).Count();
            if (count == config.LimitTimes)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "奖励次数已领取三次" };

            return new ResponseModel { ErrorCode = ErrorCode.None };
        }
        [HttpGet]
        public ActionResult Chances()
        { 
            return Json(Verify());
        }
        /// <summary>
        /// 答题正确的题号 1-10
        /// </summary>
        /// <param name="subjectNo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitAnswer(int subjectNo)
        {
            var rst = Verify();
            if (rst.ErrorCode != ErrorCode.None)
                return Json(rst);

            var repository = new ActivityRepository(DbName, MongoHost);
            return Json(new { });
        }
    }
}
