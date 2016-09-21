using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Models;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    public class TurntableController : ActivityController
    {
        // GET: Activity/Turntable
        private const string GameKey = "turntable";
        public ActionResult Index()
        {
            var state = ActivityState(GameKey);
            if (state != 0)
            {
                return Redirect("http://www.fangjinnet.com/wx/download/index");
            }
            return Redirect(ActivityModel.GameUrl);
        }

        public ActionResult Game()
        {
            var state = ActivityState(GameKey);
            if (state != 0)
            {
                return Redirect("http://www.fangjinnet.com/wx/download/index");
            }
            return Redirect("/#");
        }

        public ActionResult Play()
        {
            if (!Request.IsAjaxRequest())
            {
                return Json(new ResponseModel { ErrorCode = 1, Message = "无效的请求方式" });
            }
            var state = ActivityState(GameKey);
            if (state != 0)
            {
                return Json(new ResponseModel { ErrorCode = 1, Message = "您来的时间不对" });
            }
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = 2 , Message = "未登录"});
            }
            
            //TODO: 统计次数

            int prize;
            decimal money;
            string name;
            var sequnce = Luckdraw(out prize, out money, out name);
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

            //TODO: 发送奖励， 更改状态, 更改使用次数

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