using System;
using System.Collections.Generic;
using System.Linq;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using Quartz;

namespace FJW.Wechat.Activity.Controllers.TaskJobs
{
    public class FlowersTaskJob : IJob
    {
        private const string Key = "flowers";

        /// <summary>
        /// 31日更新统计信息 赠送成长值
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var mongoConn = new ActivityRepository(Config.ActivityConfig.DbName, Config.ActivityConfig.MongoHost);
                var config = GetConfig();
                var date = config.StartStaticsTime.Date;
                var endDate = config.EndStaticsTime.Date;
                List<TotalChanceModel> totalData = mongoConn.Query<TotalChanceModel>(it => it.Key == Key && it.Score > 0).ToList();

                Logger.Info($"赠送成长值 Job Execute : Start Time :{DateTime.Now}");
                //统计活动信息
                foreach (var item in totalData)
                {
                    MemberDataStatics(item, date, endDate);
                }
                Logger.Info($"赠送成长值 Job Execute : End Time :{DateTime.Now}");
            }
            catch (Exception ex)
            {
                Logger.Info("Execute:{0}", ex.ToString());
            }
        }

        //获取奖励配置
        private static FlowersConfig GetConfig()
        {
            return JsonConfig.GetJson<FlowersConfig>("Config/activity.flowers.json");
        }


        /// <summary>
        /// 统计是否连续获得养分
        /// </summary>
        /// <param name="item"></param>
        /// <param name="date"></param>
        /// <param name="endDate"></param>
        private void MemberDataStatics(TotalChanceModel item, DateTime date, DateTime endDate)
        {
            try
            {
                int count = 0;
                DateTime changeDate = date;
                var mongoConn = new ActivityRepository(Config.ActivityConfig.DbName, Config.ActivityConfig.MongoHost);
                for (DateTime i = date; i < endDate; i = i.AddDays(1).Date)
                {
                    //是否投资获得养分
                    changeDate = i.AddDays(1).Date;
                    var isContinuity = mongoConn.Query<RecordModel>(it => it.Key == Key && it.MemberId == item.MemberId && it.Date >= i && it.Date < changeDate).Any();
                    if (isContinuity)
                    {
                        count++;
                        continue;
                    }

                    //是否邀请好友注册 好友助力获得养分
                    isContinuity = mongoConn.Query<FriendTotalChanceModel>(
                        it => it.Key == Key && it.FriendId == item.MemberId && it.CreateTime >= i && it.CreateTime < changeDate && it.MemberId != item.MemberId && it.Type != 4 && it.Type != 1 && it.HelpCount > 0).Any();

                    if (isContinuity)
                    {
                        count++;
                    }
                    else
                        count = 0;
                }

                var hasData = mongoConn.Query<FlowersModel>(it => it.Key == Key && it.MemberId == item.MemberId).Any();
                if (!hasData)
                {
                    mongoConn.Add(new FlowersModel
                    {
                        Key = Key,
                        MemberId = item.MemberId,
                        Phone = item.Remark,
                        Count = count,
                        CreateTime = DateTime.Now
                    });
                }

                var rewardData = mongoConn.Query<FriendTotalChanceModel>(it => it.Key == Key && it.FriendId == item.MemberId && it.Type == 4).Any();
                if (!rewardData && count >= 10)
                {
                    mongoConn.Add(new FriendTotalChanceModel
                    {
                        MemberId = 0,
                        Key = Key,
                        Phone = 0,
                        FriendId = item.MemberId,
                        FriendPhone = long.Parse(item.Remark),
                        HelpCount = 10,
                        Type = 4,
                        Remark = "系统赠送",
                        LastUpdateTime = DateTime.Now,
                        CreateTime = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"MemberDataStatics Error {ex} ");
            }
        }
    }
}
