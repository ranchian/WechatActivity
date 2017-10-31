using System;
using System.Linq;
using System.Web.Mvc;

using FJW.Unit;

using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Data.Model.RDBS;

using FJW.SDK2Api.CardCoupon;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 活动通用类
    /// </summary>
    [CrossDomainFilter]
    public class ActivityPublicController<T> : ActivityController where T : BaseConfig<T>
    {
        private string Key;
        private readonly SqlDataRepository _respoRepository;
        private readonly T _config;
        public string luckString = "";

        public ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }

        public ActivityPublicController(string key, string jsonUrl)
        {
            this.Key = key;
            _respoRepository = new SqlDataRepository(SqlConnectString);
            _config = BaseConfig<T>.GetConfig(jsonUrl);
        }

        /// <summary>
        /// 验证 
        /// </summary>
        /// <returns></returns>
        private ResponseModel Validate()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return new ResponseModel { ErrorCode = ErrorCode.NotLogged, Message = "用户未登录!" };
            }

            var now = DateTime.Now;
            if (now < _config.StartTime || now > _config.EndTime)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "活动未开始或已过期" };

            return new ResponseModel { ErrorCode = ErrorCode.None, Data = "" };
        }

        /// <summary>
        /// 计算数量
        /// </summary>
        private void Summary(TotalChanceModel total, long userId)
        {
            try
            {
                var userData =
                    GetRepository()
                        .Query<TotalChanceModel>(it => it.MemberId == userId && it.Key == Key)
                        .FirstOrDefault();
                if (userData != null)
                    total = userData;

                var shares = _respoRepository.GetProductTypeShares(userId, _config.StartTime, _config.EndTime).ToList();


                long count = 0;
                foreach (var r in shares)
                {
                    long add;
                    SwitchMethod(r, out add);
                    count += add > 0 ? add : 0;
                }

                //用户投资获得抽奖次数
                var totalCnt = (int)count / 12 / 1000; ;

                //使用次数
                var totalChanceModel = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userId).FirstOrDefault();

                if (totalChanceModel != null)
                {
                    var useCount = totalChanceModel.Used;
                    //使用次数
                    total.Used = useCount;
                }

                total.Key = Key;
                total.MemberId = userId;
                total.Total = totalCnt;

                if (userData != null)
                {
                    GetRepository().Update(userData);
                }
                else
                {
                    if (userId == UserInfo.Id && UserInfo.Id != 0)
                    {
                        total.Remark = UserInfo.Phone;
                        GetRepository().Add(total);
                    }
                }
                Logger.Info($"{Key}-userData :{userData.ToJson()}, ProductShare:{shares.FirstOrDefault().ToJson()}");
            }
            catch (Exception ex)
            {
                Logger.Error("Summary :" + ex);
            }
        }

        /// <summary>
        /// 查询可以使用次数
        /// </summary>
        /// <returns></returns>
        private TotalChanceModel SelectCount(out int canUse)
        {
            //统计次数
            Summary(new TotalChanceModel(), UserInfo.Id);

            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();

            #region Test Code 测试号
            if (userChance != null && (UserInfo.Phone == "15961956476" || UserInfo.Phone == "18761216965"))
            {
                userChance.Total = 100000;
            }
            #endregion

            canUse = 0;
            //Used 自己使用
            if (userChance != null)
                canUse = userChance.Total - userChance.Used;

            return userChance;
        }

        /// <summary>
        /// 统计年化公共方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="add"></param>
        private void SwitchMethod(ProductTypeSumShare item, out long add)
        {
            switch (item.ProductTypeId)
            {
                case 5:
                    item.Title = "房金月宝";
                    add = (int)item.BuyShares * 1;
                    break;
                case 6:
                    item.Title = "房金季宝";
                    add = (int)item.BuyShares * 3;
                    break;
                case 7:
                    item.Title = "房金双季宝";
                    add = (int)item.BuyShares * 6;
                    break;
                case 8:
                    item.Title = "房金年宝";
                    add = (int)item.BuyShares * 12;
                    break;
                default:
                    add = 0;
                    break;
            }
        }
        
        /// <summary>
        /// 我的奖励
        /// </summary>
        /// <returns></returns>
        public ActionResult RewardList()
        {
            try
            {
                var validateRes = Validate();
                if (validateRes.ErrorCode != ErrorCode.None)
                    return Json(validateRes);

                int canUse;
                SelectCount(out canUse);

                var resultData = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).OrderByDescending(it => it.CreateTime).Select(it => new
                {
                    it.Type,
                    it.Name,
                    Date = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse <= 0 ? 0 : canUse, RewardList = resultData } });
            }
            catch (Exception ex)
            {
                Logger.Error($"{Key}-RewardList：{ex}");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "服务繁忙~" });
            }
        }

        /// <summary>
        /// 奖励
        /// </summary>
        /// <returns></returns>
        public ActionResult LuckReward()
        {
            try
            {
                int canUse;
                SelectCount(out canUse);

                var resultData = GetRepository().Query<LuckdrawModel>(it => it.Key == Key).OrderByDescending(it => it.CreateTime).Take(100).Select(it => new
                {
                    Phone = it.Phone.Substring(0, 3) + "****" + it.Phone.Substring(7, 4),
                    it.Type,
                    it.Name,
                    Date = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new {IsLogin = UserInfo.Id > 0 ,Count = canUse <= 0 ? 0 : canUse, RewardList = resultData } });
            }
            catch (Exception ex)
            {
                Logger.Error($"{Key}-RewardList：{ex}");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "服务繁忙~" });
            }
        }
    }
}

