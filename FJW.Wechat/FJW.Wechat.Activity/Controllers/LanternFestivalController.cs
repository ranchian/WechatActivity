using System;
using System.Linq;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Activity.Controllers
{
    [CrossDomainFilter]
    public class LanternFestivalController : ActivityController
    {
        private static LanternFestivalConfig GetConfig()
        {
            return JsonConfig.GetJson<LanternFestivalConfig>("config/activity.lanternfestival.json");
        }

        private string GetMessage(string type,decimal money)
        {
            switch (type)
            {
                case "1":
                    return money.ToString("G0") + "元现金券";
                case "2":
                    return money.ToString("G0") + "%加息券";
                case "3":
                    return money.ToString("G0") + "元体验金";
                case "4":
                    return money.ToString("G0") + "元现金奖励";
            }
            return string.Empty;
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
                return new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "用户未登录!" };
            }
            var config = GetConfig();

            var now = DateTime.Now;
            if (now < config.StartTime || now > config.EndTime)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "活动未开始或已过期" };

            var list = new ActivityRepository(DbName, MongoHost).Query<LuckdrawModel>(it => it.Key == config.ActivityKey && it.MemberId == userId).ToList();
            var count=list.Count; 
            if (count == config.LimitTimes)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "奖励次数已领取三次", Data = list.Select(m => GetMessage(m.Type, m.Money)) };

            return new ResponseModel { ErrorCode = ErrorCode.None , Data= list.Select(m => GetMessage(m.Type, m.Money))};
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
            Logger.Dedug("subjectNo:{0}", subjectNo);
            var rst = Verify();
            if (rst.ErrorCode != ErrorCode.None)
                return Json(rst);

            var config = GetConfig();
           
            var subject = config.Subjects.FirstOrDefault(m => m.SubjectNo == subjectNo);
            if(subject==null)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message="答题编号不存在" });
              
            return Json(SendAward(config,subject));
        }

        /// <summary>
        /// 发送奖励
        /// </summary>
        /// <returns></returns>
        private ResponseModel SendAward(LanternFestivalConfig config, LanternFestivalConfig.Subject subject)
        {
            var remark=string.Empty;
            var memberRepository = new MemberRepository(SqlConnectString);
            if (subject.Type == LanternFestivalConfig.AwardType.ExperienceGold)
            {
                remark= memberRepository.Give(UserInfo.Id, subject.CouponNo, 2, subject.Amount, DateTime.Now.Ticks).ToString();
            }
            else if (subject.Type == LanternFestivalConfig.AwardType.RewardGold)
            {
                memberRepository.GiveMoney(UserInfo.Id, subject.Amount, subject.CouponNo, DateTime.Now.Ticks);
            }
            else
            {
                var response = CardCouponApi.UserGrant(UserInfo.Id, config.ActivityId, subject.CouponNo);
                if (!response.IsOk)
                    return new ResponseModel {ErrorCode = ErrorCode.Exception, Message = response.ExceptionMessage};
                remark = response.Data;
            }

            var luckdrawModel = new LuckdrawModel()
            {
                Key = config.ActivityKey,
                MemberId = UserInfo.Id,
                Name = config.ActivityName,
                Type = ((int) subject.Type).ToString(),
                Status = 0,
                Money = subject.Amount,
                Prize = subject.CouponNo,
                Phone = UserInfo.Phone,
                Remark= remark
            };
            new ActivityRepository(DbName, MongoHost).Add(luckdrawModel);
            return new ResponseModel {ErrorCode = ErrorCode.None};
        }
    }
}
