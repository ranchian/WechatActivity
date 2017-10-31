using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Wechat.Activity.ConfigModel;
using Quartz.Util;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 一带一路活动
    /// </summary>
    [CrossDomainFilter]
    public class OneTakeOneController : ActivityController
    {
        private const string Key = "onetakeone";
        private readonly SqlDataRepository _respoRepository;

        public ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }

        public OneTakeOneController()
        {
            _respoRepository = new SqlDataRepository(SqlConnectString);
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
            var config = OneTakeOneConfig.GetConfig();

            var now = DateTime.Now;
            if (now < config.StartTime || now > config.EndTime)
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
                var config = OneTakeOneConfig.GetConfig();
                var userData = GetRepository().Query<TotalChanceModel>(it => it.MemberId == userId && it.Key == Key).FirstOrDefault();
                if (userData != null)
                    total = userData;


                //是否为卿(S)渠道用户(排除首投)
                var notFirst = false;
                var memberChennel = _respoRepository.GetMemberChennel(userId);
                if (memberChennel?.Channel != null && memberChennel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && memberChennel.CreateTime > config.StartTime)
                    notFirst = true;
                else if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
                    notFirst = true;

                var shares = _respoRepository.GetProductTypeShares(userId, config.StartTime, config.EndTime).ToList();

                if (notFirst)
                    Logger.Info($"ChannelMemberBefore :MemberId : {UserInfo.Id} ,Data: {shares.ToJson()}");
                if (notFirst && shares.Count > 0 && _respoRepository.GetChannelShares(userId, config.StartTime).Count() < 0)
                {
                    shares.OrderByDescending(it => it.BuyTime).FirstOrDefault().BuyShares = 0;
                    Logger.Info($"ChannelMemberAfter  :MemberId : {UserInfo.Id} ,Data: {shares.ToJson()}");
                }
                long count = 0;
                long add = 0;
                foreach (var r in shares)
                {
                    add = 0;
                    switch (r.ProductTypeId)
                    {
                        case 9:
                            add = (int)r.BuyShares * 7 / 30;
                            break;
                        case 21:
                            add = (int)r.BuyShares * 12;
                            break;
                        case 20:
                            add = (int)r.BuyShares * 6;
                            break;
                        case 19:
                            add = (int)r.BuyShares * 3;
                            break;
                        case 18:
                            add = (int)r.BuyShares * 1;
                            break;
                    }
                    count += add > 0 ? add : 0;
                }

                var totalCnt = ((int)count / 12 / 1000) * 1000;

                //好友帮助次数
                var helpCount = GetRepository().Query<FriendTotalChanceModel>(it => it.Key == Key && it.FriendId == userId).Sum(it => it.HelpCount);
                //是否绑定好友
                var bindData = GetRepository().Query<FriendTotalChanceModel>(it => it.Key == Key && it.MemberId == userId).OrderByDescending(it => it.LastUpdateTime).FirstOrDefault();
                //自己使用次数
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

                //总距离
                total.Score = helpCount + total.Used;

                if (userData != null)
                    GetRepository().Update(userData);
                else
                {
                    if (userId == UserInfo.Id && UserInfo.Id != 0)
                    {
                        total.FriendId = bindData?.FriendId ?? 0;
                        total.Remark = UserInfo.Phone;
                        GetRepository().Add(total);
                    }
                }
                Logger.Info($"userData :{ userData.ToJson()}, ProductShare:{ shares.FirstOrDefault().ToJson()}");
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

            canUse = 0;
            //自己使用 Used   为好友使用 NotUsed
            if (userChance != null)
                canUse = userChance.Total - userChance.Used - userChance.NotUsed;

            if (UserInfo.Id == 27331 || UserInfo.Id == 27329)
            {
                canUse = 1000000;
                userChance.Total = 1000000;
            }
            return userChance;
        }

        /// <summary>
        /// 使用机会
        /// </summary>
        /// <param name="type"> 1, 自己使用 2,为好友使用</param>
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
            if (useCount < 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "请输入正确的里程数哟~" });
            if (type > 0 && (canUse <= 0 || useCount > canUse))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "剩余机会不足~" });
            if (type == 2 && userChance.FriendId <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "当前无助力好友。" });

            var config = OneTakeOneConfig.GetConfig();
            List<int> drawArray = config.OneTakeOneRecordList.Select(item => item.Score).ToList();

            //当前距离
            var nowDistance = userChance.Score;
            TotalChanceModel friendData = new TotalChanceModel();
            if (type == 2)
            {
                friendData = GetRepository().Query<TotalChanceModel>(it => Key == it.Key && it.MemberId == userChance.FriendId).FirstOrDefault();
                nowDistance = friendData?.Score ?? 0;
            }

            var futureLength = nowDistance + useCount;
            var nextDraw = 0;
            for (int i = 0; i <= 29; i++)
            {
                if (drawArray[i] > futureLength || i == 29)
                {
                    nextDraw = drawArray[i];
                    break;
                }
            }

            var nextLength = nextDraw - nowDistance - useCount;

            //应该获得奖励
            var rewardList = config.OneTakeOneRecordList.Where(it => it.Score <= futureLength).ToList();

            List<int> rewardlis = new List<int>();

            for (int i = 0; i < rewardList.Count; i++)
            {
                if (GetRepository().Query<LuckdrawModel>(it => (it.Key == Key && it.MemberId == UserInfo.Id) && (rewardList[i].Score == it.Score || i == it.Sequnce)).ToList().Count > 0)
                    continue;
                rewardlis.Add(i);
            }

            if (useCount <= 0)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { CanUseCount = canUse, Score = futureLength, hasFriend = userChance.FriendId > 0, nextLength = nextLength < 0 ? 0 : nextLength, Reward = rewardlis } });
            }


            if (type == 1)
            {
                userChance.Used += useCount;
                userChance.Score += useCount;
                GetRepository().Update(userChance);
            }
            else if (type == 2)
            {
                var helpFriendData = GetRepository()
                    .Query<FriendTotalChanceModel>(
                        it => it.MemberId == UserInfo.Id && it.FriendId == userChance.FriendId && Key == it.Key)
                    .FirstOrDefault();

                userChance.NotUsed += useCount;
                GetRepository().Update(userChance);

                helpFriendData.HelpCount += useCount;
                helpFriendData.LastUpdateTime = DateTime.Now;
                GetRepository().Update(helpFriendData);
                //更新好友成绩
                Summary(new TotalChanceModel(), userChance.FriendId);
            }

            return Json(new ResponseModel
            {
                ErrorCode = ErrorCode.None,
                Data = new
                {
                    CanUseCount = userChance.Total - userChance.Used - userChance.NotUsed,
                    HelpCount = userChance.NotUsed,
                    Score = futureLength,
                    nextLength = nextLength < 0 ? 0 : nextLength,
                    Reward = rewardlis
                }
            });
        }

        /// <summary>
        /// 随机发放卡券
        /// </summary>
        [HttpPost]
        public ActionResult GiveCoupon(int type, int rewardIndex = 0, int couponIndex = 0, long memberId = 0L)
        {
            try
            {
                var validateRes = Validate();
                if (validateRes.ErrorCode != ErrorCode.None)
                {
                    return Json(validateRes);
                }
                if (rewardIndex < 0 || couponIndex < 0 || rewardIndex > 30 || couponIndex > 4)
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "无效的卡券~" });
                int canUse;
                var userChance = SelectCount(out canUse);

                memberId = UserInfo.Id;
                //验证是否领取过奖励
                var lastRewardData =
                    GetRepository()
                        .Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == memberId)
                        .OrderByDescending(it => it.CreateTime)
                        .ThenByDescending(it => it.Sequnce);

                var config = OneTakeOneConfig.GetConfig();
                var rewardItem = lastRewardData.FirstOrDefault(it => it.Sequnce == rewardIndex);

                Logger.Info($"rewardItem :{ rewardItem.ToJson() }, memberId:{ memberId }");

                if (type == 2 && (rewardItem != null || config.OneTakeOneRecordList[rewardIndex].Score > userChance.Score))
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "您已经领取过奖励了~~" });


                List<OneTakeOneRecord> rewardList = config.OneTakeOneRecordList.Where(item => item.Score <= userChance.Score).ToList();

                Logger.Info($"rewardList :{ rewardList.ToJson() }, memberId:{ memberId }");

                List<long> couponModelIds = new List<long>();
                List<int> sequnceList = new List<int>();
                List<OneTakeOneRecord> giverewardList = new List<OneTakeOneRecord>();
                //最后奖励编号
                //var lastRewardId = lastRewardData?.Sequnce ?? 0;
                //当前能获取最大奖励编号
                var canRewardId = rewardList.Count;

                long total;
                for (int i = 0; i < canRewardId; i++)
                {

                    var hasreward = lastRewardData.Where(it => it.Sequnce == i).ToList().Count > 0;
                    if (hasreward)
                        continue;

                    if (type == 2)
                        i = rewardIndex;


                    total = rewardList[i].Score;
                    var rewardData = rewardList[i];
                    var randommax = 0;

                    if (total >= 1000 && total <= 3000)
                        randommax = 5;
                    else if ((total >= 8000 && total <= 100000) || total == 380000)
                        randommax = 4;
                    else if (total >= 130000 && total <= 1000000)
                        randommax = 3;
                    var random = new Random();
                    var randowNum = random.Next(1, randommax);

                    Logger.Info($"couponIndex :{ couponIndex }, memberId:{ memberId }");

                    //发单张卡券
                    if (type == 2)
                    {
                        if (couponIndex > randommax - 1)
                            return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "无效的卡券~" });
                        randowNum = couponIndex;
                        rewardData = rewardList[rewardIndex];
                        i = canRewardId;

                        Logger.Info($"type==2,rewardData :{ rewardData.ToJson() }, memberId:{ memberId }");

                    }

                    Logger.Info($"randowNum :{ randowNum }, memberId:{ memberId }");

                    switch (randowNum)
                    {
                        case 1:
                            couponModelIds.Add(rewardData.Coupon.RateCoupon1);
                            break;
                        case 2:
                            couponModelIds.Add(rewardData.Coupon.RateCoupon2);
                            break;
                        case 3:
                            couponModelIds.Add(rewardData.Coupon.RateCoupon3);
                            break;
                        case 4:
                            couponModelIds.Add(rewardData.Coupon.RateCoupon4);
                            break;
                        default:
                            return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "无效的卡券~" });
                    }
                    giverewardList.Add(rewardData);
                    sequnceList.Add(i);
                }


                var userInfo = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == memberId).FirstOrDefault();
                if (userInfo == null)
                {
                    Summary(new TotalChanceModel(), memberId);
                    userInfo = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == memberId).FirstOrDefault();
                }

                //发放奖励
                if (couponModelIds.Count > 0)
                {

                    Logger.Info($"couponModelIds :{ couponModelIds.ToJson() }, memberId:{ memberId }");

                    List<LuckdrawModel> lis = giverewardList.Select((t, i) => new LuckdrawModel
                    {
                        MemberId = userInfo.MemberId,
                        Phone = userInfo.Remark,
                        Type = "现金券",
                        Key = Key,
                        Name = t.Reward,
                        Prize = couponModelIds[i],
                        Remark = t.Address,
                        Sequnce = type == 2 ? rewardIndex : sequnceList[i],
                        Score = t.Score
                    }).ToList();

                    var response = CardCouponApi.CouponGive(lis[0].MemberId, null, couponModelIds);
                    var repository = GetRepository();
                    foreach (var item in lis)
                    {
                        item.Status = response.IsOk ? 1 : 0;
                        item.CouponRes = response.Detail;
                        repository.Add(item);
                    }
                }
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = "发放成功。" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "您已经领取过奖励了~" });
            }
        }

        /// <summary>
        /// 查询是否有未领取卡券
        /// </summary>
        public List<int> CouponValidate()
        {
            try
            {
                int canUse;
                var userChance = SelectCount(out canUse);

                var config = OneTakeOneConfig.GetConfig();
                List<OneTakeOneRecord> canrewardList = config.OneTakeOneRecordList.Where(item => item.Score <= userChance.Score).ToList();

                var nowrewardList = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).ToList();

                List<int> resultList = new List<int>();
                if (canrewardList.Count > nowrewardList.Count)
                {
                    for (int i = nowrewardList.Count; i < canrewardList.Count; i++)
                    {
                        resultList.Add(i);
                    }
                }
                return resultList;
            }
            catch (Exception ex)
            {
                Logger.Error("CouponValidate :" + ex);
                return new List<int>();
            }
        }

        /// <summary>
        /// 绑定好友
        /// </summary>
        /// <param name="friendOpenId"></param>
        /// <param name="phone"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindFriend(long phone, string t)
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }
            var memberInfo = new Data.Model.RDBS.MemberModel();
            memberInfo = phone > 0 ? new SqlDataRepository(SqlConnectString).GetMemberId(phone.ToString()) : new MemberRepository(SqlConnectString).GetMemberInfo(t);

            if (string.IsNullOrEmpty(memberInfo?.Phone))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "好友已生成新的邀请链接~" });
            if (memberInfo.MemberId == UserInfo.Id)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "不能为自己助力哦O(∩_∩)O~~" });

            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();
            if (userChance == null)
            {
                int canUse;
                userChance = SelectCount(out canUse);
            }
            //添加好友助力记录
            var friendHelpData = GetRepository().Query<FriendTotalChanceModel>(it => it.Key == Key && it.FriendPhone == long.Parse(memberInfo.Phone) && it.MemberId == UserInfo.Id).FirstOrDefault();
            if (friendHelpData == null)
            {
                GetRepository().Add(new FriendTotalChanceModel
                {
                    MemberId = UserInfo.Id,
                    Key = Key,
                    Phone = long.Parse(UserInfo.Phone),
                    FriendId = memberInfo.MemberId,
                    FriendPhone = long.Parse(memberInfo.Phone),
                    BindDate = DateTime.Now,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                });
            }
            else
            {
                friendHelpData.LastUpdateTime = DateTime.Now;
                GetRepository().Update(friendHelpData);
            }

            userChance.FriendId = memberInfo.MemberId;
            userChance.BindDate = DateTime.Now;
            userChance.LastStatisticsTime = DateTime.Now;
            GetRepository().Update(userChance);



            //该好友是否有记录
            var friendData = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userChance.FriendId).FirstOrDefault();

            if (friendData != null)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = "OK" });

            //添加好友记录
            GetRepository().Add(new TotalChanceModel
            {
                Key = Key,
                MemberId = memberInfo.MemberId,
                Remark = phone.ToString(),
                CreateTime = DateTime.Now
            });
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = "OK" });
        }

        [HttpPost]
        public ActionResult GetMemberInfo(string t)
        {
            try
            {
                var memberInfo = new Data.Model.RDBS.MemberModel();
                memberInfo = new MemberRepository(SqlConnectString).GetMemberInfo(t);

                if (string.IsNullOrEmpty(memberInfo?.Phone))
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "好友已生成新的邀请链接~" });
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Phone = memberInfo.Phone.Substring(0, 3) + "****" + memberInfo.Phone.Substring(7, 4) } });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }

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
            var config = OneTakeOneConfig.GetConfig();
            List<int> drawArray = config.OneTakeOneRecordList.Select(item => item.Score).ToList();

            //好友自己划得距离
            var friendChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userChance.FriendId).FirstOrDefault();
            //好友被帮助得距离
            var friendCount =
                GetRepository()
                    .Query<FriendTotalChanceModel>(it => it.Key == Key && it.FriendId == userChance.FriendId)
                    .Sum(it => it.HelpCount);

            var nowCount = friendCount + friendChance.Used;
            var nextLength = nowCount;
            for (int i = 0; i <= 100; i++)
            {
                if (drawArray[i] > nextLength || i == 29)
                {
                    nextLength = drawArray[i];
                    break;
                }
            }
            var friendPhone = friendChance.Remark;
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { FriendPhone = friendPhone.Substring(0, 3) + "****" + friendPhone.Substring(7, 4), Score = nowCount, nextLength = nextLength - nowCount < 0 ? 0 : nextLength - nowCount } });
        }

        /// <summary>
        /// 好友助力次数
        /// </summary>
        /// <returns></returns>
        public ActionResult FriendList()
        {
            try
            {
                var validateRes = Validate();
                if (validateRes.ErrorCode != ErrorCode.None)
                {
                    return Json(validateRes);
                }
                //我帮好友
                var meHelpfriend = GetRepository().Query<FriendTotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id && it.MemberId != 0 && it.FriendId != 0).OrderByDescending(it => it.LastUpdateTime).Select(it => new { it.FriendId, it.FriendPhone, it.HelpCount }).FirstOrDefault();
                TotalChanceModel friendScore = new TotalChanceModel();
                if (meHelpfriend != null)
                {
                    //好友当前位置
                    friendScore =
                       GetRepository()
                           .Query<TotalChanceModel>(
                               it =>
                                   it.Key == Key && it.MemberId == meHelpfriend.FriendId && it.MemberId != 0).FirstOrDefault();
                }
                int canUse;
                SelectCount(out canUse);

                //好友帮我
                var friendHelpMe = GetRepository().Query<FriendTotalChanceModel>(it => it.Key == Key && it.FriendId == UserInfo.Id && it.HelpCount != 0).Select(it => new { Phone = it.Phone.ToString().Substring(0, 3) + "****" + it.Phone.ToString().Substring(7, 4), it.HelpCount }).ToList().ToJson();

                Logger.Info($"userChance {meHelpfriend.ToJson()} userHelp : {friendHelpMe.ToJson()} helpData :{friendHelpMe}");

                if (meHelpfriend == null)
                    return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { CanUseCount = canUse, meHelpfriend = new { }, friendHelpMe } });

                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { CanUseCount = canUse, meHelpfriend = new { FriendPhone = meHelpfriend.FriendPhone.ToString().Substring(0, 3) + "****" + meHelpfriend.FriendPhone.ToString().Substring(7, 4), meHelpfriend.HelpCount, friendScore.Score }, friendHelpMe } });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }

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
                    .Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id)
                    .OrderByDescending(it => it.CreateTime).OrderByDescending(it => it.Sequnce)
                    .Select(it => new
                    { it.Remark, it.Name });
            Logger.Info($"RewardList :{rewardList.ToJson()}");
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = rewardList });
        }
    }
}
