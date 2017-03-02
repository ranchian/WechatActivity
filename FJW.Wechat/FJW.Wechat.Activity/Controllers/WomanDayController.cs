using System;
using System.Linq;

using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Unit;

namespace FJW.Wechat.Activity.Controllers
{
    [CrossDomainFilter]
    public class WomanDayController : ActivityController
    {
        private const string GameKey = "womanday";

        private static readonly char[] Squence ={ 'F', 'C', 'F', 'C', 'H', 'C', 'F', 'C', 'F', 'C', 'E', 'C', 'F', 'C', 'F', 'B', 'E', 'C', 'H', 'C', 'H', 'C', 'H', 'C',
            'I', 'B', 'G', 'C', 'K', 'B', 'E', 'C', 'F', 'B', 'F', 'C', 'D', 'C', 'D', 'B', 'D', 'B', 'E', 'B', 'R', 'C', 'G', 'C', 'D', 'C', 'I', 'B', 'H', 'B', 'N',
            'B', 'G', 'C', 'E', 'A', 'D', 'C', 'O', 'B', 'N', 'C', 'I', 'B', 'I', 'B', 'P', 'A', 'E', 'B', 'E', 'B', 'O', 'C', 'J', 'A', 'M', 'B', 'G', 'A', 'D', 'A',
            'K', 'A', 'L', 'A', 'J', 'D', 'Q', 'C', 'I', 'C', 'L', 'D', 'E', 'A' };

        private static WomanDayConfig GetConfig()
        {
            return JsonConfig.GetJson<WomanDayConfig>("config/activity.womanday.json");
        }

        public ActionResult Accept()
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var userId = UserInfo.Id;
            var config = GetConfig();
            var channel = new SqlDataRepository(SqlConnectString).GetMemberChennel(userId);
            if (channel?.Channel != null && channel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && channel.CreateTime > config.StartTime)
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您属特殊渠道注册用户,无法参与此活动！" });
            }
            if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId, config.StartTime))
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您属特殊渠道注册用户,无法参与此活动！" });
            }
            
            var activeRepository = new ActivityRepository(DbName, MongoHost);

            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            var isNew = false;
            if (total == null)
            {
                total = new TotalChanceModel
                {
                    Used = 0,
                    Key = GameKey,
                    MemberId = userId,
                    NotUsed = 3,
                    Total = 3
                };

                isNew = true;
            }

            if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 30)
            {
                var cunt = new SqlDataRepository(SqlConnectString).ProductBuyCount(userId, config.StartTime, config.EndTime);
                total.Total = cunt + 3;
                total.NotUsed = total.Total - total.Used;
                total.LastUpdateTime = DateTime.Now;
            }
            
            if (total.NotUsed < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "机会已用完，快快去投资，每投资一笔，可再摇一次哟~" });
            }
            string name;
            long prize;
            var result = GiveCoupon( total.Used, userId, config, out name, out prize);
            var luckdraw = new LuckdrawModel
            {
                MemberId = userId,
                Key = GameKey,
                Remark = result,
                Name = name,
                Sequnce = 0,
                Prize = prize,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            activeRepository.Add(luckdraw);

            total.Used += 1;
            total.NotUsed = total.Total - total.Used;

            total.LastUpdateTime = DateTime.Now;

            if (!isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Add(total);
            }
            return Json(new ResponseModel{ Data = name});
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
                Data = rows.Select(it => new {name = it.Name }).ToArray()
            };
            return Json(data);
        }

        /// <summary>
        /// 发卡券
        /// </summary>
        /// <param name="usedCount"></param>
        /// <param name="userId"></param>
        /// <param name="config"></param>
        /// <param name="name"></param>
        /// <param name="prize"></param>
        /// <returns></returns>
        private string GiveCoupon(int usedCount, long userId, WomanDayConfig config, out string name, out long prize)
        {
            //
            //Squence
            var n = RedisManager.GetIncrement("activity:" + GameKey);
            var s = n % 100;
            var c = Squence[s];

            if (usedCount < 3 && c =='P' || c == 'Q' || c == 'R')
            {
                c = 'A';
            }
            var activityId = config.ActivityId;
            long couponId;

            switch (c)
            {
                case 'A':
                    name = "1.80";
                    couponId = config.A;
                    break;

                case 'B':
                    name = "2";
                    couponId = config.B;
                    break;

                case 'C':
                    name = "2.80";
                    couponId = config.C;
                    break;

                case 'D':
                    name = "3.80";
                    couponId = config.D;
                    break;

                case 'E':
                    name = "3.80";
                    couponId = config.E;
                    break;

                case 'F':
                    name = "3.80";
                    couponId = config.F;
                    break;

                case 'G':
                    name = "5.80";
                    couponId = config.G;
                    break;

                case 'H':
                    name = "5.80";
                    couponId = config.H;
                    break;

                case 'I':
                    name = "5.80";
                    couponId = config.I;
                    break;

                case 'J':
                    name = "8.80";
                    couponId = config.J;
                    break;

                case 'K':
                    name = "8.80";
                    couponId = config.K;
                    break;

                case 'L':
                    name = "8.80";
                    couponId = config.L;
                    break;

                case 'M':
                    name = "10.00";
                    couponId = config.M;
                    break;

                case 'N':
                    name = "10.00";
                    couponId = config.N;
                    break;

                case 'O':
                    name = "10.00";
                    couponId = config.O;
                    break;

                case 'P':
                    name = "38.00";
                    couponId = config.P;
                    break;

                case 'Q':
                    name = "38.00";
                    couponId = config.Q;
                    break;

                case 'R':
                    name = "38.00";
                    couponId = config.R;
                    break;

                default:
                    couponId = 0;
                    name = "";
                    break;

            }
            prize = couponId;
            var reuslt = CardCouponApi.UserGrant(userId, activityId, couponId);
            return reuslt.Data;
        }
    }
}
