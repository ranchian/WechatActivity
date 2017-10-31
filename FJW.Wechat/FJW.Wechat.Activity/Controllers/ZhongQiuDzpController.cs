using System;
using System.Linq;
using System.Web.Mvc;

using FJW.Unit;

using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Data.Model.RDBS;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 中秋大转盘活动
    /// </summary>
    [CrossDomainFilter]
    public class ZhongQiuDzpController : ActivityController
    {
        private const string Key = "zhongqiudzp";
        private readonly SqlDataRepository _respoRepository;
        private readonly ZhongQiuDzpConfig _config;

        //转盘奖励金额
        private static readonly long[] LuckMoneyLis = { 50, 100, 200, 300, 400, 500 };

        #region 转盘奖励
        private static readonly string[] zhongqiudzpLuck = {
         "A4","A3","A4","A3","A4","A3","A4","A3","A4","A3","A4","A3","A4","A3","A2","A3","A2","A3","A4","A3",
         "A2","A3","A4","A3","A2","A3","A2","A3","A4","A3","A2","A3","A4","A3","A2","A3","A2","A3","A4","A3",
         "A4","A3","A4","A3","A2","A3","A4","A2","A2","A2","A4","A3","A2","A3","A2","A2","A4","A2","A2","A3",
         "A4","A3","A1","A3","A1","A3","A6","A3","A2","A3","A6","A3","A4","A3","A1","A2","A5","A3","A5","A2",
         "A5","A2","A1","A3","A5","A2","A2","A2","A4","A3","A5","A3","A2","A2","A2","A2","A2","A2","A1","A3"
        };
        #endregion

        public ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }

        public ZhongQiuDzpController()
        {
            _respoRepository = new SqlDataRepository(SqlConnectString);
            _config = ZhongQiuDzpConfig.GetConfig();
        }

        /// <summary>
        /// 验证 
        /// </summary>
        /// <returns></returns>
        private ResponseModel Validate()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "用户未登录!" };
            }

            var now = DateTime.Now;
            if (now < _config.StartTime || now > _config.EndTime)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "活动未开始或已过期" };

            return new ResponseModel { ErrorCode = ErrorCode.None, Data = "" };
        }

        /// <summary>
        /// 计算数量
        /// </summary>
        private void Summary(TotalChanceModel total, long userId)
        {
            try
            {
                var userData =
                    GetRepository()
                        .Query<TotalChanceModel>(it => it.MemberId == userId && it.Key == Key)
                        .FirstOrDefault();
                if (userData != null)
                    total = userData;

                var shares = _respoRepository.GetProductTypeShares(userId, _config.StartTime, _config.EndTime).ToList();


                long count = 0;
                foreach (var r in shares)
                {
                    long add;
                    SwitchMethod(r, out add);
                    count += add > 0 ? add : 0;
                }

                //用户投资获得抽奖次数（与九月大转盘活动共享次数）
                var septemberDzpTal = GetRepository().Query<TotalChanceModel>(it => it.Key == "septemberdzp" && it.MemberId == userId).FirstOrDefault();
                var totalCnt = 0;
                if (septemberDzpTal != null)
                    totalCnt = (int)((count / 12 - septemberDzpTal.Used * 1000) / 2000);
                else
                    totalCnt = (int)count / 12 / 2000;


                //使用次数
                var totalChanceModel = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userId).FirstOrDefault();

                if (totalChanceModel != null)
                {
                    var useCount = totalChanceModel.Used;
                    //使用次数
                    total.Used = useCount;
                }

                total.Key = Key;
                total.MemberId = userId;
                total.Total = totalCnt;
                if (userData != null)
                {
                    GetRepository().Update(userData);
                }
                else
                {
                    if (userId == UserInfo.Id && UserInfo.Id != 0)
                    {
                        total.Remark = UserInfo.Phone;
                        GetRepository().Add(total);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Summary :" + ex);
            }
        }

        /// <summary>
        /// 查询可以使用次数
        /// </summary>
        /// <returns></returns>
        private TotalChanceModel SelectCount(out int canUse)
        {
            //统计次数
            Summary(new TotalChanceModel(), UserInfo.Id);

            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();

            #region Test Code
            if (userChance != null && (UserInfo.Phone == "15961956476" || UserInfo.Phone == "18761216965"))
            {
                userChance.Total = 100000;
            }
            #endregion

            canUse = 0;
            //Used 自己使用
            if (userChance != null)
                canUse = userChance.Total - userChance.Used;

            return userChance;
        }

        /// <summary>
        /// 使用机会
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Give()
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            int canUse;
            var memberData = SelectCount(out canUse);
            if (canUse <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "快去投资，赢取现金吧~" });

            long random = 0;
            var useCount = 0;
            string[] luck = { };

            //Redis 取值从1开始
            random = RedisManager.GetIncrement("activity :" + Key + "_Luck") - 1;
            luck = zhongqiudzpLuck;
            useCount = 1;


            long money = 0;
            long s = random % 100;
            var luckNum = luck[s];
            long luckMoney = 0L;

            var objId = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));

            //发放奖励
            switch (luckNum)
            {
                case "A1":
                    luckMoney = LuckMoneyLis[0];
                    break;
                case "A2":
                    luckMoney = LuckMoneyLis[1];
                    break;
                case "A3":
                    luckMoney = LuckMoneyLis[2];
                    break;
                case "A4":
                    luckMoney = LuckMoneyLis[3];
                    break;
                case "A5":
                    luckMoney = LuckMoneyLis[4];
                    break;
                case "A6":
                    luckMoney = LuckMoneyLis[5];
                    break;
            }

            //发放现金奖励
            new MemberRepository(SqlConnectString).GiveMoney(UserInfo.Id, luckMoney, _config.RewardId, objId);

            GetRepository().Add(new LuckdrawModel
            {
                MemberId = UserInfo.Id,
                Phone = UserInfo.Phone,
                Prize = -1,
                Key = Key,
                Type = luckNum,
                CouponRes = "",
                Name = luckMoney.ToString(),
                Status = 1,
                Remark = "中秋大转盘赢现金-" + luckMoney + "元现金"
            });

            memberData.Used += useCount;
            memberData.Score += 1;
            GetRepository().Update(memberData);

            SelectCount(out canUse);
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { LuckName = luckMoney + "元现金", LuckNum = luckNum, Count = canUse <= 0 ? 0 : canUse } });
        }

        /// <summary>
        /// 统计年化公共方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="add"></param>
        private void SwitchMethod(ProductTypeSumShare item, out long add)
        {
            switch (item.ProductTypeId)
            {
                case 5:
                    item.Title = "房金月宝";
                    add = (int)item.BuyShares * 1;
                    break;
                case 6:
                    item.Title = "房金季宝";
                    add = (int)item.BuyShares * 3;
                    break;
                case 7:
                    item.Title = "房金双季宝";
                    add = (int)item.BuyShares * 6;
                    break;
                case 8:
                    item.Title = "房金年宝";
                    add = (int)item.BuyShares * 12;
                    break;
                default:
                    add = 0;
                    break;
            }
        }

        /// <summary>
        /// 我的奖励
        /// </summary>
        /// <returns></returns>
        public ActionResult RewardList()
        {
            try
            {
                var validateRes = Validate();
                if (validateRes.ErrorCode != ErrorCode.None)
                    return Json(validateRes);

                int canUse;
                SelectCount(out canUse);

                var resultData = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).OrderByDescending(it => it.CreateTime).Select(it => new
                {
                    it.Type,
                    it.Name,
                    Date = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse <= 0 ? 0 : canUse, RewardList = resultData } });

            }
            catch (Exception ex)
            {
                Logger.Error($"ZhongQiuDzp RewardList：{ex}");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "服务繁忙~" });
            }
        }

        /// <summary>
        /// 奖励
        /// </summary>
        /// <returns></returns>
        public ActionResult LuckReward()
        {
            try
            {
                int canUse;
                SelectCount(out canUse);

                var resultData = GetRepository().Query<LuckdrawModel>(it => it.Key == Key).OrderByDescending(it => it.CreateTime).Take(100).Select(it => new
                {
                    Phone = it.Phone.Substring(0, 3) + "****" + it.Phone.Substring(7, 4),
                    it.Type,
                    it.Name,
                    Date = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { IsLogin = UserInfo.Id > 0, Count = canUse <= 0 ? 0 : canUse, RewardList = resultData } });
            }
            catch (Exception ex)
            {
                Logger.Error($"ZhongQiuDzp RewardList：{ex}");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "服务繁忙~" });
            }
        }
    }
}

