using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

using FJW.Unit;
using FJW.Wechat.Activity.Controllers;
using FJW.Wechat.Data;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Cache;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Activity.TaskJobs
{
    public class ArbordayTaskJob : IJob
    {

        private const string Key = "arborday";
        /// <summary>
        /// 0点更新排行榜 发放奖励
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var config = GetConfig();
                var date = DateTime.Now.Date.AddDays(config.GiveTimeDiff);
                var num = 0;
                int cnt;
                var data = new ActivityRepository(Config.ActivityConfig.DbName, Config.ActivityConfig.MongoHost).
                    QueryDesc<RecordModel, int>(it => it.Key == Key && it.MemberId != 0 && it.Phone != "" && it.Result != 0 && it.CreateTime >= date && it.CreateTime < date.AddDays(1)
                    , it => it.Result, 20, 0, out cnt).OrderByDescending(it => it.Result).ThenBy(it => it.CreateTime).ToList();

                //给上榜用户数 发放奖励 /更新每日排行榜
                List<LuckdrawModel> luckModels = new List<LuckdrawModel>();
                var mebCount = data.Count;
                for (int i = 0; i < (mebCount >= 20 ? 20 : data.Count); i++)
                {
                    long counponId = 0;
                    var memberId = data[i].MemberId;
                    var name = "";

                    bool isReceive = new ArborDayController().HasCount(memberId);

                    //是否发放奖励 
                    if (isReceive)
                    {
                        name = ExchangePrizes(memberId, i + 1, out counponId);
                    }
                    else
                        Logger.Info($"MemberId: {memberId},Phone : {data[i].Phone} 奖励次数已用完。");


                    luckModels.Add(new LuckdrawModel
                    {
                        Key = Key,
                        MemberId = memberId,
                        Phone = data[i].Phone,
                        Sequnce = i,
                        Prize = counponId,

                        Name = name,
                        Status = isReceive ? 1 : 0,//上榜用户是否发送奖励
                        Remark = data[i].LastUpdateTime.ToString()
                    });
                }
                if (luckModels.Count > 0)
                    new ActivityRepository(Config.ActivityConfig.DbName, Config.ActivityConfig.MongoHost).AddMany(luckModels);



            }
            catch (Exception ex)
            {
                Logger.Info("Execute:{0}", ex.ToString());
            }
        }

        //获取奖励配置
        private static ArbordayConfig GetConfig()
        {
            return JsonConfig.GetJson<ArbordayConfig>("Config/activity.arbordayvalue.json");
        }

        /// <summary>
        /// 发放奖励 
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="ranking">排名</param>
        /// <param name="counponId">奖励卡券编号</param>
        private string ExchangePrizes(long memberId, int ranking, out long counponId)
        {
            var config = GetConfig();
            string name;
            object result;
            counponId = 0;
            switch (ranking)
            {
                case 1:
                    //话费
                    name = "50元手机话费";
                    break;
                case 2:
                    //爱奇艺
                    name = "爱奇艺VIP月会员";
                    break;
                case 3:
                    //5%加息券
                    name = "5%加息券";
                    counponId = Convert.ToInt64(config.RateCouponA);
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponA);
                    Logger.Info("arborday memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;
                default:
                    //10元现金券
                    name = "10元现金券";
                    counponId = Convert.ToInt64(config.RateCouponB);
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponB);
                    Logger.Info("arborday memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;
            }
            return name;
        }
    }
}
