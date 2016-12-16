using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Base;
using FJW.Wechat.WebApp.Models;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    /// <summary>
    /// 拆情侣
    /// </summary>
    [CrossDomainFilter]
    public class SplitloversController : ActivityController
    {
        private const string Key = "splitlovers";
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
            if (dt <= new DateTime(2016, 12, 19, 10, 0, 0) && userId != 27329 && userId != 27331 && userId != 255925)
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
                return Json(new ResponseModel(ErrorCode.NotLogged));
            }
            bool isPlay;
            RecordModel rec;
            var res = Game(userId, out isPlay, out rec);
            if (isPlay)
            {
                return Json(res);
            }

            //开始
            var numlis = GameNum();

            RecordModel data = null;
            data = new RecordModel
            {
                RecordId = Guid.NewGuid().ToString(),
                MemberId = userId,
                Total = 0,
                Key = Key,
                Score = 0,
                Status = 0,
                Result = -1,
                Data = numlis.ToJson(),
                CreateTime = dt,
                LastUpdateTime = dt,
                Phone=UserInfo.Phone
            };
            _repsitory.Add(data);
            SetSelfGameRecordId(data.RecordId);
            Logger.Dedug("SessionId:{1} SetSelfGameRecordId:{0}", data.RecordId, Session.SessionID);
            return Json(new ResponseModel { Data = numlis });
        }

        /// <summary>
        /// 判断当日是否玩过游戏
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isPlay"></param>
        /// <param name="rec"></param>
        /// <returns></returns>
        public ResponseModel Game(long userId, out bool isPlay, out RecordModel rec)
        {
            rec = _repsitory.Query<RecordModel>(it => it.Key == Key && it.MemberId == userId && it.CreateTime.ToShortDateString() == DateTime.Now.ToShortDateString()).FirstOrDefault();
            if (rec != null && rec.Total < 0)
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 2,
                    ["msg"] = "您今日已经拆过情侣，给别人留点机会吧。"
                };
                isPlay = true;
                return new ResponseModel { ErrorCode = ErrorCode.Other, Data = dict, Message = "今日已经参加" };
            }
            isPlay = false;
            return new ResponseModel();
        }

        //返回3个随机数
        public List<int> GameNum()
        {
            List<int> numLis = new List<int>();
            Random r = new Random();
            numLis.Add(r.Next(1, 10));
            numLis.Add(r.Next(11, 20));
            numLis.Add(r.Next(21, 30));
            return numLis;
        }

        [OutputCache(Duration = 10)]
        public ActionResult Record()
        {
            int cnt;
            var data = _repsitory.QueryDesc<LuckdrawModel, DateTime>(it => it.Key == Key, it => it., 10, 0, out cnt).Select(it => new
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
        /// 游戏结束
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Over(SplitloversModel result)
        {
            var uid = UserInfo.Id;
            var dt = DateTime.Now;
            if (dt < new DateTime(2016, 12, 19, 10, 0, 0) && uid != 27329 && uid != 27331 && uid != 255925)
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
            if (result.Score < 0)
            {
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.NotVerified,
                    Message = "游戏数据不正确"
                });
            }
            RecordModel data = null;
            if (uid > 0)
            {
                bool isPlay;
                var res = Game(uid, out isPlay, out data);
                if (isPlay)
                {
                    return Json(res);
                }
            }


            var dataId = GetSelfGameRecordId();

            //若进行了3次游戏则对Total赋值
            if (data != null && !dataId.IsNullOrEmpty())
            {
                data = _repsitory.GetById(dataId);

                //提交游戏数据时间间隔验证
                var numlis = data.Data.ToList();
                float diffTime = float.Parse((DateTime.Now - data.CreateTime).ToString());
                //和初始设定提交时间相差小于1秒
                if (data.Result < 0 && Math.Abs(diffTime - numlis[data.Total]) <= 1 && data.Score >= 0)
                {
                    data.Total += 1; //游戏次数
                    data.Score += result.Score; //游戏成绩
                    if (data.Total == 3)
                        data.Result = data.Score;
                }

                data.LastUpdateTime = DateTime.Now;
                _repsitory.Update(data);
            }

            Logger.Dedug("uid:{1} SetSelfGameRecordId:{0}", uid, Session.SessionID);
            return
                Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.None,
                    Data = data?.Total ?? 0
                });
        }


        /// <summary>
        /// 排行榜
        /// </summary>
        /// <returns></returns>
        public ActionResult RankingList()
        {
            var userId = UserInfo.Id;
            int cnt;
            int num = 0;
            var data = _repsitory.QueryDesc<RecordModel,int>(it => it.Key == Key, it => it.Result, 20, 0, out cnt).Select(it => new
            {
                name =  ++num+".",
                price = it.Result,
                
                phone = StringHelper.CoverPhone(it.Phone)
            });
            if (userId < 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = data });


        }
    }

    public class SplitloversModel
    {
        public int Score { get; set; }

    }
}