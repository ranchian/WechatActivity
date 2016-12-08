using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Activity.Unit;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Wx;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 颜值 活动
    /// </summary>
    //[WAuthorize]
    [CrossDomainFilter]
    public class FacevalueController : ActivityController
    {
        private const string Key = "facevalue";
        private static readonly Random LuckRandom = new Random();
        private readonly ActivityRepository _repsitory;

        public FacevalueController()
        {
            _repsitory = new ActivityRepository(DbName, MongoHost);
        }
         
        /// <summary>
        /// 评估
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Appraise()
        {
            var config = GetConfig();
            if (config.StartTime > DateTime.Now )
            {
                return Json(new ResponseModel(ErrorCode.Other)
                {
                    Data = new Dictionary<string, object>{ ["code"] = 10,["msg"] = "活动未开始"},
                    Message = "活动未开始"
                });
            }
            if (config.EndTime < DateTime.Now)
            {
                return Json(new ResponseModel(ErrorCode.Other)
                {
                    Data = new Dictionary<string, object> { ["code"] = 10, ["msg"] = "活动已结束" },
                    Message = "活动已结束"
                });
            }

            var uid = UserInfo.Id;

            var mediaId = Request.Form["url"];
            var key = $"activity/{Key}/{mediaId}.jpg";
          
            var bytes = DependencyResolver.Current.GetService<IWxMediaApi>().Get(mediaId);
            
            QiniuHelper.UploadData(bytes, key);

            var reuslt = QiniuHelper.CheckFace(key);
            //Logger.Info("key:{0} faceresult:{1}", key, reuslt.ToJson());
            if (!reuslt.Data)
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 100,
                    ["msg"] = "无法辨识人脸的图片"
                };
               return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "无法辨识人脸的图片" });
            }
            int prize;
            decimal money;
            string title;
            var n = Luckdraw(out prize, out money, out title);
            var model = new FaceValueModel
            {
                Sequnce = n,
                Money = money,
                Prize = prize,
                Title = title,
                Url = $"http://static.fangjinnet.com/{key}"
            };
            RecordModel data = null;
            if (uid > 0)
            {
                data = _repsitory.Query<RecordModel>(it => it.Key == Key && it.MemberId == uid).FirstOrDefault();
            }
            var dataId = GetSelfGameRecordId();
            if (data == null && !dataId.IsNullOrEmpty())
            {
                data = _repsitory.GetById(dataId);
            }
            if (data == null)
            {
                data = new RecordModel
                {
                    RecordId = Guid.NewGuid().ToString(),
                    MemberId = uid,
                    Total = 24,
                    Key = Key,
                    Score = reuslt.Data ? 1 : 0,
                    Status = 0,
                    Data = model.ToJson(),
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };
                _repsitory.Add(data);
                SetSelfGameRecordId(data.RecordId);
            }
            else
            {
                if (data.Status == 0)
                {
                    data.MemberId = uid;
                    data.Data = model.ToJson();
                    data.LastUpdateTime = DateTime.Now;
                    _repsitory.Update(data);
                }
            }
            return Json(new ResponseModel { Data = new { url = model.Url,  name = model.Title + PrizeType(model.Prize) } });
            //return Json(new ResponseModel {ErrorCode = ErrorCode.NotVerified, Message = "无效的图片"});
        }


        /// <summary>
        /// 接受 同意
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Accept()
        {
            var config = GetConfig();
            if (config.StartTime > DateTime.Now)
            {
                return Json(new ResponseModel(ErrorCode.Other)
                {
                    Data = new Dictionary<string, object> { ["code"] = 10, ["msg"] = "活动未开始" },
                    Message = "活动未开始"
                });
            }
            if (config.EndTime < DateTime.Now)
            {
                return Json(new ResponseModel(ErrorCode.Other)
                {
                    Data = new Dictionary<string, object> { ["code"] = 10, ["msg"] = "活动已结束" },
                    Message = "活动已结束"
                });
            }
            var uid = UserInfo.Id;
           
            if (uid < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            RecordModel data = null;

            if (uid > 0)
            {
                data = _repsitory.Query<RecordModel>(it => it.Key == Key && it.MemberId == uid).FirstOrDefault();
            }
            if (data == null)
            {
                var dataId = GetSelfGameRecordId();
                if (!dataId.IsNullOrEmpty())
                {
                    data = _repsitory.GetById(dataId);
                    Logger.Dedug("SessionId:{1} GetSelfGameRecordId:{0}", data.RecordId, Session.SessionID);
                }
            }
            if (data == null || data.Score == 0 || string.IsNullOrEmpty(data.Data))
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 1;
                dict["msg"] = "没有游戏数据";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "没有游戏数据" });
            }
            if (data.Status > 0)
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 2;
                dict["msg"] = "已经领取过奖励了";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "已经领取过奖励了" });
            }
            var channl = new SqlDataRepository(SqlConnectString).GetMemberChennel(uid);
            if (channl != null && channl.Channel != null
                  && channl.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase)
                   && channl.CreateTime > new DateTime(2016, 12, 09))
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 6;
                dict["msg"] = "无法领取奖励：WQWLCPS";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "无法领取奖励：WQWLCPS" });
            }
            if (new MemberRepository( SqlConnectString).DisableMemberInvite(uid))
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 6;
                dict["msg"] = "无法领取奖励：特殊的邀请人";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "无法领取奖励：特殊的邀请人" });
            }

            var jsonObj = data.Data.Deserialize<FaceValueModel>();

            ExchangePrizes(uid, jsonObj.Prize, jsonObj.Money);
            data.Status = 1;
            data.MemberId = uid;
            data.LastUpdateTime = DateTime.Now;
            _repsitory.Update(data);
            return Json(new ResponseModel());
        }

        /// <summary>
        /// 结果
        /// </summary>
        /// <returns></returns>
        public ActionResult Result()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel(ErrorCode.NotLogged));
            }
            var row =
                _repsitory.Query<RecordModel>(it => it.Key == Key && it.MemberId == userId && it.Status == 1)
                    .FirstOrDefault();

            if (row == null)
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 1;
                dict["msg"] = "没有游戏数据";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "没有游戏数据" });
            }
            var data = row.Data.Deserialize<FaceValueModel>();

            return Json(new ResponseModel
            {
                Data =
                    new
                    {
                        title = data.Title,
                        type = PrizeType(data.Prize),
                        time = row.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
                    }
            });
        }


        private static int Luckdraw(out int prize, out decimal money, out string title)
        {
            var n = LuckRandom.Next(0, 100);

            //2%加息券
            var m = 5;
            if (n < m)
            {
                prize = 1;
                title = "2%";
                money = 2;
                return n;
            }

            //2.5%加息券
            m += 10;
            if (n < m)
            {
                prize = 2;
                title = "2.5%";
                money = 2.5m;
                return n;
            }

            //3.5%加息券
            m += 15;
            if (n < m)
            {
                prize = 3;
                title = "3.5%";
                money = 3.5m;
                return n;
            }

            //3%加息券
            m += 15;
            if (n < m)
            {
                prize = 4;
                title = "3%";
                money = 3;
                return n;
            }

            //4%加息券
            m += 5;
            if (n < m)
            {
                prize = 5;
                title = "4%";
                money = 4;
                return n;
            }

            //1元现金券
            m += 5;
            if (n < m)
            {
                prize = 6;
                title = "1元";
                money = 1;
                return n;
            }

            //3元现金券
            m += 15;
            if (n < m)
            {
                prize = 7;
                title = "3元";
                money = 3;
                return n;
            }

            //5元现金券
            m += 10;
            if (n < m)
            {
                prize = 8;
                title = "5元";
                money = 5;
                return n;
            }

            //10元现金券
            m += 5;
            if (n < m)
            {
                prize = 9;
                title = "10元";
                money = 10;
                return n;
            }

            //100元体验金
            m += 5;
            if (n < m)
            {
                prize = 10;
                title = "100元";
                money = 100;
                return n;
            }

            //200元体验金
            m += 4;
            if (n < m)
            {
                prize = 11;
                title = "200元";
                money = 200;
                return n;
            }

            //500元体验金
            m += 3;
            if (n < m)
            {
                prize = 12;
                title = "500元";
                money = 500;
                return n;
            }

            //1000元体验金
            m += 2;
            if (n < m)
            {
                prize = 13;
                title = "1000元";
                money = 1000;
                return n;
            }

            //10000元体验金

            prize = 14;
            title = "10000元";
            money = 10000;
            return n;
        }

        private static string PrizeType(int prize)
        {
            if (prize < 1)
            {
                return string.Empty;
            }
            if (prize < 6)
            {
                return "加息劵";
            }
            if (prize < 10)
            {
                return "现金券";
            }
            if (prize < 15)
            {
                return "体验金";
            }
            return string.Empty;
        }

        private static FaceValueConfig GetConfig()
        {
            return JsonConfig.GetJson<FaceValueConfig>("Config/activity.facevalue.json");
        }

        private void ExchangePrizes(long memberId, int prize, decimal money)
        {
            var config = GetConfig();
            object result;
            switch (prize)
            {
                case 1:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponA);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;

                case 2:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponB);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;

                case 3:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponC);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;

                case 4:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponD);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;
                case 5:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.RateCouponE);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;

                case 6:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.CashCouponA);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;
                case 7:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.CashCouponB);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;
                case 8:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.CashCouponC);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;
                case 9:
                    result = CardCouponApi.UserGrant(memberId, config.ActivityId, config.CashCouponD);
                    Logger.Info("facevalue memberId:{0} reuslt:{1}", memberId, result.ToJson());
                    break;

                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    new MemberRepository(SqlConnectString).Give(memberId, config.ExperienceId, 2, money, memberId);
                    break;
            }
        }

    }

    public class FaceValueModel
    {
        public int Prize { get; set; }

        public decimal Money { get; set; }

        public string Title { get; set; }

        public int Sequnce { get; set; }

        public string Url { get; set; }

    }

    public class FaceValueConfig
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public long ActivityId { get; set; }

        public long ExperienceId { get; set; }

        public long RateCouponA { get; set; }

        public long RateCouponB { get; set; }

        public long RateCouponC { get; set; }

        public long RateCouponD { get; set; }

        public long RateCouponE { get; set; }

        public long CashCouponA { get; set; }

        public long CashCouponB { get; set; }

        public long CashCouponC { get; set; }

        public long CashCouponD { get; set; }



    }
}