using FJW.Wechat.Cache;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Unit;
using FJW.Wechat.Activity.Models;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 母亲节活动
    /// </summary>
    [CrossDomainFilter]
    public class MothersDayController : ActivityController
    {

        private const string Key = "mothersday";

        private static readonly char[] luck = {
            'E','C','E','C','E','C','E','C','E','C','E','C','E','C','E','C','E','C','E','C',
            'E','C','E','C','E','C','E','D','E','D','B','C','E','D','E','C','B','C','E','D',
            'B','D','B','C','B','C','B','C','B','D','F','C','F','C','E','D','B','D','F','D',
            'D','C','B','D','E','D','E','D','E','D','F','C','A','D','E','C','D','C','D','C',
            'F','C','F','D','B','C','A','C','D','D','F','C','E','D','E','D','F','D','D','D'
        };

        private readonly SqlDataRepository _respoRepository;

        //获取配置
        private static MothersDayConfig GetConfig()
        {
            return JsonConfig.GetJson<MothersDayConfig>("Config/activity.mothersday.json");
        }
        protected ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }

        public MothersDayController()
        {
            _respoRepository = new SqlDataRepository(SqlConnectString);

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
            var config = GetConfig();

            var now = DateTime.Now;
            if (now < config.StartTime || now > config.EndTime)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "活动未开始或已过期" };


            return new ResponseModel { ErrorCode = ErrorCode.None, Data = "" };
        }

        /// <summary>
        /// 计算数量
        /// </summary>
        private void Summary(TotalChanceModel total, long userId)
        {
            var config = GetConfig();
            var dataCount = GetRepository().Query<TotalChanceModel>(it => it.MemberId == userId && it.Key == Key).FirstOrDefault();
            if (dataCount != null)
                total = dataCount;

            if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 30)
            {
                var shares = _respoRepository.GetProductTypeSingle(userId, config.StartTime, config.EndTime);
                int count = 0;
                int add=0;
                foreach (var r in shares)
                {
                    add = 0;
                    switch (r.ProductTypeId)
                    {
                        case 8:
                            add = (int)r.BuyShares / 100;
                            break;
                        case 7:
                            add = (int)r.BuyShares / 200;
                            break;
                        case 6:
                            add = (int)r.BuyShares / 400;
                            break;
                        case 5:
                            add = (int)r.BuyShares / 1200;
                            break;
                    }
                    count += add >= 1 ? add : 0;
                }
               
                var totalCnt = count;

                var useCount = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == userId && it.Type == "1").Count();

                var notUsed = totalCnt - useCount;

                if (notUsed < 0)
                {
                    total.NotUsed = 0;
                }
                else
                {
                    total.Total = totalCnt;
                    total.NotUsed = notUsed;
                }

                total.Key = Key;
                total.Used = useCount;
                total.MemberId = userId;
                total.LastStatisticsTime = DateTime.Now;
            }


            if (dataCount != null)
                GetRepository().Update(dataCount);

            else
                GetRepository().Add(total);

        }


        [HttpPost]
        /// <summary>
        /// 点灯发放卡券
        /// </summary>
        /// <returns></returns>
        public ActionResult Give()
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            //统计次数
            Summary(new TotalChanceModel(), UserInfo.Id);


            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();
            //祈福灯点亮次数
            var count = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id && it.Type == "1").Count();


            var isOpen = string.IsNullOrEmpty(userChance?.Remark) ? "1" : "";//能否点亮百岁灯
            var isMessage = string.IsNullOrEmpty(userChance.Prizes) ? "1" : "";//能否发短信
            //已经发放卡券
            if (count == userChance?.Total || count >= 9)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = count, isOpen, isMessage } });

            if (userChance?.Total > 0 && userChance.Total > count)
            {
                var resCount = userChance.Total > 9 ? 9 : userChance.Total;
                for (int i = count + 1; i <= resCount; i++)
                {
                    //发放卡券
                    bool giveResult;
                    ExchangePrizes(UserInfo.Id, i, out giveResult);
                    //更新点亮情况
                    if (giveResult)
                    {
                        userChance.Used++;
                        userChance.NotUsed--;
                        GetRepository().Update(userChance);
                    }
                }
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = resCount, isOpen, isMessage } });
            }
            return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Data = "点亮机会不足" });
        }

        /// <summary>
        /// 发放卡券
        /// </summary>
        private string ExchangePrizes(long memberId, int ranking, out bool giveResult)
        {
            var config = GetConfig();
            string name = "";
            giveResult = false;
            long counponId = 0;
            switch (ranking)
            {
                case 1:
                    name = "5元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon1);
                    break;
                case 2:
                    name = "2%加息券";
                    counponId = Convert.ToInt64(config.RateCoupon2);
                    break;
                case 3:
                    name = "8元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon3);
                    break;
                case 4:
                    name = "15元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon4);
                    break;
                case 5:
                    name = "10元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon5);
                    break;
                case 6:
                    name = "20元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon6);
                    break;
                case 7:
                    name = "3.5%加息券";
                    counponId = Convert.ToInt64(config.RateCoupon7);
                    break;
                case 8:
                    name = "30元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon8);
                    break;
                case 9:
                    name = "40元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon9);
                    break;
            }

            var result = CardCouponApi.UserGrant(memberId, config.ActivityId, counponId);
            Logger.Info("mothersday memberId:{0} reuslt:{1}", memberId, result.ToJson());
            if (result.IsOk)
            {
                giveResult = true;
                GetRepository().Add(new LuckdrawModel
                {
                    MemberId = UserInfo.Id,
                    Phone = UserInfo.Phone,
                    Prize = counponId,
                    Type = "1",
                    Key = Key,
                    Sequnce = ranking,
                    Name = name,
                    Remark = "母亲节活动-" + name
                });
            }
            return name;
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <returns></returns>
        public ActionResult SendSms(string msg, long phone)
        {
            var smsMsg = "";
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            //是否能发送短信
            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();
            if (!string.IsNullOrEmpty(userChance?.Prizes) && userChance.Total >= 3)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "条件不满足。" });
            }

            switch (msg)
            {
                case "1":
                    smsMsg = "亲爱的妈妈，孩子祝您身体健康，吃嘛嘛香;吉祥如意，做嘛嘛顺。愿您事事顺心，时时舒心，年年安心，天天开心，永葆童心！"; break;
                case "2":
                    smsMsg = "您在我眼中是最美，每一个微笑都让我幸福。太多的语言也无法向您表达，我对您深深的爱意。我爱您，妈妈！祝您天天快乐，永远年轻！"; break;
                case "3":
                    smsMsg = "老婆，辛苦了，你永远是我的宝贝，是孩子的漂亮妈妈，祝你母亲节快乐，心情靓足一百分！"; break;
                case "4":
                    smsMsg = "母爱无价买不到，愿孩子Ta妈开心快乐，皱纹减少，每天生活没烦恼。"; break;
            }

            var realName = _respoRepository.GetMemberRealName(UserInfo.Id);

            if (string.IsNullOrEmpty(smsMsg))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Data = "短信内容无效。" });
            if (!Regex.IsMatch(phone.ToString(), "^((13[0-9])|(14[5|7])|(15([0-3]|[5-9]))|(18[0,5-9]))\\d{8}$"))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "手机号格式不正确" });

            var smsRes = _respoRepository.AddSms(new Sms
            {
                Phone = phone,
                Msg = smsMsg + "——" + realName,
                sign = 1
            });

            if (smsRes <= 0)
            {
                Logger.Info($"短信发送失败：Phone:{UserInfo.Phone},ActivityName:MothersDay");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Data = "Error" });
            }
            userChance.Prizes = new { sms = 1, smsStr = msg, phone }.ToJson();
            userChance.LastStatisticsTime = DateTime.Now;
            GetRepository().Update(userChance);
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = "OK" });

        }

        public ActionResult LuckDraw()
        {
            //百岁灯点亮次数
            var prizeCount = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id && it.Type == "2").Count();

            //祈福灯点亮次数
            var count = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id && it.Type == "1").Count();

            if (prizeCount > 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "已点亮过百岁灯。" });

            if (count < 9)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "请先点亮祈福灯噢。" });


            var n = RedisManager.GetIncrement("activity:" + Key);
            var name = "";
            var luckId = -1;
            long s = n % 100;
            var c = luck[s];
            switch (c)
            {
                case 'A':
                    name = "无线按摩器";
                    luckId = 0;
                    break;
                case 'B':
                    name = "定制创意奖杯";
                    luckId = 1; break;
                case 'C':
                    name = "妈妈紫砂杯";
                    luckId = 4;
                    break;
                case 'D':
                    name = "足季艾叶足贴";
                    luckId = 5;
                    break;
                case 'E':
                    name = "馥珮护手霜";
                    luckId = 3;
                    break;
                case 'F':
                    name = "仿真肥皂花";
                    luckId = 2;
                    break;
            }

            GetRepository().Add(new LuckdrawModel
            {
                MemberId = UserInfo.Id,
                Phone = UserInfo.Phone,
                Prize = -1,
                Key = Key,
                Type = "2",
                Name = name,
                Remark = "母亲节活动-" + name
            });

            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();
            if (userChance != null)
            {
                userChance.Remark = new { LuckDraw = 1, Date = DateTime.Now }.ToJson();
                GetRepository().Update(userChance);
            }
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { luckId, name } });
        }

        public ActionResult MemberRecord()
        {
            var resData = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).OrderByDescending(it => it.CreateTime).Select(it => new { Sequnce = it.Sequnce, Name = it.Name });
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = resData });
        }
    }
    //奖励实体
    public class MothersDayConfig
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public long ActivityId { get; set; }

        public long RateCoupon1 { get; set; }

        public long RateCoupon2 { get; set; }
        public long RateCoupon3 { get; set; }
        public long RateCoupon4 { get; set; }
        public long RateCoupon5 { get; set; }
        public long RateCoupon6 { get; set; }
        public long RateCoupon7 { get; set; }
        public long RateCoupon8 { get; set; }
        public long RateCoupon9 { get; set; }


    }
}
