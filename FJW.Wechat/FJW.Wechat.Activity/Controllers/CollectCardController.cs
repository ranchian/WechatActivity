using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.RDBS;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;

namespace FJW.Wechat.Activity.Controllers
{
    public class CollectCardController : ActivityController
    {

        private static readonly Random LuckRandom = new Random();

        private const string GameKey = "collectcard";

        private CollectCardModel GetConfig()
        {
            return JsonConfig.GetJson<CollectCardModel>("config/activity.collectcard.json");
        }

        /// <summary>
        /// 获取机会
        /// </summary>
        /// <returns></returns>
        public ActionResult Chances()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var config = GetConfig();

            var total = GetChances(userId, config.StartTime, config.EndTime);

            var dict = new Dictionary<string, int>();
            dict["chance"] = total.Total;
            dict["used"] = total.Used;
            dict["notUsed"] = total.NotUsed;
            return Json(new ResponseModel { Data = dict });
       
        }

        /// <summary>
        /// 获取统计次数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private TotalChanceModel GetChances(long userId, DateTime startTime, DateTime endTime)
        {
            //
            var activeRepository = new ActivityRepository(DbName, MongoHost);

            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            if (total == null)
            {
                var all = Data(userId, startTime, endTime);
                //总次数
                var chances = SumChance(all);

                //create
                total = new TotalChanceModel
                {
                    Key = GameKey,
                    MemberId = userId,
                    Total = chances,
                    Used = 0,
                    NotUsed = 0,
                    LastStatisticsTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    Remark = all.ToJson()
                };
                activeRepository.Add(total);
            }
            else
            {
                //统计间隔 > 30s 
                if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 30)
                {
                    var all = Data(userId, startTime, endTime);
                    //总次数
                    var chances = SumChance(all);
                    //if changed then save total
                    if (chances != total.Total)
                    {
                        total.Total = chances;
                        total.NotUsed = chances - total.NotUsed;
                        total.Remark = all.ToJson();
                        total.LastStatisticsTime = DateTime.Now;
                        activeRepository.Update(total);
                    }
                }
            }
            return total;
        }


        public ProductTypeSumShare[] Data(long memberId , DateTime startTime, DateTime endTime)
        {
            var info = new MemberRepository( SqlConnectString ).GetBuySummary(memberId, startTime, endTime);
            return info.ToArray();
        }



        private int SumChance(ProductTypeSumShare[] rows)
        {
            int count = 0;
            foreach (var r in rows)
            {
                switch (r.ProductTypeId)
                {
                    case 5:
                        count += (int) r.Shares/1200;
                        break;
                    case 6:
                        count += (int)r.Shares / 400;
                        break;
                    case 7:
                        count += (int)r.Shares / 200;
                        break;
                    case 8:
                        count += (int)r.Shares / 100;
                        break;
                }
            }
            return count;
        }



        /// <summary>
        /// 兑换
        /// </summary>
        /// <returns></returns>
        public ActionResult Exchange()
        {
            return Json(null);
        }

        /// <summary>
        /// 翻开
        /// </summary>
        /// <returns></returns>
        public ActionResult Accept()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }

            var config = GetConfig();

            var total = GetChances(userId, config.StartTime, config.EndTime);
           


            if (total.Total > total.Used)
            {
                var activeRepository = new ActivityRepository(DbName, MongoHost);
                var prizes = total.Prizes.Deserialize<List<CardPrize>>() ?? new List<CardPrize>();
                //
                var card = LuckDraw();
                //

                long activityId;
                var couponId = LuckCoupon(out activityId);
                var reuslt = CardCouponApi.UserGrant(userId, activityId, couponId);
                var luckdraw = new LuckdrawModel
                {
                    MemberId = userId,
                    Key = GameKey,
                    Remark = reuslt.Data,
                    Prize = (int)couponId,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };

                activeRepository.Add(luckdraw);
                
                //
                prizes.Add(new CardPrize
                {
                    Card = card, CouponId = couponId
                });
                //
                total.Used++;
                total.NotUsed = total.Total - total.Used;
                total.Prizes = prizes.ToJson();
                total.LastUpdateTime = DateTime.Now;
                activeRepository.Update(total);
                


            }
            return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "您没有抽奖机会了" });
            
        }

        /// <summary>
        /// 抽卡
        /// </summary>
        /// <returns></returns>
        private CardType LuckDraw()
        {
            var n = LuckRandom.Next(0, 100);
            if (n < 30)
            {
                return CardType.FeCard;
            }

            if (n < 30 + 20)
            {
                return CardType.AgCard;
            }

            if (n < 30 + 20 + 30 )
            {
                return CardType.AuCard;
            }

            if (n < 30 + 20 + 30 + 9)
            {
                return CardType.PtCard;
            }
            return CardType.FjCard;
        }

        /// <summary>
        /// 抽券
        /// </summary>
        /// <returns></returns>
        private long LuckCoupon(out long activityId)
        {
            var config = GetConfig();
            activityId = config.ActivityId;
            var n = LuckRandom.Next(0, 100);
            if ( n < 3)
            {
                //4元现金券
                return config.CashCardA;
            }

            if (n < 3 + 5)
            {
                //5元现金券
                return config.CashCardB;
            }

            if (n < 3 + 5 + 5)
            {
                //8元现金券
                return config.CashCardC;
            }

            if (n < 3 + 5 + 5 + 12)
            {
                //10元现金券
                return config.CashCardD;
            }

            if (n < 3 + 5 + 5 + 12 + 30)
            {
                //1%加息券
                return config.RateCardA;
            }

            if (n < 3 + 5 + 5 + 12 + 30 + 20)
            {
                //1.5%加息券
                return config.RateCardB;
            }

            if (n < 3 + 5 + 5 + 12 + 30 + 20 + 15)
            {
                //2%加息券
                return config.RateCardC;
            }

            //2.5%加息券
            return config.RateCardD;

        }
    }


    /// <summary>
    /// 卡片类型
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// 房金卡
        /// </summary>
        FjCard = 1,
        
        /// <summary>
        /// 铂金卡
        /// </summary>
        PtCard ,

        /// <summary>
        /// 黄金卡
        /// </summary>
        AuCard,

        /// <summary>
        /// 白银卡
        /// </summary>
        AgCard,

        /// <summary>
        ///  青铜
        /// </summary>
        CuCard,

        /// <summary>
        /// 黑铁
        /// </summary>
        FeCard
    }

    /// <summary>
    /// 奖品
    /// </summary>
    public class CardPrize
    {
        /// <summary>
        /// 类型
        /// </summary>
        public CardType Card { get; set; }

        /// <summary>
        /// 卡券
        /// </summary>
        public long CouponId { get; set; }

        /// <summary>
        /// 是否已经使用
        /// </summary>
        public bool Used { get; set; }
    }
}
