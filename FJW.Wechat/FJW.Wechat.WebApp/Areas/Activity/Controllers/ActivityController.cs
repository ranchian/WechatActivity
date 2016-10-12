using System;
using System.Web.Configuration;

using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Base;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    /// <summary>
    /// 活动
    /// </summary>
    public abstract class ActivityController : WController
    {
        protected readonly string MongoHost;
        protected readonly string DbName;
        protected readonly string SqlConnectString;

        protected ActivityController()
        {
            MongoHost = Config.ActivityConfig.MongoHost;
            DbName = Config.ActivityConfig.DbName;
            SqlConnectString = WebConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }

        private const string SelfGameKey = "WAC_SELF";

        protected Activity ActivityModel { get; set; }

        /// <summary>
        /// 获取自己玩的 记录Id
        /// </summary>
        /// <returns></returns>
        protected string GetSelfGameRecordId()
        {
            var id = Session[SelfGameKey];
            if (id != null)
            {
                return id.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 临时保存自己玩的 记录Id
        /// </summary>
        /// <param name="id"></param>
        protected void SetSelfGameRecordId(string id)
        {
            HttpContext.Session[SelfGameKey] = id;
        }

        /// <summary>
        /// 获取帮别人玩的 记录Id
        /// </summary>
        /// <returns></returns>
        protected string GetHelpGamRecordId(string fid)
        {
            var id = HttpContext.Session[fid];
            if (id != null)
            {
                return id.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 临时保存帮别人玩的 记录Id
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="id"></param>
        protected void SetHelpGamRecordId(string fid, string id)
        {
            HttpContext.Session[fid] = id;
        }

        protected int ActivityState(string key)
        {
            ActivityModel = RedisManager.Get<Activity>("Activity:" + key);
            if (ActivityModel == null)
            {
                var acty = new ActivityRepository(DbName, MongoHost).GetActivity(key);
                if (acty == null)
                {
                    return -2;
                }
                ActivityModel = new Activity
                {
                    Key = acty.Key,
                    RewardType = acty.RewardType,
                    RewardId = acty.RewardId,
                    ProductId = acty.ProductId,
                    MaxValue = acty.MaxValue,
                    StartTime = acty.StartTime,
                    EndTime = acty.EndTime,
                    GameUrl = acty.GameUrl
                };
                RedisManager.Set("Activity:" + key, acty, 30 * 60);
            }
            if (DateTime.Now < ActivityModel.StartTime || DateTime.Now > ActivityModel.EndTime || ActivityModel.GameUrl.IsNullOrEmpty())
            {
                return -1;
            }
            return 0;
        }
        
    }

    public class Activity
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 奖励类型
        /// </summary>
        public int RewardType { get; set; }

        /// <summary>
        /// 奖励
        /// </summary>
        public long RewardId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// 活动开始时间
        /// </summary>

        public DateTime StartTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>

        public DateTime EndTime { get; set; }

        /// <summary>
        /// 游戏地址
        /// </summary>
        public string GameUrl { get; set; }
    }
}