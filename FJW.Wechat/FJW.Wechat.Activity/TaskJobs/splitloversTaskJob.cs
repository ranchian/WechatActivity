using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.Unit;
using FJW.Wechat.Activity.Controllers;
using FJW.Wechat.Data;
using Quartz.Util;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Cache;

namespace FJW.Wechat.Activity.TaskJobs
{
    public class splitloversTaskJob : IJob
    {

        private const string Key = "splitlovers";
        private readonly ActivityRepository _repsitory;

        /// <summary>
        /// 24时更新排行榜 发放奖励
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var config = GetConfig();
                List<RecordModel> data;
                var date = DateTime.Now.Date.AddDays(config.GiveTimeDiff);
                var num = 0;
                int cnt;
                data = new ActivityRepository(Config.ActivityConfig.DbName, Config.ActivityConfig.MongoHost).QueryDesc<RecordModel, int>(it => it.Key == Key && it.MemberId != 0
                && it.Phone != "" && it.Result != 0 && it.CreateTime >= date  && it.CreateTime < date.AddDays(1)
                , it => it.Result, 20, 0, out cnt).ToList();

                //发放奖励
                //上榜用户数 
                var mebCount = data.Count;
                for (int i = 0; i < (mebCount >= 10 ? 10 : data.Count); i++)
                {
                    ExchangePrizes(data[i].MemberId, i+1);
                }
               

            }
            catch (Exception ex)
            {
                Logger.Info("Execute:{0}", ex.ToString());
            }
        }

        //获取奖励配置
        private static SplitloversConfig GetConfig()
        {
            return JsonConfig.GetJson<SplitloversConfig>("Config/activity.splitloversvalue.json");
        }

        /// <summary>
        /// 发放奖励
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="ranking">排名</param>
        private void ExchangePrizes(long memberId, int ranking)
        {
            var config = GetConfig();
            object result;
            var sqlConnectString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            switch (ranking)
            {
                case 1:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponA);
                    Logger.Info("splicelovers memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;

                case 2:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponB);
                    Logger.Info("splicelovers memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;
                case 3:
                    new MemberRepository(sqlConnectString).Give(memberId, config.ExperienceId, 2, config.MoneyA, memberId);
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    new MemberRepository(sqlConnectString).Give(memberId, config.ExperienceId, 2, config.MoneyB, memberId);
                    break;
            }
        }
    }
}
