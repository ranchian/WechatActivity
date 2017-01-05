using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.RDBS;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Activity.Models;
using FJW.Wechat.Cache;

namespace FJW.Wechat.Activity.Controllers
{
    [CrossDomainFilter]
    public class CollectCardController : ActivityController
    {

        private static readonly Random LuckRandom = new Random();

        private const string GameKey = "collectcard";

        private static readonly CardType[] CardSqunce = {
CardType.AuCard, CardType.CuCard, CardType.AgCard, CardType.PtCard, CardType.AuCard, CardType.PtCard, CardType.FeCard, CardType.AuCard,CardType.FeCard,CardType.PtCard,
CardType.AuCard, CardType.FjCard,CardType.AuCard,CardType.CuCard,CardType.PtCard,CardType.PtCard,CardType.FeCard,CardType.FeCard,CardType.CuCard,CardType.AgCard,
CardType.AgCard,CardType.AuCard,CardType.AuCard,CardType.CuCard,CardType.CuCard,CardType.AuCard,CardType.AuCard,CardType.AuCard,CardType.AgCard,CardType.FeCard,
CardType.FeCard,CardType.CuCard,CardType.CuCard,CardType.FeCard,CardType.CuCard,CardType.PtCard,CardType.AuCard,CardType.PtCard,CardType.PtCard,CardType.AgCard,CardType.AuCard,
CardType.PtCard,CardType.CuCard,CardType.CuCard,CardType.AuCard,CardType.FeCard,CardType.AgCard,CardType.AuCard,CardType.AgCard,CardType.AuCard,CardType.FeCard,CardType.FeCard,
CardType.AuCard,CardType.FeCard,CardType.AuCard,CardType.AgCard,
CardType.AgCard,CardType.AgCard,CardType.AuCard,CardType.FeCard,CardType.AuCard,CardType.AuCard,CardType.AgCard,CardType.FeCard,CardType.AuCard,CardType.FeCard,CardType.AgCard,
CardType.FeCard,CardType.FeCard,CardType.FeCard,CardType.AgCard,CardType.AuCard,CardType.AuCard,CardType.AuCard,CardType.FeCard,CardType.FeCard,CardType.AuCard,CardType.AgCard,
CardType.AgCard,CardType.FeCard,CardType.AgCard,CardType.FeCard,CardType.AuCard,CardType.FeCard,CardType.AuCard,CardType.AgCard,CardType.FeCard,CardType.AgCard,CardType.AgCard,
CardType.AgCard,CardType.AuCard,CardType.FeCard,CardType.FeCard,CardType.AuCard,CardType.FeCard,CardType.FeCard,CardType.FeCard,CardType.AuCard,CardType.FeCard,CardType.FeCard
        };

        private CollectCardModel GetConfig()
        {
            return JsonConfig.GetJson<CollectCardModel>("config/activity.collectcard.json");
        }

        #region 获取机会

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
            var prizes = total.Prizes.Deserialize<List<CardPrize>>() ?? new List<CardPrize>();

            var dict = new Dictionary<string, object>();
            dict["chance"] = total.Total;
            dict["used"] = total.Used;
            dict["notUsed"] = total.NotUsed;
            var cardPrizes = new int[6];
            cardPrizes[0] = prizes.Count(it => it.Card == CardType.FjCard); //房金卡
            cardPrizes[1] = prizes.Count(it => it.Card == CardType.PtCard); //珀金
            cardPrizes[2] = prizes.Count(it => it.Card == CardType.AuCard); //金
            cardPrizes[3] = prizes.Count(it => it.Card == CardType.AgCard); //银
            cardPrizes[4] = prizes.Count(it => it.Card == CardType.CuCard); //铜
            cardPrizes[5] = prizes.Count(it => it.Card == CardType.FeCard); //铁
            dict["cards"] = cardPrizes;
            //prizes.Where(it => !it.Used)
            //    .GroupBy(it => it.Card)
            //    .Select(it => new {card = it.Key, count = it.Count()});
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
                var channel = new SqlDataRepository(SqlConnectString).GetMemberChennel(userId);
                if (chances > 0 && channel?.Channel != null && channel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && channel.CreateTime > startTime)
                {
                    chances = 0;
                }
                if (chances > 0 && new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
                {
                    chances = 0;
                }
                //create
                total = new TotalChanceModel
                {
                    Key = GameKey,
                    MemberId = userId,
                    Total = chances,
                    Used = 0,
                    NotUsed = chances,
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
                    var channel = new SqlDataRepository(SqlConnectString).GetMemberChennel(userId);
                    if (chances > 0 && channel?.Channel != null && channel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && channel.CreateTime > startTime)
                    {
                        chances = 0;
                    }
                    if (chances > 0 && new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
                    {
                        chances = 0;
                    }
                    //if changed then save total
                    if (chances != total.Total)
                    {
                        total.Total = chances;
                        total.NotUsed = chances - total.Used;
                        total.Remark = all.ToJson();
                        total.LastStatisticsTime = DateTime.Now;
                        activeRepository.Update(total);
                    }
                }
            }
            return total;
        }


