using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using Quartz;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 拆情侣
    /// </summary>
    [CrossDomainFilter]
    public class SplitloversController : ActivityController
    {
        private const string Key = "splitlovers";
        //private const string RedisKey = "Splitlovers:Total";
        private readonly ActivityRepository _repsitory;

        public SplitloversController()
        {
            _repsitory = new ActivityRepository(DbName, MongoHost);
        }

        /// <summary>
        /// 游戏开始
        /// </summary>
        /// <returns></returns>
        public ActionResult State()
        {
            var dt = DateTime.Now;
            var userId = UserInfo.Id;
            if (dt <= new DateTime(2016, 12, 9, 10, 0, 0) && userId != 27329 && userId != 27331 && userId != 255925)
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

            if (dt > new DateTime(2016, 12, 27, 0, 0, 0))
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

                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "未登录" });
            }
            bool isPlay;
            RecordModel rec;
            var res = Game(userId, out isPlay, out rec);
            if (isPlay)
            {
                return Json(res);
            }
            return Json(new ResponseModel(ErrorCode.None));
        }

        /// <summary>
        /// 判断当日是否玩过游戏
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isPlay"></param>
        /// <param name="rec"></param>
        /// <returns></returns>
        public string Game(long userId, out bool isPlay, out RecordModel rec)
        {
            try
            {
                var date = DateTime.Now.Date;
                rec =
                _repsitory.Query<RecordModel>(
                    it =>
                        it.Key == Key && it.MemberId == userId &&
                        it.CreateTime >= date && it.CreateTime < date.AddDays(1)).FirstOrDefault();
                if (rec != null && rec.Total > 0)
                {
                    isPlay = true;
                    return "";
                }
                isPlay = false;
                return "";
            }
            catch (Exception ex)
            {
                isPlay = true;
                rec = null;
                Logger.Dedug("uid:{0} Game:{1}", userId, ex.ToString());
                return "";
            }
        }


        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Over(SplitloversModel result)
        {
            Logger.Dedug("result:{0}", result.Score);
            var config = GetConfig();
            var uid = UserInfo.Id;

            var dt = DateTime.Now;
            if (dt < config.StartTime && uid != 27329 && uid != 27331 && uid != 255925)
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

            if (dt > config.EndTime)
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

            if (result.Score < 0)
            {
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.NotVerified,
                    Message = "游戏数据不正确"
                });
            }

            //未登录
            if (uid < 1)
                return Json(NotUser(result.Score));
            //已登录
            if (uid > 0)
                return Json(HasUser(uid, result.Score));



            //var dataId = GetSelfGameRecordId();

            ////若提交了3次游戏则对Total赋值
            //if (data != null && !dataId.IsNullOrEmpty())
            //{
            //    data = _repsitory.GetById(dataId);

            //    //提交游戏数据时间间隔验证
            //    var numlis = data.Data.Deserialize<List<int>>();
            //    double diffTime = (DateTime.Now - data.CreateTime).TotalSeconds;
            //    //和初始设定提交时间相差小于2秒
            //    if (data.Result < 0 && Math.Abs(diffTime - numlis[data.Total]) <= 2 && data.Score >= 0)
            //    {
            //        data.Total += 1; //游戏次数
            //        data.Score += result.Score; //游戏成绩
            //        if (data.Total == 3)
            //            data.Result = data.Score;
            //    }
            //    else
            //    {
            //        return Json(new ResponseModel { ErrorCode = ErrorCode.None, Message = "提交时间错误" });
            //    }

            //    data.LastUpdateTime = DateTime.Now;
            //    _repsitory.Update(data);
            //}

            Logger.Dedug("uid:{1} UserScore:{0}", result.Score, uid);
            return Json(new ResponseModel { ErrorCode = ErrorCode.None });
        }

        /// <summary>
        /// 用户未登录存储临时数据
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public ResponseModel NotUser(int score)
        {
            var recordId = GetSelfGameRecordId();
            if (score == 0)
                return new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "未玩游戏或成绩为0" };
            //存储最大成绩
            if (!string.IsNullOrEmpty(recordId))
            {
                var data = _repsitory.Query<RecordModel>(it =>
                    it.Key == Key && it.RecordId == recordId).FirstOrDefault();
                maxScore(data, score);
            }
            else
            {
                var gameData = new RecordModel
                {
                    RecordId = Guid.NewGuid().ToString(),
                    Result = score,
                    Key = Key,
                    Score = score,
                    Status = 0,
                    Total = 1,
                    CreateTime = DateTime.Now
                };
                SetSelfGameRecordId(gameData.RecordId);
                _repsitory.Add(gameData);
            }
            return new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "未登录" };
        }

        /// <summary>
        /// 用户登录同步数据
        /// </summary>
        public ResponseModel HasUser(long uid, int score)
        {
            bool isPlay;
            RecordModel data = null;
            var res = Game(uid, out isPlay, out data);
            if (isPlay)
            {
                maxScore(data, score);
                SetSelfGameRecordId(null);
                return new ResponseModel { ErrorCode = ErrorCode.None, Message = "游戏数据已更新" };
            }
            //登陆后获取临时成绩
            var recordId = GetSelfGameRecordId();

            //该记录是否有用户
            data = _repsitory.Query<RecordModel>(it =>
                it.Key == Key && it.RecordId == recordId).FirstOrDefault();
            Logger.Dedug("临时记录 Phone:{0}  Data{1}", UserInfo.Phone, data.ToJson());
            if (data == null)
            {
                if (score > 0)
                {
                    var gameData = new RecordModel
                    {
                        RecordId = Guid.NewGuid().ToString(),
                        Result = score,
                        Key = Key,
                        MemberId = UserInfo.Id,
                        Phone = UserInfo.Phone,
                        Score = score,
                        Status = 0,
                        Total = 1,
                        CreateTime = DateTime.Now
                    };
                    SetSelfGameRecordId(gameData.RecordId);
                    _repsitory.Add(gameData);
                    Logger.Dedug("Phone:{0} Message:游戏数据已更新 ", UserInfo.Phone, score);
                    return new ResponseModel { ErrorCode = ErrorCode.None, Message = "游戏数据已更新" };
                }
                Logger.Dedug("Phone:{0} Message:没有用户数据 ", UserInfo.Phone, score);
                return new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "没有用户数据" };
            }

            if (data.MemberId != 0)
                return new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "成绩已提交" };

            data.MemberId = uid;
            data.Phone = UserInfo.Phone;
            data.LastUpdateTime = DateTime.Now;

            maxScore(data, score);
            _repsitory.Update(data);

            Logger.Dedug("同步数据 Phone:{0} Score:{1} DATA:{2}", UserInfo.Phone, score, data.ToJson());
            return new ResponseModel { ErrorCode = ErrorCode.None };
        }

        //获取最高分数
        public void maxScore(RecordModel data, int score)
        {
            if (data.Result < score)
            {
                data.LastUpdateTime = DateTime.Now;
                data.Score = score;
                data.Result = score;
                data.Total = data.Total + 1;
                _repsitory.Update(data);
            }
        }

        //获取配置
        private static SplitloversConfig GetConfig()
        {
            return JsonConfig.GetJson<SplitloversConfig>("Config/activity.splitloversvalue.json");
        }

        /// <summary>
        /// 总排行榜
        /// </summary>
        /// <returns></returns>
        public ActionResult Total()
        {
            try
            {




                List<RecordModel> data = new List<RecordModel>();
                var num = 0;
                int cnt;
                var date = DateTime.Now.Date;
                data = new ActivityRepository(Config.ActivityConfig.DbName, Config.ActivityConfig.MongoHost).QueryDesc<RecordModel, int>(it => it.Key == Key
                && it.MemberId != 0 && it.Phone != "" && it.Result != 0 && it.CreateTime >= date && it.CreateTime < date.AddDays(1)
                , it => it.Result, 20, 0, out cnt).ToList();

                object resData = data.Select(it => new
                {
                    Sequnce = ++num,
                    Phone = StringHelper.CoverPhone(it.Phone)
                }).ToList();

                return Json(resData);



            }
            catch (Exception ex)
            {
                Logger.Dedug("Total:{0}", ex.ToString());
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other });
            }
        }



        /// <summary>
        /// 用户排行榜
        /// </summary>
        /// <returns></returns>        
        public ActionResult RankingList()
        {
            var userId = UserInfo.Id;
            if (userId <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "未登录" });
            int cnt;
            int num = 0;
            var date = DateTime.Now.Date;
            var data = _repsitory.QueryDesc<RecordModel, int>(
                it => it.Key == Key && it.Result != 0 && it.MemberId != 0 && it.CreateTime >= date && it.CreateTime < date.AddDays(1),
                it => it.Result, 1000000, 1, out cnt).Select(it => new
                {
                    Num = ++num,
                    Record = it.Result,
                    Id = it.MemberId
                }).FirstOrDefault(it => it.Id == userId);
            if (data == null)
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "未进排名" });

            return Json(data);




        }



    }
    //奖励实体
    public class SplitloversConfig
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public long ActivityId { get; set; }

        public long ExperienceId { get; set; }

        public long RateCouponA { get; set; }

        public long RateCouponB { get; set; }

        public long MoneyA { get; set; }

        public long MoneyB { get; set; }

        public int Hour { get; set; }

        public int Minute { get; set; }

        public int GiveTimeDiff { get; set; }
        
    }
    public class SplitloversModel
    {
        public int Score { get; set; }

    }

    //排行榜实体
    public class SplitloversRow
    {
        public string Phone { get; set; }
        public int Sequnce { get; set; }
    }
}