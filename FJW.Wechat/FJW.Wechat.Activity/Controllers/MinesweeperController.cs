using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Activity.Controllers
{

    /// <summary>
    /// 扫雷
    /// </summary>
    [CrossDomainFilter]
    public class MinesweeperController : ActivityController
    {

        private const string Key = "minesweeper";
        private readonly ActivityRepository _repsitory;

        public MinesweeperController()
        {
            _repsitory = new ActivityRepository(DbName, MongoHost);
        }


        [OutputCache(Duration = 10)]
        public ActionResult Record()
        {
            int cnt;
            var data = _repsitory.QueryDesc<LuckdrawModel, DateTime>(it => it.Key == Key, it => it.CreateTime, 20, 0, out cnt).Select(it => new
            {
                name = PrizeType(it.Prize),
                price = PrizeAmount(it.Prize),
                phone = StringHelper.CoverPhone(it.Phone)
            });
            return Json(new ResponseModel
            {
                Data = data
            });
        }

        /// <summary>
        /// 状态
        /// </summary>
        /// <returns></returns>
        public ActionResult State()
        {
            var dt = DateTime.Now;
            var userId = UserInfo.Id;
            if (dt < new DateTime(2016, 11, 15, 10, 0, 0) && userId != 27329 && userId != 27331 && userId != 255925)
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 4,
                    ["msg"] = "活动未开始"
                };
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动未开始"
                });
            }

            if (dt > new DateTime(2016, 11, 28, 17, 0, 0))
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 5,
                    ["msg"] = "活动已结束"
                };
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动已结束"
                });
            }

            if (userId < 1)
            {
                return Json(new ResponseModel(ErrorCode.NotLogged));
            }
            var d = _repsitory.Query<RecordModel>(it => it.Key == Key && it.MemberId == userId).FirstOrDefault();
            if (d != null && d.Status != 0)
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 2,
                    ["msg"] = "已经领取过奖励了"
                };
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "已经领取过奖励了" });
            }
            return Json(new ResponseModel());
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Over(MineSweeperModel result)
        {
            var uid = UserInfo.Id;
            var dt = DateTime.Now;
            if (dt < new DateTime(2016, 11, 15, 10, 0, 0) &&  uid != 27329 && uid != 27331 && uid != 255925)
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 4,
                    ["msg"] = "活动未开始"
                };
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动未开始"
                });
            }

            if (dt > new DateTime(2016, 11, 28, 17, 0, 0))
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 5,
                    ["msg"] = "活动已结束"
                };
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动已结束"
                });
            }
            if (result == null || result.Rows == null || result.Rows.Count == 0 || !CheckResult(result.Rows))
            {
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.NotVerified,
                    Message = "游戏数据不正确"
                });
            }
            var displayArr = new List<string>();
            foreach (var p in result.Rows)
            {
                var t = DisplayName(p);
                if (t != null)
                {
                    displayArr.Add(t);
                }
            }
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
                    Score = result.Rows.Count,
                    Status = 0,
                    Data = result.Rows.ToJson(),
                    CreateTime = DateTime.Now
                };
                _repsitory.Add(data);
                SetSelfGameRecordId(data.RecordId);
                Logger.Dedug("SessionId:{1} SetSelfGameRecordId:{0}", data.RecordId, Session.SessionID);

            }
            else
            {
                if (data.Status != 0)
                {
                    var dict = new Dictionary<string, object>
                    {
                        ["code"] = 2,
                        ["msg"] = "已经领取过奖励了"
                    };
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "已经领取过奖励了" });
                }
                data.Data = result.Rows.ToJson();
                data.MemberId = uid;
                data.Score = result.Rows.Count;
                data.LastUpdateTime = DateTime.Now;
                _repsitory.Update(data);
            }
            return
                Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.None,
                    Data = displayArr.GroupBy(it => it).Select(it => new { name = it.Key, count = it.Count() })
                });
        }

        /// <summary>
        /// 领取结果
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Accept()
        {
            var uid = UserInfo.Id;
            var dt = DateTime.Now;
            if (dt < new DateTime(2016, 11, 15, 10, 0, 0) && uid != 27329 && uid != 27331 && uid != 255925)
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 4,
                    ["msg"] = "活动未开始"
                };
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动未开始"
                });
            }

            if (dt > new DateTime(2016, 11, 28, 17, 0, 0))
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 5,
                    ["msg"] = "活动已结束"
                };
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动已结束"
                });
            }

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
            if (data == null)
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
            var result = data.Data.Deserialize<string[]>();
            if (data.Score < 1 || result.Length < 1)
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 3;
                dict["msg"] = "无效的奖励";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "无效的奖励" });
            }

            var channl = new SqlDataRepository(SqlConnectString).GetMemberChennel(uid);
            if (channl != null && channl.Channel != null  
                && channl.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase)
                && channl.CreateTime > new DateTime(2016, 11, 15))
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 6;
                dict["msg"] = "无法领取奖励：WQWLCPS";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "无法领取奖励：WQWLCPS" });
            }

            data.MemberId = uid;
            data.Status = 1;
            data.LastUpdateTime = DateTime.Now;
            var config = GetConfig();
            foreach (var row in result)
            {
                string name;
                var sequnce = GetSequnce();
                int przieId;
                ExchangePrizes(uid, row, sequnce, config, out name, out przieId);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                var record = new LuckdrawModel
                {
                    MemberId = uid,
                    Key = Key,
                    Prize = przieId,
                    Money = 0,
                    Name = name,
                    Sequnce = sequnce,
                    Phone = UserInfo.Phone,
                    Status = 1
                };
                _repsitory.Add(record);
            }
            _repsitory.Update(data);
            return Json(new ResponseModel { ErrorCode = ErrorCode.None });
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
            var rows = _repsitory.Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == userId).ToList();


            if (rows.Count == 0)
            {
                var dict = new Dictionary<string, object>();
                dict["code"] = 1;
                dict["msg"] = "没有游戏数据";
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "没有游戏数据" });
            }

            var data = rows.Select(it => new
            {
                name = PrizeType(it.Prize),
                price = PrizeAmount(it.Prize),
                time = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(new ResponseModel
            {
                Data = data
            });
        }

        #region private

        /// <summary>
        /// 兑奖
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="prize"></param>
        /// <param name="squnce">顺序</param>
        /// <param name="config">活动配置</param>
        /// <param name="name">奖品名称</param>
        /// <param name="prizeId">奖品Id</param>
        private void ExchangePrizes(long userId, string prize, long squnce, MineSweeperConfig config, out string name, out int prizeId)
        {
            long id;
            var activityId = config.CouponActivityId;
            var productId = config.ExpresssProductId;
            decimal m;
            switch (prize)
            {
                case "A":
                    id = config.A;
                    m = 0.1m;
                    prizeId = 1;
                    name = "0.1元现金";
                    new MemberRepository(SqlConnectString).GiveMoney(userId, m, id, squnce);
                    break;

                case "B":
                    id = config.A;
                    m = 0.2m;
                    prizeId = 2;
                    name = "0.2元现金";
                    new MemberRepository(SqlConnectString).GiveMoney(userId, m, id, squnce);
                    break;

                case "C":
                    id = config.A;
                    m = 0.3m;
                    prizeId = 3;
                    name = "0.3元现金";
                    new MemberRepository(SqlConnectString).GiveMoney(userId, m, id, squnce);
                    break;

                case "D":
                    id = config.A;
                    m = 0.4m;
                    prizeId = 4;
                    name = "0.4元现金";
                    new MemberRepository(SqlConnectString).GiveMoney(userId, m, id, squnce);
                    break;

                case "E":
                    id = config.E;
                    m = 88;
                    prizeId = 5;
                    new MemberRepository(SqlConnectString).Give(userId, id, productId, m, squnce);
                    name = "88元体验金";
                    break;

                case "F":
                    id = config.E;
                    m = 888;
                    prizeId = 6;
                    new MemberRepository(SqlConnectString).Give(userId, id, productId, m, squnce);
                    name = "888元体验金";
                    break;

                case "G":
                    id = config.E;
                    m = 1888;
                    prizeId = 7;
                    new MemberRepository(SqlConnectString).Give(userId, id, productId, m, squnce);
                    name = "1888元体验金";
                    break;

                case "H":
                    name = "10元现金券";
                    id = config.H;
                    prizeId = 8;
                    CardCouponApi.UserGrant(userId, activityId, id);
                    break;

                case "I":
                    name = "5元现金券";
                    id = config.I;
                    prizeId = 9;
                    CardCouponApi.UserGrant(userId,  activityId, id);
                    break;

                case "J":
                    name = "8元现金券";
                    id = config.J;
                    prizeId = 10;
                    CardCouponApi.UserGrant(userId, activityId, id);
                    break;

                case "K":
                    name = "4元现金券";
                    id = config.K;
                    prizeId = 11;
                    CardCouponApi.UserGrant(userId,  activityId, id);
                    break;

                case "L":
                    name = "1%加息券";
                    id = config.L;
                    prizeId = 12;
                    CardCouponApi.UserGrant(userId,  activityId, id);
                    break;

                case "M":
                    name = "1.5%加息券";
                    id = config.M;
                    prizeId = 13;
                    CardCouponApi.UserGrant(userId, activityId, id);
                    break;

                case "N":
                    name = "1.5%加息券";
                    id = config.N;
                    prizeId = 14;
                    CardCouponApi.UserGrant(userId, activityId, id);
                    break;

                case "O":
                    name = "1.5%加息券";
                    id = config.O;
                    prizeId = 15;
                    CardCouponApi.UserGrant(userId, activityId, id);
                    break;

                case "P":
                    name = "1.5%加息券";
                    id = config.P;
                    prizeId = 16;
                    CardCouponApi.UserGrant(userId, activityId, id);
                    break;

                case "Q":
                    name = "1.5%加息券";
                    id = config.Q;
                    prizeId = 17;
                    CardCouponApi.UserGrant(userId, activityId, id);
                    break;

                default:
                    prizeId = 0;
                    name = "";
                    break;

            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        private MineSweeperConfig GetConfig()
        {
            var config = RedisManager.Get<MineSweeperConfig>("Activity:MineSweeper");
            if (config == null)
            {
                var model = _repsitory.GetActivity(Key);
                config = model.Config.Deserialize<MineSweeperConfig>();
                RedisManager.Set("Activity:MineSweeper", model.Config, 60 * 60);
                if (config == null)
                {
                    throw new Exception("Activity:MineSweeper 未配置");
                }
            }
            return config;
        }

        /// <summary>
        /// 领取顺序
        /// </summary>
        /// <returns></returns>
        private long GetSequnce()
        {
            return RedisManager.GetIncrement("Increment:MineSweeper");
        }

        private string PrizeType(int prize)
        {
            if (prize < 1)
            {
                return string.Empty;
            }
            if (prize < 5)
            {
                return "现金红包";
            }
            if (prize < 8)
            {
                return "体验金";
            }
            if (prize < 12)
            {
                return "现金券";
            }
            if (prize < 18)
            {
                return "加息券";
            }
            return string.Empty;
        }

        private string PrizeAmount(int prize)
        {
            switch (prize)
            {
                case 1:
                    return "0.1元";
                case 2:
                    return "0.2元";
                case 3:
                    return "0.3元";
                case 4:
                    return "0.4元";
                case 5:
                    return "88元";
                case 6:
                    return "888元";
                case 7:
                    return "1888元";
                case 8:
                    return "10元";
                case 9:
                    return "5元";
                case 10:
                    return "8元";
                case 11:
                    return "4元";
                case 12:
                    return "1%";
                case 13:
                case 14:
                case 15:
                    return "1.5%";

                case 16:
                case 17:
                    return "2%";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 检查 游戏结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static bool CheckResult(List<string> result)
        {
            var a = result.Count(it => it == "A");//0.1元现金
            if (a > 1)
            {
                return false;
            }

            var b = result.Count(it => it == "B");//0.2元现金
            if (b > 1)
            {
                return false;
            }

            var c = result.Count(it => it == "C");//0.3元现金
            if (c > 1)
            {
                return false;
            }

            var d = result.Count(it => it == "D");//0.4元现金
            if (d > 1)
            {
                return false;
            }

            var e = result.Count(it => it == "E");//88元体验金
            if (e > 2)
            {
                return false;
            }

            var f = result.Count(it => it == "F");//888元体验金
            if (f > 1)
            {
                return false;
            }
            var g = result.Count(it => it == "G");//1888元体验金
            if (g > 1)
            {
                return false;
            }

            var h = result.Count(it => it == "H");//10元现金券/满200元可用/限房金年宝使用
            if (h > 1)
            {
                return false;
            }

            var i = result.Count(it => it == "I");//5元现金券/满500元可用/限房金双季宝使用
            if (i > 3)
            {
                return false;
            }

            var j = result.Count(it => it == "J");//8元现金券/满800元可用/限房金季宝使用
            if (j > 2)
            {
                return false;
            }

            var k = result.Count(it => it == "K");//4元现金券/满1000元可用/限房金月宝使用
            if (k > 1)
            {
                return false;
            }

            var l = result.Count(it => it == "L");//1%加息券/满100元可用/除新手专享外的任意定期产品使用
            if (l > 3)
            {
                return false;
            }

            var m = result.Count(it => it == "M");//1.5%加息券/满100元可用/限房金季宝、
            if (m > 1)
            {
                return false;
            }
            var n = result.Count(it => it == "N");//1.5%加息券/满100元可用/限房金双季宝
            if (n > 1)
            {
                return false;
            }

            var o = result.Count(it => it == "O");//1.5%加息券/满100元可用/限房金年宝
            if (o > 1)
            {
                return false;
            }

            var p = result.Count(it => it == "P");//2%加息券/满100元可用/限房金双季宝使用
            if (p > 2)
            {
                return false;
            }
            var q = result.Count(it => it == "Q");//2%加息券/满100元可用/限房金年宝使用
            if (q > 1)
            {
                return false;
            }
            return true;
        }

        private static string DisplayName(string prize)
        {
            switch (prize)
            {
                case "A":
                    return "0.1元现金";
                case "B":
                    return "0.2元现金";
                case "C":
                    return "0.3元现金";
                case "D":
                    return "0.4元现金";
                case "E":
                    return "88元体验金";
                case "F":
                    return "888元体验金";
                case "G":
                    return "1888元体验金";
                case "H":
                    return "10元现金券";
                case "I":
                    return "5元现金券";
                case "J":
                    return "8元现金券";
                case "K":
                    return "4元现金券";
                case "L":
                    return "1%加息券";
                case "M":
                case "N":
                case "O":
                    return "1.5%加息券";
                case "P":
                case "Q":
                    return "2%加息券";

            }
            return null;
        }

        #endregion
    }
    /// <summary>
    /// 游戏结果
    /// </summary>
    public class MineSweeperModel
    {
        public List<string> Rows { get; set; }

        public int MiSeconds { get; set; }
    }

    /// <summary>
    /// 活动配置
    /// </summary>
    public class MineSweeperConfig
    {
        /// <summary>
        /// 现金
        /// </summary>
        public long A { get; set; }

        /// <summary>
        /// 体验金
        /// </summary>
        public long E { get; set; }

        /// <summary>
        /// 10元现金券
        /// </summary>
        public long H { get; set; }

        /// <summary>
        /// 5元现金券
        /// </summary>
        public long I { get; set; }

        /// <summary>
        /// 8元现金券
        /// </summary>
        public long J { get; set; }

        /// <summary>
        /// 4元现金券
        /// </summary>
        public long K { get; set; }

        /// <summary>
        /// 1%加息券
        /// </summary>
        public long L { get; set; }

        /// <summary>
        /// 1.5%加息券
        /// </summary>
        public long M { get; set; }

        /// <summary>
        /// 1.5%加息券
        /// </summary>
        public long N { get; set; }

        /// <summary>
        /// 1.5%加息券
        /// </summary>
        public long O { get; set; }

        /// <summary>
        /// 2%加息券
        /// </summary>
        public long P { get; set; }

        /// <summary>
        /// 2%加息券
        /// </summary>
        public long Q { get; set; }


        /// <summary>
        /// 卡券活动Id
        /// </summary>
        public long CouponActivityId { get; set; }

        /// <summary>
        /// 体验金产品Id
        /// </summary>
        public long ExpresssProductId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}