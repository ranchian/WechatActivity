using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Cache;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Wx;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 端午节活动
    /// </summary>
    [CrossDomainFilter]
    public class DuanWuController : ActivityController
    {

        private const string Key = "duanwu";


        private readonly SqlDataRepository _respoRepository;

        //获取配置
        private static DuanWuConfig GetConfig()
        {
            return JsonConfig.GetJson<DuanWuConfig>("Config/activity.duanwu.json");
        }

        protected ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }


        public DuanWuController()
        {
            _respoRepository = new SqlDataRepository(SqlConnectString);
        }
        [WAuthorize]

        public ActionResult Index()
        {
            return Redirect("http://a.fangjinnet.com/html/2017duanwu/dragonBoat-game.html");
        }

        [WAuthorize]

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
            var config = GetConfig();

            var now = DateTime.Now;
            if (now < config.StartTime || now > config.EndTime)
                return new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "活动未开始或已过期" };


            return new ResponseModel { ErrorCode = ErrorCode.None, Data = "" };
        }

        [WAuthorize]

        /// <summary>
        /// 计算数量
        /// </summary>
        private void Summary(TotalChanceModel total, long userId)
        {
            var config = GetConfig();
            var userData = GetRepository().Query<TotalChanceModel>(it => it.MemberId == userId && it.Key == Key).FirstOrDefault();
            if (userData != null)
                total = userData;

            //是否为卿渠道用户(排除首投)
            var notFirst = false;
            var memberChennel = _respoRepository.GetMemberChennel(userId);
            if (memberChennel?.CreateTime >= config.StartTime && memberChennel.Channel == "WQWL")
                notFirst = true;

            var shares = _respoRepository.GetProductTypeSingle(userId, config.StartTime, config.EndTime);
            if (notFirst)
                shares.FirstOrDefault().BuyShares = 0;

            int count = 0;
            int add = 0;
            foreach (var r in shares)
            {
                add = 0;
                switch (r.ProductTypeId)
                {
                    case 8:
                        add = (int)r.BuyShares / 200;
                        break;
                    case 7:
                        add = (int)r.BuyShares / 400;
                        break;
                    case 6:
                        add = (int)r.BuyShares / 800;
                        break;
                    case 5:
                        add = (int)r.BuyShares / 2400;
                        break;
                }
                count += add >= 1 ? add : 0;
            }

            var totalCnt = count;

            var helpCount = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.FriendId == userId).Sum(it => it.NotUsed);
            var useCount = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == userId && it.Type == "1").Count();

            total.Key = Key;
            //使用次数
            total.Used = useCount;
            total.MemberId = userId;
            total.Total = totalCnt;
            //总距离
            total.Prizes = (helpCount + total.Used).ToString();
            var shareItem = shares.LastOrDefault();
            if (shareItem != null) total.LastStatisticsTime = shareItem.BuyTime;


            if (userData != null)
                GetRepository().Update(userData);
            else
            {
                if (userId == UserInfo.Id && UserInfo.Id != 0)
                {
                    total.Remark = UserInfo.Phone;
                    total.WechatId = UserInfo.WxUserInfo?.OpenId;
                    total.NickName = UserInfo.WxUserInfo?.NickName;
                    total.HeadimgUrl = UserInfo.WxUserInfo?.HeadimgUrl;
                    GetRepository().Add(total);
                }
            }
        }

        [WAuthorize]

        /// <summary>
        /// 查询可以使用次数
        /// </summary>
        /// <returns></returns>
        private TotalChanceModel SelectCount(out int canUse)
        {
            //统计次数
            Summary(new TotalChanceModel(), UserInfo.Id);

            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();

            canUse = 0;
            //自己使用 Used   为好友使用 NotUsed
            if (userChance != null)
                canUse = userChance.Total - userChance.Used - userChance.NotUsed;

            return userChance;
        }

        [WAuthorize]

        /// <summary>
        /// 使用龙舟次数
        /// </summary>
        /// <param name="type"> 1, 自己使用 2,为好友使用</param>
        /// <param name="friendPhone">好友手机号</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Give(int type = 0, int useCount = 0)
        {

            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            int canUse;
            var userChance = SelectCount(out canUse);

            if (useCount <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse, hasFriend = userChance.FriendId > 0, UserScore = Convert.ToInt32(userChance.Prizes) } });
            if (canUse <= 0 || useCount > canUse)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "剩余机会不足~" });

            var rewardArr = "";
            //随机卡券
            int[] numList = new int[useCount];
            var random = new Random();

            //发放卡券 
            for (int i = 0; i < useCount; i++)
            {
                numList[i] = random.Next(1, 12);

            }
            for (int i = 0; i < useCount; i++)
            {
                bool giveResult;


                switch (type)
                {
                    case 1:

                        ExchangePrizes(UserInfo.Id, numList[i], false, out giveResult);
                        if (giveResult)
                        {
                            //更新游戏结果
                            userChance.Used = ++userChance.Used;
                            userChance.Prizes = (Convert.ToInt32(userChance.Prizes) + 1).ToString();
                            GetRepository().Update(userChance);
                        }

                        break;
                    case 2:
                        if (userChance.FriendId <= 0)
                            return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "当前无助力好友。" });

                        ExchangePrizes(userChance.FriendId, numList[i], true, out giveResult);
                        if (giveResult)
                        {
                            //更新游戏结果
                            userChance.NotUsed = ++userChance.NotUsed;
                            GetRepository().Update(userChance);
                        }


                        break;
                }

            }
            //更新好友成绩
            if (type == 2)
                Summary(new TotalChanceModel(), userChance.FriendId);

            return Json(new ResponseModel
            {
                ErrorCode = ErrorCode.None,
                Data = new
                {
                    CanUseCount = userChance.Total - userChance.Used - userChance.NotUsed,
                    HelpCount = userChance.NotUsed
                    //Reward = rewardArr.Substring(0, rewardArr.LastIndexOf(",", StringComparison.Ordinal)).Split(',')
                }
            });
        }

        [WAuthorize]

        /// <summary>
        /// 绑定好友
        /// </summary>
        /// <param name="friendOpenId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindFriend(long phone)
        {
            int canUse;
            SelectCount(out canUse);

            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();

            var memberInfo = new SqlDataRepository(SqlConnectString).GetMemberId(phone.ToString());
            if (memberInfo == null || memberInfo.MemberId <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "邀请用户未注册~" });
            if (memberInfo.MemberId == UserInfo.Id)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "不能为自己助力哦O(∩_∩)O~~" });
            if (userChance?.FriendId > 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "您已帮助其他用户~" });

            //绑定好友 以及 时间
            userChance.FriendId = memberInfo.MemberId;
            userChance.BindDate = DateTime.Now;
            GetRepository().Update(userChance);

            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = "OK" });
        }

        [WAuthorize]

        public ActionResult FriendLink(long phone)
        {
            return Redirect("http://a.fangjinnet.com/html/2017duanwu/duanwu-invite.html?friend=" + phone);
        }

        [WAuthorize]

        /// <summary>
        /// 好友游戏当前距离
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FriendCount()
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }
            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();
            if (userChance == null || userChance.FriendId <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "当前无助力好友。" });
            //好友自己划得距离
            var friendChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userChance.FriendId).FirstOrDefault();
            //好友被帮助得距离
            var friendCount =
                GetRepository()
                    .Query<TotalChanceModel>(it => it.Key == Key && it.FriendId == userChance.FriendId)
                    .Sum(it => it.NotUsed);

            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = friendCount + friendChance.Used, friendChance.NickName, friendChance.HeadimgUrl } });
        }


        /// <summary>
        /// 发放卡券
        /// </summary>
        private void ExchangePrizes(long memberId, int ranking, bool isFriend, out bool giveResult)
        {
            //GetMemberChennel

            var config = GetConfig();
            string name = "";
            giveResult = true;
            long counponId = 0;
            switch (ranking)
            {
                case 1:
                    name = "15元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon1);
                    break;
                case 2:
                    name = "30元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon2);
                    break;
                case 3:
                    name = "45元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon3);
                    break;
                case 4:
                    name = "50元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon4);
                    break;
                case 5:
                    name = "60元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon5);
                    break;
                case 6:
                    name = "2%加息券";
                    counponId = Convert.ToInt64(config.RateCoupon6);
                    break;
                case 7:
                    name = "2.5%加息券";
                    counponId = Convert.ToInt64(config.RateCoupon7);
                    break;
                case 8:
                    name = "3%加息券";
                    counponId = Convert.ToInt64(config.RateCoupon8);
                    break;
                case 9:
                    name = "3.5%加息券";
                    counponId = Convert.ToInt64(config.RateCoupon9);
                    break;
                case 10:
                    name = "100元现金券";
                    counponId = Convert.ToInt64(config.RateCoupon10);
                    break;
                case 11:
                    name = "5%加息券";
                    counponId = Convert.ToInt64(config.RateCoupon11);
                    break;
            }
            var repository = GetRepository();


            Task.Run(() =>
            {
                var result = CardCouponApi.UserGrant(memberId, config.ActivityId, counponId);
                repository.Add(new LuckdrawModel
                {
                    MemberId = memberId,
                    Phone = isFriend ? memberId.ToString() : UserInfo.Phone,
                    Prize = counponId,
                    Type = isFriend ? "3" : "1", //1 自己获得奖励   2现金奖励  3好友助力奖励
                    Key = Key,
                    Status = result.IsOk ? 1 : 0,
                    Sequnce = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff")),
                    Name = name,
                    Remark = "端午节活动-" + name + (isFriend ? "-该奖励来自好友-" + UserInfo.Id : "")
                });

                //获取100元现金记录
                var moneyLuck = repository.Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == memberId && it.Type != "2").Count() / 10;

                var moneyLuckCount =
                           repository
                               .Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == memberId && it.Type == "2")
                               .Count();


                if (moneyLuck <= moneyLuckCount)
                {
                    return;
                }

                repository.Add(new LuckdrawModel
                {
                    MemberId = memberId,
                    Phone = isFriend ? memberId.ToString() : UserInfo.Phone,
                    Type = "2",
                    Money = 100,
                    Key = Key,
                    Name = "100元现金",
                    Sequnce = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff")),//获得时间
                    Remark = "端午节活动-100元现金"
                });
            });
        }


        /// <summary>
        /// 总排名
        /// </summary>
        /// <returns></returns>
        public ActionResult TotalRecord()
        {
            int num = 0;
            var resData = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.Prizes != "0").OrderByDescending(it => Convert.ToInt16(it.Prizes)).ThenByDescending(it => it.LastStatisticsTime).Take(3).Select(it => new { Num = ++num, WecharName = it.NickName, it.HeadimgUrl, Phone = it.Remark.Substring(0, 3) + "****" + it.Remark.Substring(7, 4), Count = Convert.ToInt32(it.Prizes) });
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = resData });
        }

        [WAuthorize]

        /// <summary>
        /// 好友助力次数
        /// </summary>
        /// <returns></returns>
        public ActionResult FriendList()
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }
            //我帮好友划
            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id && it.MemberId != 0 && it.FriendId != 0).Select(it => new { it.FriendId, HelpCount = it.NotUsed, it.BindDate }).FirstOrDefault();
            string helpData=null;
            if (userChance != null)
            {
                 helpData = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userChance.FriendId).Select(it => new { WecharName = it.NickName, it.HeadimgUrl, userChance.HelpCount, userChance.BindDate }).ToList().ToJson();
            }
            //好友帮我划
            var userHelp = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.FriendId == UserInfo.Id && it.MemberId != 0).Select(it => new { WecharName = it.NickName, it.HeadimgUrl, HelpCount = it.NotUsed, it.BindDate }).ToList().ToJson();
            Logger.Info($"userChance {userChance.ToJson()} userHelp : {userHelp.ToJson()} helpData :{helpData}");
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { userHelp, helpData } });


        }

        [WAuthorize]

        /// <summary>
        /// 我的奖励
        /// </summary>
        /// <returns></returns>
        public ActionResult RewardList()
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }
            var rewardList =
                GetRepository()
                    .Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id && it.Type!="2")
                    .OrderByDescending(it => it.Sequnce)
                    .Select(it => new
                    { it.Name, CreateTime = it.CreateTime.ToString("yyyy-MM-dd") });
            Logger.Info($"RewardList :{rewardList.ToJson()}");
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = rewardList });
        }
    }
}
