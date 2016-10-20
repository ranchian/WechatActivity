using System;
using System.Linq;
using System.Web.Mvc;

using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Models;
using FJW.Wechat.WebApp.Areas.Activity.Models;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    public class DefaultController : ActivityController
    {
        /// <summary>
        /// 自己玩
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Game(string key)
        {
            var state = ActivityState(key);
            if (state == 0)
            {
                return Redirect(ActivityModel.GameUrl);
            }
            return Redirect("http://www.fangjinnet.com/wx/download/index");
        }

        public ActionResult LoginResult(string t, string key)
        {
            if (string.IsNullOrEmpty(t))
            {
                return Content("无效的请求");
            }
            var mberRepository = new MemberRepository(SqlConnectString);
            var m = mberRepository.GetMemberInfo(t);
            if (m == null)
            {
                return Content("无效的登录结果");
            }
            UserInfo.Id = m.MemberId;
            UserInfo.Token = t;
            SetLoginInfo(UserInfo);

            //Logger.Debug("MemberId:" + m.MemberId);
            //Logger.Debug("UserInfo:" + JsonConvert.SerializeObject(Session["SessionUserInfo"]));
            //var id = string.IsNullOrEmpty(fid) ? GetSelfGameRecordId() : GetHelpGamRecordId(fid);
            var id = GetSelfGameRecordId();
            var repositry = new ActivityRepository(DbName, MongoHost);
            var model = repositry.GetById(id) ?? new RecordModel();

            return Redirect(string.Format("/html/{0}/download.html?key={0}&amount={1}", key, model.Score));
        }

        /// <summary>
        /// 玩游戏
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Play(string key, string fid = "")
        {
            var state = ActivityState(key);
            if (state != 0)
            {
                return Redirect("http://www.fangjinnet.com/wx/download/index");
            }

            var id = string.IsNullOrEmpty(fid) ? GetSelfGameRecordId() : GetHelpGamRecordId(fid);
            if (string.IsNullOrEmpty(id))
            {
                var model = new RecordModel
                {
                    RecordId = Guid.NewGuid().ToString(),
                    InvitedCount = 0,
                    Total = 0,
                    FriendGameId = fid,
                    Status = 0,
                    Score = 0,
                    Seconds = 0,
                    JoinType = string.IsNullOrEmpty(fid) ? 2 : 1,
                    Key = key,
                    MemberId = 0,
                    WechatId = string.Empty
                };
                new ActivityRepository(DbName, MongoHost).Add(model);
                id = model.RecordId;
                if (!string.IsNullOrEmpty(id))
                {
                    SetSelfGameRecordId(id);
                }
                else
                {
                    SetHelpGamRecordId(fid, id);
                }

                return Json(new ResponseModel {   Data = new { id, islogin = UserInfo.Id > 0 } });
            }
            return Json(new ResponseModel {  Data = new { id, islogin = UserInfo.Id > 0 } });
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Over(ScoreModel model)
        {
            var fid = model.Fid ?? string.Empty;
            var score = model.Score;
            var id = string.IsNullOrEmpty(fid) ? GetSelfGameRecordId() : GetHelpGamRecordId(fid);
            if (!string.IsNullOrEmpty(id))
            {
                var repositry = new ActivityRepository(DbName, MongoHost);
                var m = repositry.GetById(id);
                if (m.Status != 0)
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "已经领取过奖励了" });
                }
                if (m.Score < score)
                {
                    if (score > 6000)
                    {
                        score = 6001;
                    }
                    m.Score = score;
                    repositry.Update(m);
                    return Json(new ResponseModel { Message = "" });
                }
                return Json(new ResponseModel {  Message = "", Data = new { id, islogin = UserInfo.Id > 0 } });
            }
            return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "没有该数据", Data = new { id, islogin = UserInfo.Id > 0 } });
        }

        /// <summary>
        /// 领取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Receive(string key, string fid = "")
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel {ErrorCode  = ErrorCode.NotLogged, Message = "未登录" });
            }
            var state = ActivityState(key);
            if (state != 0)
            {
                return Redirect("http://www.fangjinnet.com/wx/download/index");
            }
            var id = string.IsNullOrEmpty(fid) ? GetSelfGameRecordId() : GetHelpGamRecordId(fid);
            if (!string.IsNullOrEmpty(id))
            {
                var repositry = new ActivityRepository(DbName, MongoHost);
                var model = repositry.GetById(id);
                if (model.Status != 0)
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "已经领取过奖励了" });
                }
                var records = repositry.Query<RecordModel>(it => it.MemberId == UserInfo.Id && it.Status > 0).ToArray();
                if (records.Length > 0)
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "已经领取过奖励了" });
                }

                var mberRepository = new MemberRepository(SqlConnectString);
                if (string.IsNullOrEmpty(fid))
                {
                    var r = mberRepository.Give(UserInfo.Id, ActivityModel.RewardId, ActivityModel.ProductId, model.Score, 0);
                    model.MemberId = UserInfo.Id;
                    model.Result = r;
                    model.Status = 1;
                    model.LastUpdateTime = DateTime.Now;
                    repositry.Update(model);
                    return Json(new ResponseModel { Message = "" });
                }
            }

            return new EmptyResult();
        }

        /// <summary>
        /// 结果
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Result(string key)
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode  = ErrorCode.NotLogged, Message = "未登录" });
            }
            return new EmptyResult();
        }
    }
}