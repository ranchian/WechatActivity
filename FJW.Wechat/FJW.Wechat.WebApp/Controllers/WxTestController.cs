using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FJW.Wechat.Activity.Controllers;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Wx;

namespace FJW.Wechat.WebApp.Controllers
{
    [WAuthorize]
    public class WxTestController : ActivityController
    {
        private const string Key = "Test";
        // GET: WxTest
        public ActionResult Index()
        {
            var openId = UserInfo.OpenId;

            var repository = new ActivityRepository( DbName , MongoHost);
            var row = repository.Query<WxShareModel>(it => it.Key == Key && it.OpenId == openId).FirstOrDefault();
            if (row== null)
            {
                row = new WxShareModel
                {
                    Key = Key,
                    OpenId = openId,
                    UserId = UserInfo.Id,
                    HeadimgUrl = UserInfo.WxUserInfo.HeadimgUrl,
                    NickName = UserInfo.WxUserInfo.NickName,
                    CreateTime = DateTime.Now
                };
                repository.Add(row);
            }
            var rows = repository.Query<WxShareSupportModel>(it => it.RowId == row.ID.ToString()).ToList();
            ViewBag.HeadimgUrl = row.HeadimgUrl;
            ViewBag.NickName = row.NickName;
            ViewBag.RowId = row.ID;
            ViewBag.Rows = rows;
            return View();
        }



        public ActionResult Support(string r)
        {
            var openId = UserInfo.OpenId;
            Guid gid;
            if (! Guid.TryParse(r, out gid))
            {
                return Content("无效的连接");
            }
            var repository = new ActivityRepository(DbName, MongoHost);
            var row = repository.Query<WxShareModel>(it => it.ID == gid).FirstOrDefault();
            if (row == null)
            {
                return Content("无效的分享数据");
            }
            var rows = repository.Query<WxShareSupportModel>(it => it.RowId == r).ToList();
            if (!rows.Any(it=>it.OpenId == openId))
            {
                repository.Add(new WxShareSupportModel
                {
                    OpenId = openId,
                    RowId = r,
                    UserId = UserInfo.Id,
                    HeadimgUrl = UserInfo.WxUserInfo.HeadimgUrl,
                    NickName = UserInfo.WxUserInfo.NickName,
                    CreateTime = DateTime.Now
                });
                rows = repository.Query<WxShareSupportModel>(it => it.RowId == r).ToList();
            }
            ViewBag.HeadimgUrl = row.HeadimgUrl;
            ViewBag.NickName = row.NickName;
            ViewBag.RowId = row.ID;
            ViewBag.Rows = rows;
            return View();
        }
    }
}