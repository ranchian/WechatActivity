using System;
using System.Linq;
using System.Web.Mvc;

using FJW.Unit;

using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Data.Model.RDBS;

using FJW.SDK2Api.CardCoupon;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 九月大转盘赚大闸蟹活动
    /// </summary>
    [CrossDomainFilter]
    public class SeptemberDzpController : ActivityController
    {
        private const string Key = "septemberdzp";
        private readonly SqlDataRepository _respoRepository;
        private readonly SeptemberDzpConfig _config;

        private static string[] luckNameArr = { "3%加息券", "10元现金", "388元阳澄湖大闸蟹礼券", "528元阳澄湖大闸蟹礼券", "768元阳澄湖大闸蟹礼券", "1158元阳澄湖大闸蟹礼券" };
        #region 转盘奖励
        private static readonly string[] septemberdzpLuck = {
            "A2","A1","A2","A1","A2","A1","A2","A1","A2","A1","A1","A1","A2","A1","A2","A1","A2","A1","A2","A1",
            "A2","A1","A1","A1","A2","A1","A1","A1","A2","A1","A1","A1","A4","A1","A1","A1","A1","A1","A1","A1",
            "A1","A1","A1","A1","A1","A1","A1","A1","A3","A1","A3","A1","A2","A1","A1","A1","A4","A1","A2","A1",
            "A1","A1","A1","A1","A2","A1","A3","A1","A1","A1","A3","A1","A1","A1","A1","A1","A3","A1","A1","A1",
            "A2","A1","A6","A1","A5","A1","A4","A1","A1","A1","A1","A1","A3","A1","A3","A1","A3","A1","A5","A1"
        };
        #endregion

        public ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }

        public SeptemberDzpController()
        {
            _respoRepository = new SqlDataRepository(SqlConnectString);
            _config = SeptemberDzpConfig.GetConfig();
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

                //用户投资获得抽奖次数
                var totalCnt = (int)count / 12 / 1000;

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
                Logger.Info($"userData :{userData.ToJson()}, ProductShare:{shares.FirstOrDefault().ToJson()}");
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
                userChance.Total = 100000 + userChance.Score;
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
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "快去投资，赢取大闸蟹吧~" });

            long random = 0;
            var useCount = 0;
            string[] luck = { };

            //Redis 取值从1开始
            random = RedisManager.GetIncrement("activity :" + Key + "_SeptemberDzp") - 1;
            luck = septemberdzpLuck;
            useCount = 1;


            long money = 0;
            long s = random % 100;
            var luckNum = luck[s];
            var luckName = "";

            var objId = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));

            //发放奖励
            switch (luckNum)
            {
                case "A1":
                    luckName = luckNameArr[0];
                    break;
                case "A2":
                    luckName = luckNameArr[1];
                    new MemberRepository(SqlConnectString).GiveMoney(UserInfo.Id, 10, _config.RewardId, objId);
                    break;
                case "A3":
                    luckName = luckNameArr[2];
                    break;
                case "A4":
                    luckName = luckNameArr[3];
                    break;
                case "A5":
                    luckName = luckNameArr[4];
                    break;
                case "A6":
                    luckName = luckNameArr[5];
                    break;
            }

            GetRepository().Add(new LuckdrawModel
            {
                MemberId = UserInfo.Id,
                Phone = UserInfo.Phone,
                Prize = -1,
                Key = Key,
                Type = luckNum,
                CouponRes = "",
                Name = luckName,
                Status = luckNum == "A1" ? 0 : 1,
                Remark = "九月大转盘赚大闸蟹活动-" + luckName
            });

            memberData.Used += useCount;
            memberData.Score += 1;
            GetRepository().Update(memberData);

            SelectCount(out canUse);
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { LuckName = luckName, LuckNum = luckNum, Count = canUse <= 0 ? 0 : canUse } });
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
        /// 选择卡券
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult GiveCoupon(int type = 0)
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            var luckCoupon = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id && it.Status == 0 && it.Type == "A1").FirstOrDefault();
            if (luckCoupon == null)
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "当前无可领取卡券" });

            var couponName = "";
            var CouponId = 0L;
            switch (type)
            {
                case 1:
                    CardCouponApi.UserGrant(UserInfo.Id, _config.ActivityId, _config.RateCouponA);
                    CouponId = _config.RateCouponA;
                    couponName = "3%加息券，限房金季宝";
                    break;
                case 2:
                    CardCouponApi.UserGrant(UserInfo.Id, _config.ActivityId, _config.RateCouponB);
                    couponName = "3%加息券，限房金双季宝";
                    CouponId = _config.RateCouponB;
                    break;
                case 3:
                    CardCouponApi.UserGrant(UserInfo.Id, _config.ActivityId, _config.RateCouponC);
                    couponName = "3%加息券，限房金年宝";
                    CouponId = _config.RateCouponC;
                    break;
                default:
                    return Json(new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "请选择卡券哟~" });
            }

            luckCoupon.Status = 1;
            luckCoupon.Prize = CouponId;
            luckCoupon.Remark = "九月大转盘赚大闸蟹活动-" + couponName;
            luckCoupon.LastUpdateTime = DateTime.Now;
            GetRepository().Update(luckCoupon);
            return Json(new ResponseModel { ErrorCode = ErrorCode.None });
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
                Logger.Error($"SeptemberDzp RewardList：{ex}");
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

                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse <= 0 ? 0 : canUse, RewardList = resultData } });
            }
            catch (Exception ex)
            {
                Logger.Error($"SeptemberDzp RewardList：{ex}");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "服务繁忙~" });
            }
        }
    }
}

