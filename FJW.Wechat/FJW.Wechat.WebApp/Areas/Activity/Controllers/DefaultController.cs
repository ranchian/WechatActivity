using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Models;
using System.Web.Mvc;
using System.Web.Configuration;
using System;
using FJW.Wechat.WebApp.Areas.Activity.Models;
using Newtonsoft.Json;
using System.Linq;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    public class DefaultController : ActivityController
    {

        private readonly string _mongoHost;
        private readonly string _dbName;
        private readonly string _sqlConnectString;
        public DefaultController()
        {
            _mongoHost = WebConfigurationManager.AppSettings["MongoHost"];
            _dbName = WebConfigurationManager.AppSettings["DbName"];
            _sqlConnectString = WebConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }

        /// <summary>
        /// 自己玩
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Game(string key)
        {
            var acty = new ActivityRepository(_dbName, _mongoHost).GetActivity(key);
            if (acty == null || DateTime.Now < acty.StartTime || DateTime.Now > acty.EndTime || string.IsNullOrEmpty(acty.GameUrl))
            {
                return Redirect("http://www.fangjinnet.com/wx/download/index");
            }
            return Redirect(acty.GameUrl);
        }


        public ActionResult LoginResult(string t , string key)
        {
            if (string.IsNullOrEmpty(t))
            {
                return Content("无效的请求");
            }
            var mberRepository = new MemberRepository(_sqlConnectString);
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
            var repositry = new ActivityRepository(_dbName, _mongoHost);
            var model = repositry.GetById(id)?? new RecordModel();

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
            var acty = new ActivityRepository(_dbName, _mongoHost).GetActivity(key);
            if (acty == null || DateTime.Now < acty.StartTime || DateTime.Now > acty.EndTime || string.IsNullOrEmpty(acty.GameUrl))
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
                new ActivityRepository(_dbName, _mongoHost).Add(model);
                id = model.RecordId;
                if (!string.IsNullOrEmpty(id))
                {
                    SetSelfGameRecordId(id);
                }
                else
                {
                    SetHelpGamRecordId(fid, id);
                }

                return Json(new ResponseModel { IsSuccess = true, Data =  new { id, islogin = UserInfo.Id > 0 } });
            }
            return Json(new ResponseModel { IsSuccess = true, Data = new { id, islogin = UserInfo.Id > 0 } });
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="key"></param>
        /// <param name="score"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Over(ScoreModel model)
        {
            var fid = model.Fid ?? string.Empty;
            var score = model.Score;
            var id = string.IsNullOrEmpty(fid) ? GetSelfGameRecordId() : GetHelpGamRecordId(fid);
            if (!string.IsNullOrEmpty(id))
            {
                var repositry = new ActivityRepository(_dbName, _mongoHost);
                var m = repositry.GetById(id);
                if (m.Status != 0)
                {
                    return Json(new ResponseModel { IsSuccess = false, Message = "已经领取过奖励了" });
                }
                if (m.Score < score)
                {
                    m.Score = score;
                    repositry.Update(m);
                   return Json(new ResponseModel { IsSuccess = true, Message = "" });
                }
                return Json(new ResponseModel { IsSuccess = true, Message = "", Data = new { id, islogin = UserInfo.Id > 0 } });
            }
            return Json(new ResponseModel { IsSuccess = false, Message = "没有该数据", Data = new { id, islogin = UserInfo.Id > 0 } });
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
                return Json(new ResponseModel { IsSuccess = false, Message = "未登录" });
            }
            var acty = new ActivityRepository(_dbName, _mongoHost).GetActivity(key);
            if (acty == null || DateTime.Now < acty.StartTime || DateTime.Now > acty.EndTime || string.IsNullOrEmpty(acty.GameUrl))
            {
                return Redirect("http://www.fangjinnet.com/wx/download/index");
            }
            var id = string.IsNullOrEmpty(fid) ? GetSelfGameRecordId() : GetHelpGamRecordId(fid);
            if (!string.IsNullOrEmpty(id))
            {
                var repositry = new ActivityRepository(_dbName, _mongoHost);
                var model = repositry.GetById(id);
                if (model.Status != 0)
                {
                    return Json(new ResponseModel { IsSuccess = false, Message = "已经领取过奖励了" });
                }
                var records = repositry.Query<RecordModel>(it => it.MemberId == UserInfo.Id && it.Status > 0).ToArray();
                if (records.Length > 0)
                {
                    return Json(new ResponseModel { IsSuccess = false, Message = "已经领取过奖励了" });
                }
                
                var mberRepository = new MemberRepository(_sqlConnectString);
                if (string.IsNullOrEmpty(fid)) {
                    var r = mberRepository.Give(UserInfo.Id, acty.RewardId, acty.ProductId, model.Score, 0);
                    model.MemberId = UserInfo.Id;
                    model.Result = r;
                    model.Status = 1;
                    model.LastUpdateTime = DateTime.Now;
                    repositry.Update(model);
                    return Json(new ResponseModel { IsSuccess = true, Message = "" });
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
                return Json(new ResponseModel { IsSuccess = false, Message = "未登录" }, JsonRequestBehavior.AllowGet);
            }
            return new EmptyResult();
        }
    }
}