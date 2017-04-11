

using System;
using System.Linq;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Activity.Rules;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 开宝箱(口令)
    /// </summary>
    [CrossDomainFilter]
    public class BoxwordController: ActivityController
    {
        private const string GameKey = "boxword";

        private static readonly char[] Squence = { 'B', 'E', 'B', 'E', 'B', 'E', 'D', 'E', 'D', 'E', 'D',
            'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'B', 'E', 'D', 'E', 'B', 'E',
            'B', 'E', 'C', 'E', 'D', 'E', 'B', 'E', 'C', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'C',
            'E', 'A', 'E', 'D', 'E', 'A', 'E', 'A', 'E', 'C', 'E', 'A', 'E', 'A', 'E', 'C', 'E', 'B', 'E',
            'A', 'E', 'C', 'C', 'C', 'C', 'D', 'C', 'A', 'C', 'A', 'C', 'C', 'C', 'B', 'C', 'C', 'E', 'A',
            'C', 'A', 'C', 'D', 'E', 'D', 'E', 'D', 'E', 'B', 'E', 'C', 'C' };

        /// <summary>
        /// 总机会
        /// </summary>
        /// <returns></returns>
        public ActionResult Total()
        {
            var config = BoxWordConfig.GetConfig();
            if (config.StartTime > DateTime.Now)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Message = "活动未开始" });
            }
            if (config.EndTime < DateTime.Now)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Message = "活动已结束" });
            }
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var repository = GetRepository();
            var uid = UserInfo.Id;
            var d = DateTime.Now.Year*1000 + DateTime.Now.Month*100 + DateTime.Now.Day;
            var row = repository.Query<TotalChanceModel>(it => it.MemberId == uid && it.Key == GameKey && it.Date == d).FirstOrDefault();
             
            if (row == null)
            {
                row = new TotalChanceModel
                {
                    MemberId = uid,
                    Key = GameKey,
                    Total = 2,
                    Used = 0,
                    NotUsed = 2,
                    Date = d,
                    CreateTime = DateTime.Now
                };
                repository.Add(row);
            }

            return Json(new ResponseModel
            {
                Data = new {total = row.Total, used = row.Used, notUsed = row.NotUsed}
            });
        }

        /// <summary>
        /// 兑换
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Exchange(string word = "")
        {
            var config = BoxWordConfig.GetConfig();
            if (config.StartTime > DateTime.Now)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Message = "活动未开始"});
            }
            if (config.EndTime < DateTime.Now)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Message = "活动已结束" });
            }
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }

            var repository = GetRepository();
            var uid = UserInfo.Id;
            var d = DateTime.Now.Year * 1000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
            var row = repository.Query<TotalChanceModel>(it => it.MemberId == uid && it.Key == GameKey && it.Date == d).FirstOrDefault();
            if (row == null)
            {
                row = new TotalChanceModel
                {
                    MemberId = uid,
                    Key = GameKey,
                    Total = 2,
                    Used = 0,
                    NotUsed = 2,
                    Date = d,
                    CreateTime = DateTime.Now
                };
                repository.Add(row);
            }

            if (row.Used >= 2)
            {
                return Json(new ResponseModel
                {
                   ErrorCode = ErrorCode.Other, Message = "您今天免费开天天宝箱的机会已用完，投资可打开更高等级的宝箱哦~"
                });
            }

            if (row.Used > 0 )
            {
                var days = (int) (DateTime.Now.Date - config.StartTime.Date).TotalDays;
                var code = BoxWordRule.GetWord(days);
                if (string.IsNullOrEmpty(word) || code != word)
                {
                    return Json(new ResponseModel
                    {
                        ErrorCode = ErrorCode.NotVerified,
                        Message = "口令错误"
                    });
                }
            }

            row.Used++;
            row.NotUsed--;
            row.LastUpdateTime = DateTime.Now;
            long couponId;
            long sequnce;
            var name = GiveCoupin(config, out couponId, out sequnce);
            var result = CardCouponApi.UserGrant(uid, config.ActivityId, couponId);
            var luckdraw = new LuckdrawModel
            {
                MemberId = uid,
                Key = GameKey,
                Remark = result.Data,
                Name = name,
                Sequnce = sequnce,
                Prize = couponId,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            repository.Update(row);
            repository.Add(luckdraw);

            return Json(new ResponseModel
            {
                Data = name
            });
        }

        private static string GiveCoupin(BoxWordConfig config, out long couponId, out long s)
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey);
            s = n % 100;
            var c = Squence[s];

            switch (c)
            {
                case 'A':
                    couponId = config.CouponA;
                    return "3元现金券";

                case 'B':
                    couponId = config.CouponB;
                    return "2元现金券";

                case 'C':
                    couponId = config.CouponC;
                    return "10元现金券";

                case 'D':
                    couponId = config.CouponD;
                    return "5元现金券";

                case 'E':
                    couponId = config.CouponE;
                    return "15元现金券";
            }
            couponId = -1;
            return "";
        }

    }
}
