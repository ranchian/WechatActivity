using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using FJW.Unit;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;

using static FJW.Wechat.Data.SqlDataRepository;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 三月换购活动
    /// </summary>
    [CrossDomainFilter]
    public class ExchangeBuyController : ActivityController
    {

        //所有的奖品
        private readonly List<RealThing> realThingsLis;
        private readonly SqlDataRepository _respoRepository;
        public ExchangeBuyController()
        {
            //获取奖品
            _respoRepository = new SqlDataRepository(SqlConnectString);
            realThingsLis = _respoRepository.GetPrize();
        }

        /// <summary>
        /// 活动开始
        /// </summary>
        /// <returns></returns>
        public ResponseModel Start()
        {
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
                return new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动未开始"
                };
            }

            if (dt > config.EndTime)
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = 5,
                    ["msg"] = "活动已结束"
                };
                return new ResponseModel
                {
                    ErrorCode = ErrorCode.Other,
                    Data = dict,
                    Message = "活动已结束"
                };
            }

            if (UserInfo.Id < 1)
            {
                return new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "未登录" };
            }
            return new ResponseModel { ErrorCode = ErrorCode.None };
        }
        /// <summary>
        /// 兑换
        /// </summary>
        /// <returns></returns>
        public ActionResult Exchange(int prizeId = 0, int productMappingDetailId = 0)
        {
            var startRes = Start();
            //活动未开始
            if (startRes.ErrorCode != 0)
                return Json(startRes);

            var config = GetConfig();
            var realThingEnt = realThingsLis.FirstOrDefault(it => it.PrizeId == prizeId);

            //有无奖品   
            if (realThingEnt == null)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None });

            //选择奖励
            if (productMappingDetailId == 0)
            {
                var resultData =
                    _respoRepository.MemberBuyRecord(UserInfo.Id, config.StartTime, config.EndTime, realThingEnt.ExchangeMoney).Select(it => new { it.ProductMappingDetailId, it.TotalIncome, it.State, it.ProductShares, it.Title, Phone = StringHelper.CoverPhone(UserInfo.Phone) }).OrderBy(it=>it.State);
                if(resultData.Count()<=0)
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = StringHelper.CoverPhone(UserInfo.Phone) ,Message = "暂无购买记录。"});
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = resultData });
            }

            //兑换奖励
            var buyRecord = _respoRepository.MemberBuyRecord(UserInfo.Id, config.StartTime, config.EndTime, realThingEnt.ExchangeMoney, productMappingDetailId)
                .FirstOrDefault(it => it.State == 0);

            if (buyRecord == null)
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "不满足兑换条件" });

            buyRecord.ActivityName = "2017.3.20三月换购活动";
            buyRecord.PrizeId = prizeId;
            buyRecord.PrizeName = realThingEnt.Name;
            buyRecord.PrizeMoney = realThingEnt.ExchangeMoney;
            buyRecord.Phone = UserInfo.Phone;
            buyRecord.ReceiveState = 1;

            //奖励记录
            Logger.Info($"AddEntityReward :{buyRecord.ToJson()}");
            var addRes = _respoRepository.Add(buyRecord);
            if (addRes > 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Message = "OK" });

            Logger.Error("AddEntityReward :插入奖励实体失败");
            return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "兑换失败" });
        }

        private static ExchangeBuyConfig GetConfig()
        {
            return JsonConfig.GetJson<ExchangeBuyConfig>("Config/activity.exchangebuyvalue.json");
        }

        //奖励实体
        private class ExchangeBuyConfig
        {
            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

        }
    }




}