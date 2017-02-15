using System;
using System.Collections.Generic;
using System.Linq;

using System.Web.Mvc;
using FJW.SDK2Api;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data.Model.RDBS;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 二月红
    /// </summary>
    [CrossDomainFilter]
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
            var data = new SqlDataRepository(SqlConnectString).ProductBuyRanking(config.StartTime, config.EndTime).ToList();
            AppendSequnce(data, 5);
            AppendSequnce(data, 6);
            AppendSequnce(data, 7);
            AppendSequnce(data, 8);
            foreach (var d in data)
            {
                d.Phone = StringHelper.CoverPhone(d.Phone);
            }
            
            return Json(new ResponseModel
            {
                Data = data.OrderBy(it=>it.Id).ToArray()
            });
        }

        [HttpPost]
        public ActionResult Exchange(int type)
        {
            if (type < 1 || type > 4)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "无效的卡券类型" });
            }
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var userId = UserInfo.Id;

            var config = GetConfig();
            var channel = new SqlDataRepository(SqlConnectString).GetMemberChennel(userId);
            if (channel?.Channel != null && channel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && channel.CreateTime > config.StartTime)
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您无法参与这次活动：WQWLCPS" });
            }
            if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您无法参与这次活动：特殊的邀请人" });
            }
            //
            var activeRepository = new ActivityRepository(DbName, MongoHost);

            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            var isNew = false;
            if (total == null)
            {
                total = new TotalChanceModel();
                total.Used = 0;
                total.Key = GameKey;
                total.MemberId = userId;
                total.NotUsed = 3;
                total.Total = 3;

                isNew = true;
            }
            else if (total.NotUsed < 1)
            {

                return Json(new ResponseModel() { ErrorCode = ErrorCode.Other, Message = "你已经领取了3个红包" });
            }
            string name;
            
            long couponId;

            switch (type)
            {
                case 1:
                    name = "2.2%";
                    couponId = config.RateCouponA;
                    break;

                case 2:
                    name = "3.6%";
                    couponId = config.RateCouponB;
                    break;

                case 3:
                    name = "5.8%";
                    couponId = config.RateCouponC;
                    break;
                case 4:
                    name = "8.8%";
                    couponId = config.RateCouponD;
                    break;

                default:
                    return Json(new ResponseModel {ErrorCode = ErrorCode.NotVerified, Message = "无效的卡券类型"});
            }
            var result = GiveCoupon(userId, couponId, config.ActivityId);
            var luckdraw = new LuckdrawModel
            {
                MemberId = userId,
                Key = GameKey,
                Remark = result,
                Name = name,
                Sequnce = 0,
                Prize = config.RateCouponA,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            activeRepository.Add(luckdraw);

            total.Used += 1;
            total.NotUsed = total.Total - total.Used;

            total.LastUpdateTime = DateTime.Now;
            if (isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Update(total);
            }
            return Json(new ResponseModel());
        }


        private string GiveCoupon(long userId, long couponId, long activityId)
        {
           return (CardCouponApi.UserGrant(userId, activityId, couponId) ?? new ApiResponse()).Data;
        }

        /// <summary>
        /// 游戏结果
        /// </summary>
        /// <returns></returns>
        public ActionResult Result()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var rows = new ActivityRepository(DbName, MongoHost).Query<LuckdrawModel>(it => it.Key == GameKey && it.MemberId == userId).ToList();
            var data = new ResponseModel
            {
                Data = rows.Select(it => new { time = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), type = it.Type, name = it.Name }).ToArray()
            };
            return Json(data);
        }

        public static void AppendSequnce(List<RankingRow> rows, long id )
        {
            if (!rows.Exists(it=>it.Id == id))
            {
                rows.Add(new RankingRow { Id = id, Phone = string.Empty, Shares = 0});
            }
        }

    }
}
