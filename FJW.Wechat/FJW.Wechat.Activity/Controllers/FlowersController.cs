using System;
using System.Linq;
using System.Web.Mvc;

using FJW.Unit;

using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Data.Model.RDBS;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 7月养花活动
    /// </summary>
    [CrossDomainFilter]
    public class FlowersController : ActivityController
    {
        private const string Key = "flowers";
        private readonly SqlDataRepository _respoRepository;
        private readonly FlowersConfig _config;

        public ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }

        public FlowersController()
        {
            _respoRepository = new SqlDataRepository(SqlConnectString);
            _config = FlowersConfig.GetConfig();
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


                //是否为卿(S)渠道用户(排除首投)
                var notFirst = false;
                var memberChennel = _respoRepository.GetMemberChennel(userId);
                if (memberChennel?.Channel != null &&
                    memberChennel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) &&
                    memberChennel.CreateTime > _config.StartTime)
                    notFirst = true;
                else if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
                    notFirst = true;

                var shares = _respoRepository.GetProductTypeShares(userId, _config.StartTime, _config.EndTime).ToList();

                if (notFirst)
                    Logger.Info($"ChannelMemberBefore :MemberId : {UserInfo.Id} ,Data: {shares.ToJson()}");

                if (notFirst && shares.Count > 0 &&
                    _respoRepository.GetChannelShares(userId, _config.StartTime).Count() < 0)
                {
                    shares.OrderByDescending(it => it.BuyTime).FirstOrDefault().BuyShares = 0;
                    Logger.Info($"ChannelMemberAfter  :MemberId : {UserInfo.Id} ,Data: {shares.ToJson()}");
                }

                long count = 0;
                foreach (var r in shares)
                {
                    long add;
                    SwitchMethod(r, out add);
                    count += add > 0 ? add : 0;
                }

                //每日投资统计
                DayBuyRecord(userId);

                //用户自己投资获得养分
                var totalCnt = (int)count / 12 / 100;
                var friendData =
                    GetRepository()
                        .Query<FriendTotalChanceModel>(
                            it => it.Key == Key && it.MemberId != userId && it.FriendId == userId);
                var helpCount = 0;
                int invest = 0;
                FriendTotalChanceModel bindData = null;
                if (friendData != null && friendData.Any())
                {
                    //邀请好友获得养分
                    invest = friendData.Where(it => it.Type == 2).GroupBy(it => it.MemberId).Count();

                    //好友助力获得养分
                    helpCount = friendData.Where(it => it.Type != 2 && it.HelpCount > 0).Sum(it => it.HelpCount);

                    //是否绑定好友
                    bindData = friendData.OrderByDescending(it => it.LastUpdateTime).FirstOrDefault();
                }

                //自己使用次数
                var totalChanceModel =
                    GetRepository()
                        .Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userId)
                        .FirstOrDefault();

                if (totalChanceModel != null)
                {
                    var useCount = totalChanceModel.Used;
                    //使用次数
                    total.Used = useCount;
                }

                total.Key = Key;
                total.MemberId = userId;
                total.Total = totalCnt;

                //总分数
                total.Score = helpCount + total.Used + invest;

                if (userData != null)
                {
                    GetRepository().Update(userData);
                }
                else
                {
                    if (userId == UserInfo.Id && UserInfo.Id != 0)
                    {
                        total.FriendId = bindData?.FriendId ?? 0;
                        total.Remark = UserInfo.Phone;
                        GetRepository().Add(total);
                    }
                }
                Logger.Info($"userData :{userData.ToJson()}, ProductShare:{shares.FirstOrDefault().ToJson()}");
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

            var userChance =
                GetRepository()
                    .Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id)
                    .FirstOrDefault();

            canUse = 0;
            //自己使用 Used   为好友使用 NotUsed
            if (userChance != null)
                canUse = userChance.Total - userChance.Used - userChance.NotUsed;

            return userChance;
        }

        /// <summary>
        /// 使用机会
        /// </summary>
        /// <param name="type"> 1, 自己使用 2,为好友使用</param>
        /// <param name="useCount">使用次数</param>
        /// <param name="flowersType">花种(1,闭月羞花;2,生如夏花;3,锦上添花;)</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Give(int type = 0, int useCount = 0, int flowersType = 0)
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            int canUse;
            var userChance = SelectCount(out canUse);
            if (useCount < 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "请输入正确的次数哟~" });
            if (type > 0 && (canUse <= 0 || useCount > canUse))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "快去投资，帮好友的花浇水哟~" });
            if (type == 2 && userChance.FriendId <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "当前无助力好友，快去邀请吧。" });

            //当前距离
            var nowDistance = userChance.Score;
            TotalChanceModel friendData = new TotalChanceModel();
            if (type == 2)
            {
                friendData =
                    GetRepository()
                        .Query<TotalChanceModel>(it => Key == it.Key && it.MemberId == userChance.FriendId)
                        .FirstOrDefault();
                nowDistance = friendData?.Score ?? 0;
            }

            //绑定花种类
            if (userChance.Type <= 0 && flowersType > 0)
            {
                userChance.Type = flowersType;
                GetRepository().Update(userChance);
            }

            var futureScore = nowDistance + useCount;
            if (useCount <= 0 || (type == 1 && userChance.Type <= 0))
                return
                    Json(new ResponseModel
                    {
                        ErrorCode = ErrorCode.None,
                        Data =
                            new
                            {
                                CanUseCount = canUse,
                                Score = nowDistance,
                                hasFriend = userChance.FriendId > 0,
                                FlowersType = type == 2 ? friendData.Type : userChance.Type
                            }
                    });



            if (type == 1)
            {
                userChance.Used += useCount;
                userChance.Score += useCount;
                GetRepository().Update(userChance);

                GetRepository().Add(new FriendTotalChanceModel
                {
                    MemberId = UserInfo.Id,
                    Key = Key,
                    Phone = long.Parse(UserInfo.Phone),
                    FriendId = UserInfo.Id,
                    FriendPhone = long.Parse(UserInfo.Phone),
                    HelpCount = useCount,
                    Type = 1,
                    Remark = "自己使用",
                    LastUpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now
                });
            }
            else if (type == 2)
            {
                var helpFriendData = GetRepository()
                    .Query<FriendTotalChanceModel>(
                        it => it.MemberId == UserInfo.Id && it.FriendId == userChance.FriendId && Key == it.Key)
                    .OrderByDescending(it => it.CreateTime).FirstOrDefault();

                userChance.NotUsed += useCount;
                GetRepository().Update(userChance);

                if (helpFriendData.Type == 2)
                    helpFriendData.Type = 3;


                helpFriendData.Remark = "助力好友";
                helpFriendData.HelpCount = useCount;
                helpFriendData.LastUpdateTime = DateTime.Now;
                helpFriendData.ID = Guid.NewGuid();
                helpFriendData.CreateTime = DateTime.Now;
                GetRepository().Add(helpFriendData);
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
                    Score = futureScore,
                    FlowersType = type == 1 ? userChance.Type : friendData.Type
                }
            });
        }

        /// <summary>
        /// 绑定好友
        /// </summary>
        /// <param name="friendOpenId"></param>
        /// <param name="phone"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindFriend(string phone = "", string t = "")
        {
            var validateRes = Validate();

            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            if (string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(t))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "参数有误~" });

            var friendInfo = new MemberModel();
            friendInfo = !string.IsNullOrEmpty(phone)
                ? _respoRepository.GetMemberId(phone)
                : new MemberRepository(SqlConnectString).GetMemberInfo(t);

            if (string.IsNullOrEmpty(friendInfo?.Phone))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "好友已生成新的邀请链接~" });
            if (friendInfo.MemberId == UserInfo.Id)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "不能为自己助力哦O(∩_∩)O~~" });

            var userChance =
                GetRepository()
                    .Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id)
                    .FirstOrDefault();

            if (userChance == null)
            {
                int canUse;
                userChance = SelectCount(out canUse);
            }

            //是否新注册用户
            var isNewMember = _respoRepository.IsActivityMember(UserInfo.Phone, t, _config.StartTime, _config.EndTime);
            var hasFriend =
                GetRepository()
                    .Query<FriendTotalChanceModel>(it => it.Key == Key && it.Type == 2 && it.MemberId == UserInfo.Id);

            Logger.Info($"newmember : {UserInfo.Phone}  --- {isNewMember} ");

            //type : 2 活动链接注册好友 3 好友投资助力
            var isNewMemberBind = isNewMember && !hasFriend.Any();
            var type = isNewMemberBind ? 2 : 3;

            var friendData =
                GetRepository()
                    .Query<FriendTotalChanceModel>(
                        it =>
                            it.Key == Key && it.MemberId == UserInfo.Id && it.FriendId == friendInfo.MemberId).OrderByDescending(it => it.LastUpdateTime)
                    .FirstOrDefault();
            if (friendData == null)
            {
                //添加好友助力记录
                GetRepository().Add(new FriendTotalChanceModel
                {
                    MemberId = UserInfo.Id,
                    Key = Key,
                    Phone = long.Parse(UserInfo.Phone),
                    FriendId = friendInfo.MemberId,
                    FriendPhone = long.Parse(friendInfo.Phone),
                    BindDate = DateTime.Now,
                    HelpCount = isNewMemberBind ? 1 : 0,
                    CreateTime = DateTime.Now,
                    Type = type,
                    Remark = isNewMemberBind ? "新注册用户" : "好友投资助力",
                    LastUpdateTime = DateTime.Now
                });
            }

            userChance.FriendId = friendInfo.MemberId;
            userChance.BindDate = DateTime.Now;
            userChance.LastUpdateTime = DateTime.Now;
            GetRepository().Update(userChance);

            //该好友是否有记录
            var friendTotalData =
                GetRepository()
                    .Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userChance.FriendId)
                    .FirstOrDefault();

            if (friendTotalData != null)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = "OK" });

            //添加好友记录
            GetRepository().Add(new TotalChanceModel
            {
                Key = Key,
                MemberId = friendInfo.MemberId,
                Remark = friendInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            });
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = "OK" });
        }

        /// <summary>
        /// 好友游戏当前成长值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FriendCount()
        {
            try
            {
                var validateRes = Validate();
                if (validateRes.ErrorCode != ErrorCode.None)
                {
                    return Json(validateRes);
                }
                var userChance =
                    GetRepository()
                        .Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id)
                        .FirstOrDefault();
                if (userChance == null || userChance.FriendId <= 0)
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "当前无助力好友。" });

                //统计次数
                Summary(new TotalChanceModel(), userChance.FriendId);

                var friendData =
                    GetRepository()
                        .Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userChance.FriendId)
                        .FirstOrDefault();

                //我帮好友浇水数据
                var helpList =
                    GetRepository()
                        .Query<FriendTotalChanceModel>(
                            it => it.Key == Key && it.MemberId == UserInfo.Id && it.FriendId == friendData.MemberId && it.HelpCount > 0)
                        .OrderByDescending(it => it.CreateTime)
                        .Select(it => new
                        {
                            it.HelpCount,
                            it.CreateTime,
                            it.Type
                        });

                var friendPhone = friendData.Remark;
                return
                    Json(new ResponseModel
                    {
                        ErrorCode = ErrorCode.None,
                        Data =
                            new
                            {
                                FriendPhone = friendPhone.Substring(0, 3) + "****" + friendPhone.Substring(7, 4),
                                friendData.Score,
                                HelpList = helpList,
                                FlowersType = friendData.Type
                            }
                    });
            }
            catch (Exception ex)
            {
                Logger.Error($"FriendCount ： {ex}");
                return Json(new ResponseModel {ErrorCode = ErrorCode.Exception});
            }
        }

        /// <summary>
        /// 好友帮助我
        /// </summary> 获得养份类型（1、肥料 2、阳光 3、 水分 4、系统赠送）
        /// <returns></returns>
        [HttpPost]
        public ActionResult FriendHelpMe()
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            var helpList =
                GetRepository()
                    .Query<FriendTotalChanceModel>(it => it.Key == Key && it.FriendId == UserInfo.Id && it.HelpCount > 0)
                    .OrderByDescending(it => it.CreateTime)
                    .Select(it => new
                    {
                        Phone = it.Phone == 0 || it.Phone.ToString() == UserInfo.Phone ? "" : it.Phone.ToString().Substring(0, 3) + "****" + it.Phone.ToString().Substring(7, 4),
                        it.HelpCount,
                        it.CreateTime,
                        it.Type
                    });

            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { HelpList = helpList.ToList() } });
        }

        /// <summary>
        /// 记录每日产品投资份额
        /// </summary>
        /// <param name="userId"></param>
        public void DayBuyRecord(long userId)
        {
            var dayShare = _respoRepository.GetProductTypeShares(userId, DateTime.Now.Date, DateTime.Now.AddDays(1).Date);

            long count = 0;
            foreach (var r in dayShare)
            {
                long add;
                SwitchMethod(r, out add);
                count += add > 0 ? add : 0;
            }

            var daybuyShare = (int)count / 12 / 100;

            var hasRecordData =
                GetRepository()
                    .Query<RecordModel>(it => it.Date == DateTime.Now.Date && it.Key == Key && it.MemberId == userId)
                    .FirstOrDefault();
            if (hasRecordData != null)
            {
                hasRecordData.Total = daybuyShare;
                hasRecordData.LastUpdateTime = DateTime.Now;
                GetRepository().Update(hasRecordData);
                return;
            }

            GetRepository().Add(new RecordModel
            {
                MemberId = userId,
                Key = Key,
                Total = daybuyShare,
                Date = DateTime.Now.Date,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            });
            Logger.Info($" MemberId :{userId} DayBuyRecord 每日投资份额统计: {hasRecordData.ToJson()}");
        }

        /// <summary>
        /// 投资明细
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductBuyDetail()
        {
            try
            {
                var shares =
                    _respoRepository.GetProductTypeShares(UserInfo.Id, _config.StartTime, _config.EndTime).ToList();
                long count = 0;

                foreach (var r in shares)
                {
                    long add = 0;
                    SwitchMethod(r, out add);
                    count += add > 0 ? add : 0;
                }
                //用户年化投资金额
                var totalCnt = (int)count / 12;

                var productShareList =
                    shares.Where(it => !string.IsNullOrEmpty(it.Title)).Select(it => new { it.Title, it.BuyShares });
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.None,
                    Data = new
                    {
                        ProductShareList = productShareList,
                        ProductStatics = totalCnt
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"ProductBuyDetail : {ex}");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception });
            }
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
                case 9:
                    item.Title = "新手专享";
                    add = (int)item.BuyShares * 7 / 30;
                    break;
                default:
                    add = 0;
                    break;
            }
        }

        [HttpPost]
        public ActionResult GetMemberInfo(string t)
        {
            try
            {
                var memberInfo = new MemberModel();
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
        /// 统计花朵
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FlowersGroup()
        {
            if (UserInfo.Id != 55 )
                return Json("");

            var groupData = GetRepository()
                .Query<TotalChanceModel>(it => it.Key == Key && it.Score > 0 && it.MemberId > 0)
                .Select(it => new TotalChanceModel
                {
                    Score = NumResult(it.Score * 10),
                    Type = it.Type
                });
            var result = groupData.GroupBy(it => it.Type).Select(it => new
            {
                Type = it.Key,
                Score = it.Sum(i => i.Score)
            });
            Logger.Info($"FlowersGroup : {result.ToJson()}");
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = result });
        }

        public int NumResult(int count)
        {
            if (count >= 200 && count <= 599)
                return 1;
            if (count >= 600 && count <= 1999)
                return 2;
            if (count >= 2000 && count <= 4999)
                return 3;
            if (count >= 5000 && count <= 9999)
                return 4;
            if (count >= 10000)
                return 5;
            return 0;
        }
    }
}