        public ProductTypeSumShare[] Data(long memberId, DateTime startTime, DateTime endTime)
        {
            var info = new MemberRepository(SqlConnectString).GetBuySummary(memberId, startTime, endTime);
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
                        count += (int)r.BuyShares / 1200;
                        break;
                    case 6:
                        count += (int)r.BuyShares / 400;
                        break;
                    case 7:
                        count += (int)r.BuyShares / 200;
                        break;
                    case 8:
                        count += (int)r.BuyShares / 100;
                        break;
                }
            }
            return count;
        }

        #endregion

        #region  兑换

        /// <summary>
        /// 兑换
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Exchange(CardExchangeModel model)
        {
            var type = model.Type;
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }

            if (type == 1 && model.Count > 0)
            {
                string msg;
                if (ExchangeAll(model.Count, userId, out msg))
                {
                    return Json(new ResponseModel());
                }
                return Json(new ResponseModel(ErrorCode.Other) { Message = msg });
            }

            if (type == 2 && model.Cards != null && model.Cards.Length > 0)
            {
                string msg;
                if (ExchangeSame(userId, model.Cards, out msg))
                {
                    return Json(new ResponseModel());
                }
                return Json(new ResponseModel(ErrorCode.Other) { Message = msg });
            }
            return Json(new ResponseModel(ErrorCode.NotVerified) { Message = "无效的兑换请求" });
        }

        /// <summary>
        /// 兑换全类型
        /// </summary>
        /// <param name="count"></param>
        /// <param name="userId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ExchangeAll(int count, long userId, out string msg)
        {
            msg = "";
            var config = GetConfig();

            var total = GetChances(userId, config.StartTime, config.EndTime);
            var prizes = total.Prizes.Deserialize<List<CardPrize>>() ?? new List<CardPrize>();
            var activityRepository = new ActivityRepository(DbName, MongoHost);
            var memberRepository = new MemberRepository(SqlConnectString);

            var fjs = TakeCount(prizes, CardType.FjCard, count).ToList();

            var pts = TakeCount(prizes, CardType.PtCard, count).ToList();

            var aus = TakeCount(prizes, CardType.AuCard, count).ToList();

            var ags = TakeCount(prizes, CardType.AgCard, count).ToList();

            var cus = TakeCount(prizes, CardType.CuCard, count).ToList();

            var fes = TakeCount(prizes, CardType.FeCard, count).ToList();

            if (fjs.Count < count || pts.Count < count || aus.Count < count || ags.Count < count || cus.Count < count || fes.Count < count)
            {
                msg = "福卡的种类不全";
                return false;
            }
            SetUsed(fjs);
            SetUsed(pts);
            SetUsed(aus);
            SetUsed(ags);
            SetUsed(cus);
            SetUsed(fes);

            var money = 318 * count;
            //发送奖励
            var date = DateTime.Now;
            //var objId = 318 * 1000000 + date.Year * 10000 + date.Month * 100 + date.Day;
            var objId = long.Parse(date.ToString("yyyyMMddHHmmssffff"));
            memberRepository.GiveMoney(userId, money, config.RewardId, objId);
            var luckModel = new LuckdrawModel
            {
                Key = GameKey,
                MemberId = userId,
                Money = money,
                Prize = (int)config.RewardId,
                Phone = UserInfo.Phone,
                Name = $"{money}元",
                Type = "现金红包",
                CreateTime = DateTime.Now
            };
            total.Prizes = prizes.Where(it => !it.Used).ToJson();
            activityRepository.Add(luckModel);
            activityRepository.Update(total);

            return true;
        }

        /// <summary>
        /// 兑换同类型
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tuples"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ExchangeSame(long userId, CardTuple[] tuples, out string msg)
        {
            msg = "";
            var config = GetConfig();

            var total = GetChances(userId, config.StartTime, config.EndTime);
            var prizes = total.Prizes.Deserialize<List<CardPrize>>() ?? new List<CardPrize>();
            var activityRepository = new ActivityRepository(DbName, MongoHost);
            var memberRepository = new MemberRepository(SqlConnectString);
            var cards = prizes.Where(it => !it.Used).ToList();
            var usedCards = new List<CardPrize>();
            var money = 0;
            foreach (var tuple in tuples)
            {
                var t = tuple.Type;
                var cnt = 5 * tuple.Count;
                var tmps = TakeCount(cards, t, cnt);
                if (tmps.Count < cnt)
                {
                    msg = "福卡数量不足";
                    return false;
                }
                money += tuple.Count * 8;
                usedCards.AddRange(tmps);
            }

            SetUsed(usedCards);
            var date = DateTime.Now;
            var objId = long.Parse(date.ToString("yyyyMMddHHmmssffff"));
            //发送奖励
            memberRepository.GiveMoney(userId, money, config.RewardId, objId);
            var luckModel = new LuckdrawModel
            {
                Key = GameKey,
                MemberId = userId,
                Money = money,
                Prize = (int)config.RewardId,
                Phone = UserInfo.Phone,
                Name = $"{money}元",
                Type = "现金红包",
                CreateTime = DateTime.Now
            };
            total.Prizes = cards.Where(it => !it.Used).ToJson();
            activityRepository.Add(luckModel);
            activityRepository.Update(total);
            return true;

        }


        private void SetUsed(IEnumerable<CardPrize> cards)
        {
            foreach (var c in cards)
            {
                c.Used = true;
            }
        }

        private List<CardPrize> TakeCount(IEnumerable<CardPrize> source, CardType t, int count)
        {
            return source.Where(it => !it.Used && it.Card == t).Take(count).ToList();
        }

        #endregion

        #region 抽奖

        /// <summary>
        /// 翻开
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Accept(bool isAll = false)
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }

            var config = GetConfig();

            var total = GetChances(userId, config.StartTime, config.EndTime);


            if (total.Total <= total.Used)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "您没有机会了" });

            var channel = new SqlDataRepository(SqlConnectString).GetMemberChennel(userId);
            if (channel != null && channel.Channel != null && channel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && channel.CreateTime > config.StartTime)
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您无法参与这次活动：WQWLCPS" });
            }
            if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您无法参与这次活动：特殊的邀请人" });
            }

            var count = 1;
            if (isAll)
            {
                count = total.Total - total.Used;
            }
            var activeRepository = new ActivityRepository(DbName, MongoHost);
            var prizes = total.Prizes.Deserialize<List<CardPrize>>() ?? new List<CardPrize>();
            var luckdraws = new LuckdrawModel[count];
            var cards = new CardPrize[count];
            for (var i = 0; i < count; i++)
            {
                //福卡
                var card = LuckDraw();

                //卡券
                long activityId;
                string name;
                string type;
                var couponId = LuckCoupon(out activityId, out type, out name);
                var reuslt = CardCouponApi.UserGrant(userId, activityId, couponId);
                var luckdraw = new LuckdrawModel
                {
                    MemberId = userId,
                    Key = GameKey,
                    Remark = reuslt.Data,
                    Type = type,
                    Name = name,
                    Prize = (int)couponId,
                    Phone = UserInfo.Phone,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };
                luckdraws[i] = luckdraw;
                cards[i] = new CardPrize
                {
                    Card = card,
                    CouponId = couponId
                };
            }

            activeRepository.AddMany(luckdraws);

            //
            prizes.AddRange(cards);
            //
            total.Used += count;
            total.NotUsed = total.Total - total.Used;
            total.Prizes = prizes.ToJson();
            total.LastUpdateTime = DateTime.Now;
            activeRepository.Update(total);

            var cardPrizes = new int[6];
            cardPrizes[0] = cards.Count(it => it.Card == CardType.FjCard); //房金卡
            cardPrizes[1] = cards.Count(it => it.Card == CardType.PtCard); //珀金
            cardPrizes[2] = cards.Count(it => it.Card == CardType.AuCard); //金
            cardPrizes[3] = cards.Count(it => it.Card == CardType.AgCard); //银
            cardPrizes[4] = cards.Count(it => it.Card == CardType.CuCard); //铜
            cardPrizes[5] = cards.Count(it => it.Card == CardType.FeCard); //铁
            return
                Json(new ResponseModel
                {
                    Data =
                        new
                        {
                            couponPrizes = luckdraws.Select(it => new { Key = it.Name + it.Type }),
                            cardPrizes,
                            notUsed = total.NotUsed
                        }
                });

        }

        /// <summary>
        /// 抽卡
        /// </summary>
        /// <returns></returns>
        private static CardType LuckDraw()
        {
            var n = RedisManager.GetIncrement("activity:" +GameKey);
            var s = n%100;
            return CardSqunce[s];
            /*
            if (n < 30)
            {
                return CardType.FeCard;
            }

            if (n < 30 + 10)
            {
                return CardType.CuCard;
            }

            if (n < 30 + 10 + 20)
            {
                return CardType.AgCard;
            }

            if (n < 30 + 10 + 20 + 30)
            {
                return CardType.AuCard;
            }

            if (n < 30 + 10 + 20 + 30 + 9)
            {
                return CardType.PtCard;
            }
            return CardType.FjCard;
            */
        }

        /// <summary>
        /// 抽券
        /// </summary>
        /// <returns></returns>
        private long LuckCoupon(out long activityId, out string type, out string name)
        {
            var config = GetConfig();
            activityId = config.ActivityId;
            var n = LuckRandom.Next(0, 100);
            if (n < 3)
            {
                type = "现金券";
                name = "4元";
                //4元现金券
                return config.CashCouponA;
            }

            if (n < 3 + 5)
            {
                type = "现金券";
                name = "5元";
                //5元现金券
                return config.CashCouponB;
            }

            if (n < 3 + 5 + 5)
            {
                type = "现金券";
                name = "8元";
                //8元现金券
                return config.CashCouponC;
            }

            if (n < 3 + 5 + 5 + 12)
            {
                type = "现金券";
                name = "10元";
                //10元现金券
                return config.CashCouponD;
            }

            if (n < 3 + 5 + 5 + 12 + 30)
            {
                type = "加息券";
                name = "1%";
                //1%加息券
                return config.RateCouponA;
            }

            if (n < 3 + 5 + 5 + 12 + 30 + 20)
            {
                type = "加息券";
                name = "1.5%";
                //1.5%加息券
                return config.RateCouponB;
            }

            if (n < 3 + 5 + 5 + 12 + 30 + 20 + 15)
            {
                type = "加息券";
                name = "2%";
                //2%加息券
                return config.RateCouponC;
            }
            type = "加息券";
            name = "2.5%";
            //2.5%加息券
            return config.RateCouponD;

        }

        #endregion

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

        /// <summary>
        /// 首页数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Record()
        {
            var rows = new ActivityRepository(DbName, MongoHost).Query<LuckdrawModel>(it => it.Key == GameKey).ToList();
            var data = new ResponseModel
            {
                Data = rows.Select(it => new { phone = StringHelper.CoverPhone(it.Phone), type = it.Type, name = it.Name }).ToArray()
            };
            return Json(data);
        }


    }


    /// <summary>
    /// 卡片类型
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// 无效的福卡
        /// </summary>
        None,

        /// <summary>
        /// 房金卡
        /// </summary>
        FjCard,

        /// <summary>
        /// 铂金卡
        /// </summary>
        PtCard,

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
