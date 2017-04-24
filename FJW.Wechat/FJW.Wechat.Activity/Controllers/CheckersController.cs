using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using FJW.SDK2Api.CardCoupon;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 跳棋
    /// </summary>
    public class CheckersController : ActivityController
    {
        private const string GameKey = "checkers";
        private static readonly Random Random = new Random();
        public ActionResult Total()
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var userId = UserInfo.Id;
            var config = CheckersConfig.GetConfig();
            var activeRepository = GetRepository();
            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            var isNew = false;
            if (total == null)
            {
                total = new TotalChanceModel
                {
                    Used = 0,
                    Key = GameKey,
                    MemberId = userId,
                    NotUsed = 0,
                    Total = 0,
                    Prizes = string.Empty
                };

                isNew = true;
            }
            var state = total.Prizes.Deserialize<State>() ?? new State();
            Summary(total, userId, config, state.Shared, state.Cycle);
           
            if (!isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Add(total);
            }
  
            return Json(new ResponseModel
            {
                Data = new { notUsed = total.NotUsed,  state.Sequnce }
            });
        }


        /// <summary>
        /// 计算数量
        /// </summary>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <param name="config"></param>
        /// <param name="shared">是否分享过</param>
        /// <param name="cycle"></param>
        private void Summary(TotalChanceModel total, long userId, CheckersConfig config, bool shared, int cycle)
        {
            var sqlRepository = new SqlDataRepository(SqlConnectString);
            if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 30)
            {
                var shares = sqlRepository.ProductTypeBuyRecrods(userId, config.StartTime, config.EndTime);
                int count = 0;
                foreach (var r in shares)
                {
                    switch (r.ProductTypeId)
                    {
                        case 8:
                            count += (int)r.BuyShares / 500;
                            break;
                        case 7:
                            count += (int)r.BuyShares / 1000;
                            break;
                        case 6:
                            count += (int)r.BuyShares / 2000;
                            break;
                    }
                }

                var totalCnt = count;
                if (shared)
                {
                    totalCnt++;
                }
                //周期
                totalCnt += cycle;
                var notUsed = totalCnt - total.Used;
                total.Remark = shares.ToJson();
                if (notUsed < 0)
                {
                    total.NotUsed = 0;
                }
                else
                {
                    total.Total = totalCnt;
                    total.NotUsed = notUsed;
                }

                total.LastStatisticsTime = DateTime.Now;
            }
        }


        /// <summary>
        /// 分享结果
        /// </summary>
        /// <param name="d"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public ActionResult ShareResult(string d, string t)
        {
            return new EmptyResult();
        }


        /// <summary>
        /// Next
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Next()
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var userId = UserInfo.Id;
            var config = CheckersConfig.GetConfig();


            var activeRepository = GetRepository();

            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            var isNew = false;
            if (total == null)
            {
                total = new TotalChanceModel
                {
                    Used = 0,
                    Key = GameKey,
                    MemberId = userId,
                    NotUsed = 0,
                    Total = 0,
                    Prizes = string.Empty
                };
                isNew = true;
            }

            var state = total.Prizes.Deserialize<State>() ?? new State();
            Summary(total, userId, config, state.Shared, state.Cycle);

            long couponId;
            int next;
            int type;
            
            if (total.NotUsed < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "机会已用完，快快去投资" });
            }

            if (state.Sequnce >= 30)//重新开始
            {
                state.Sequnce = 0;
            }

            var name = LuckRaw(config, state.Sequnce, out couponId, out next, out type);
            if (next >= 30)
            {
                //再来一次
                state.Cycle++;
            }
            total.NotUsed--;
            total.Used++;
            state.Sequnce = next;
            total.Prizes = state.ToJson();

            var result = couponId > 0 ? CardCouponApi.UserGrant(userId, config.ActivityId, couponId).Data : string.Empty;
            var luckdraw = new LuckdrawModel
            {
                MemberId = userId,
                Key = GameKey,
                Remark = result,
                Name = name,
                Type = type.ToString(),
                Sequnce = next,
                Prize = couponId,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            activeRepository.Add(luckdraw);

            total.LastUpdateTime = DateTime.Now;
       
            if (!isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Add(total);
            }

            return Json(new ResponseModel { Data = new { name, notUsed = total.NotUsed, next} });
        }


        private static string LuckRaw(CheckersConfig config, int current, out long couponId, out int next, out int type )
        {
            next = 0;
            var n = Random.Next(1, 7);
            string name;
            if (  current == 0)
            {
                //2
                var canben = CanBe2();
                
                if (canben)
                {
                    next = 2;
                }
                if (! canben )
                {
                    next = current + n;
                    if (next == 2)
                    {
                        next = 3;
                    }
                }
                couponId = Prize(config, current, out name, out type);
                return name;
            }
            //not 9
            if (current + 6 < 9) //  1, 2
            {
                next = current + n;
                couponId = Prize(config, current, out name, out type);
                return name;
            }

            //9
            if (current + 6 >= 9 && current+ 6 < 12) // 3, 4, 5
            {
                var canbe = CanBe9();
                if (canbe)
                {
                    next = 9;
                }
                if (!canbe )
                {
                    next = current + n;
                    if (next == 9)
                    {
                        next = 8;
                    }
                    couponId = Prize(config, current, out name, out type);
                    return name;
                }
            }
            //12
            if (current + 6 >= 12 && current + 6 < 16) // 6, 7, 8, 9
            {
                var canbe = CanBe12();
                if (canbe)
                {
                    next = 12;
                }
                if (!canbe)
                {
                    next = current + n;
                    if (next == 12)
                    {
                        next = 11;
                    }
                    couponId = Prize(config, current, out name, out type);
                    return name;
                }
            }

            //16
            if (current + 6 >= 16 && current + 6 < 22) // 10, 11, 12, 13, 14, 15
            {
                var canbe = CanBe16();
                if (canbe)
                {
                    next = 16;
                }
                if (!canbe)
                {
                    next = current + n;
                    if (next == 22)
                    {
                        next = 21;
                    }
                    couponId = Prize(config, current, out name, out type);
                    return name;
                }
            }
            
            //22
            if (current + 6 >= 22 && current + 6 < 27) // 16, 17, 18, 19, 20
            {
                var canbe = CanBe22();
                if (canbe)
                {
                    next = 22;
                }
                if (!canbe)
                {
                    next = current + n;
                    if (next == 22)
                    {
                        next = 21;
                    }
                }
                couponId = Prize(config, current, out name, out type);
                return name;
            }
            //27
            if (current< 27 && current + 6 >= 27 ) // 21, 22, 23, 24, 25, 26
            {
                var canbe = CanBe27();
                if (canbe)
                {
                    next = 27;
                }
                if (!canbe)
                {
                    next = current + n;

                    if (next == 27)
                    {
                        next = 26;
                    }

                    if (next > 30)
                    {
                        next = 30;
                    }
                }
                couponId = Prize(config, current, out name, out type);
                return name;
            }
            next = current + n;
            if (next > 30)
            {
                next = 30;
            }
            couponId = Prize(config, current, out name, out type);
            return name;
        }

        private static long Prize(CheckersConfig config, long sequnce , out string name, out int type)
        {
            type = 1;
            long prize;
            switch (sequnce)
            {
                case 1:
                    prize = config.Card1;
                    name = "5元现金券";
                    break;

                case 2:
                    prize = 0;
                    type = 2;
                    name = "10现金";
                    break;

                case 3:
                    prize = config.Card3;
                    name = "1%加息券";
                    break;

                case 4:
                    prize = config.Card4;
                    name = "8元现金券";
                    break;

                case 5:
                    prize = config.Card5;
                    name = "15元现金券";
                    break;

                case 6:
                    prize = config.Card6;
                    name = "20元现金券";
                    break;

                case 7:
                    prize = config.Card7;
                    name = "25元现金券";
                    break;

                case 8:
                    prize = config.Card8;
                    name = "30元现金券";
                    break;

                case 9:
                    prize =  0;
                    name = "Kindle入门款电子书阅读器";
                    break;

                case 10:
                    prize = config.Card10;
                    name = "35元现金券";
                    break;

                case 11:
                    prize = config.Card11;
                    name = "3%加息券";
                    break;

                case 12:
                    prize =  0;
                    name = "20元现";
                    type = 2;
                    break;

                case 13:
                    prize = config.Card13;
                    name = "40元现金券";
                    break;

                case 14:
                    prize = config.Card14;
                    name = "50元现金券";
                    break;

                case 15:
                    prize = config.Card15;
                    name = "3%加息券";
                    break;

                case 16:
                    prize = 0;
                    type = 3;
                    name = "欧乐电动牙刷";
                    break;

                case 17:
                    prize = config.Card17;
                    name = "60元现金券";
                    break;

                case 18:
                    prize = config.Card18;
                    name = "3.5%加息券";
                    break;

                case 19:
                    prize = config.Card19;
                    name = "65元现金券";
                    break;

                case 20:
                    prize = config.Card20;
                    name = "70元现金券";
                    break;

                case 21:
                    prize = config.Card21;
                    name = "4%加息券";
                    break;

                case 22:
                    prize = 0;
                    name = "荣事达养生壶";
                    break;

                case 23:
                    prize = config.Card23;
                    name = "75元现金券";
                    break;

                case 24:
                    prize = config.Card24;
                    name = "4.5%加息券";
                    break;

                case 25:
                    prize = config.Card25;
                    name = "80元现金券";
                    break;

                case 26:
                    prize = config.Card26;
                    name = "90元现金券";
                    break;

                case 27:
                    prize = 0;
                    name = "松下空气净化器";
                    type = 3;
                    break;

                case 28:
                    prize = config.Card28;
                    name = "100元现金券";
                    break;

                case 29:
                    prize = config.Card29;
                    name = "8%加息券";
                    break;

                case 30:
                    prize = 0;
                    type = 4;
                    name = "一次掷骰子机会";
                    break;

                default:
                    prize = 0;
                    name = "";
                    type = -1;
                    break;
            }
            return prize;
        }

        private static bool CanBe2()
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey + "2");
            if (n == 0)
            {
                //跳过第一次
                return false;
            }
            return n % 10 == 0;
        }

        private static bool CanBe9()
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey + "9");
            var m = n%100;
            if (m == 0) //跳过 0 
            {
                return false;
            }
            return m % 33 == 0;
        }

        private static bool CanBe12()
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey + "12");
            return n % 5 == 0;
        }

        private static bool CanBe16()
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey + "16") % 100;
            return n % 12 == 0;
        }

        private static bool CanBe22()
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey + "22") % 100;
            return n % 16 == 0;
        }

        private static bool CanBe27()
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey + "27") % 100;
            return n % 99 == 0;
        }
        
    }

    public class State
    {
        /// <summary>
        /// 是否分享
        /// </summary>
        public bool Shared { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Sequnce { get; set; }

        /// <summary>
        /// 周期
        /// </summary>
        public int Cycle { get; set; }
    }
}
