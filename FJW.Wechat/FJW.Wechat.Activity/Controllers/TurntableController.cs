using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using FJW.Unit;
using FJW.Wechat.Data;

namespace FJW.Wechat.Activity.Controllers
{

    public class TurntableController : ActivityController
    {
        private const string GameKey = "turntable";

        /// <summary>
        /// 获取抽奖次数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetActTimes()
        {
            int mOnline = 0, mTimes = 1;

            // 判断用户是否登陆 如果没登陆则返回数据 online=0,times=0
            if (UserInfo.Id < 1)
                return Json(new { online = mOnline, times = 0 });

            ActTimes(ref mOnline, ref mTimes);
            return Json(new { online = mOnline, times = mTimes });
        }

        /// <summary>
        /// 获取剩余抽奖次数
        /// </summary>
        /// <param name="mOnline"></param>
        /// <param name="mTimes"></param>
        private void ActTimes(ref int mOnline, ref int mTimes)
        {
            var repository = new MemberRepository(SqlConnectString);

            var awardCnt = repository.GetMemberAward(UserInfo.Id);
            if (awardCnt > 0)
            {
                mTimes = awardCnt;
                if (mTimes <= 0) mTimes = 0;
            }
            else
                mTimes = 0;
            mOnline = 1;
        }

        /// <summary>
        /// 获取奖项列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetActResult(int type = 0)
        {
            try
            {


                var state = ActivityState(GameKey);
                if (state != 0)
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "活动未开始或已结束" });
                }
                if (type == 1 && UserInfo.Id < 1)
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "未登录" });
                }
                var sqlRepository = new MemberRepository(SqlConnectString);
                var dt = sqlRepository.GetRecord(type, UserInfo.Id, GameKey);
                return Json(new ResponseModel { ErrorCode = 0, Message = "", Data = dt.ToJson() });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new ResponseModel {ErrorCode = ErrorCode.NotLogged, Message = "Error"});
            }
        }

        /// <summary>
        /// 用户获奖统计
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMemberSum()
        {
            var state = ActivityState(GameKey);
            if (state != 0)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "活动未开始或已结束" });
            }
            if (UserInfo.Id > 0)
            {
                //
                var sqlRepository = new MemberRepository(SqlConnectString);
                return Json(new ResponseModel { ErrorCode = 0, Message = "", Data = sqlRepository.GetRocordSum(UserInfo.Id) });
            }
            return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "用户未登录" });

        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Play()
        {
            var state = ActivityState(GameKey);
            if (state != 0)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "活动未开始或已结束" });
            }
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "未登录" });
            }

            try
            {
                int mOnline = 0, mTimes = 1;
                ActTimes(ref mOnline, ref mTimes);

                if (mTimes > 0)
                {
                    int prize;
                    string name;
                    decimal money;
                    var sequnce = Luckdraw(out prize, out money, out name);

                    //异步执行插入操作
                    Task.Factory.StartNew(() =>
                    {

                        //添加数据库
                        var sqlRepository = new MemberRepository(SqlConnectString);
                        sqlRepository.AddRecord(UserInfo.Id, name, prize, money, GameKey, sequnce);

                        //发送奖励
                        sqlRepository.GiveMoney(UserInfo.Id, money, 8, sequnce);

                        //mongoDb中添加数据
                        var record = new LuckdrawModel
                        {
                            Sequnce = sequnce,
                            Key = GameKey,
                            MemberId = UserInfo.Id,
                            Prize = prize,
                            Money = money,
                            Name = name,
                            Status = 0,
                        };
                        var repository = new ActivityRepository(DbName, MongoHost);
                        repository.Add(record);

                        //推送消息
                        /*string msg = string.Format("尊敬的房金网会员，您参加的幸运大转盘 Iphone 7 plus免费送活动，抽中了{0}，感谢您的参与", name);
                        var userInfo = sqlRepository.GetMemberInfo(UserInfo.Token);
                        if (userInfo != null)
                        {
                            var dic = new Dictionary<string, string>
                            {
                                {"Intro",msg},
                                {"ReceiveType", "1"},
                                {"PhoneList",userInfo.Phone},
                                {"PushType","4"},
                                {"Url",""},
                                {"ProductId",""}
                            };

                            new PushHelper().PushMsg(dic);
                        }*/
                    });

                    return Json(new ResponseModel {  Message = "", Data = new { name = name, prize = prize, money = money } });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return Json(new ResponseModel());
        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <param name="prize">奖品</param>
        /// <param name="money">奖金</param>
        /// <param name="name">描述</param>
        /// <returns></returns>
        private static long Luckdraw(out int prize, out decimal money, out string name)
        {
            //prize = 0;
            money = 0;
            var l = RedisManager.GetIncrement("Increment:" + GameKey);
            if (l % 400 == 0)
            {
                prize = 1;
                name = "iphone 7 plus(128G)";
                return l;
            }
            if (l % 200 == 0)
            {
                prize = 2;
                name = "apple watch2";
                return l;
            }
            if (l % 60 == 0)
            {
                prize = 3;
                money = 800;
                name = "800元现金";
                return l;
            }
            if (l % 10 == 0)
            {
                prize = 4;
                money = 80;
                name = "80元现金";
                return l;
            }
            if (l % 5 == 0)
            {
                prize = 5;
                money = 10;
                name = "10元现金";
                return l;
            }
            prize = 6;
            money = 5;
            name = "5元现金";
            return l;
        }
    }
}