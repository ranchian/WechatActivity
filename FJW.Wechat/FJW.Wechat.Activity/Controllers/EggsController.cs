using System;
using System.Linq;
using System.Web.Mvc;

using FJW.Unit;

using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Data.Model.RDBS;
using System.Collections.Generic;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 八月砸蛋活动
    /// </summary>
    [CrossDomainFilter]
    public class EggsController : ActivityController
    {
        private const string Key = "eggs";
        private readonly SqlDataRepository _respoRepository;
        private readonly EggsConfig _config;
        private readonly long[] luckStrArrA = { 50, 20, 12, 10, 8, 6, 5, 3, 2 };
        private readonly long[] luckStrArrB = { 800, 300, 208, 108, 100, 88, 66, 58, 50 };
        private readonly long[] luckStrArrC = { 10000, 5000, 3000, 2088, 1266, 1000, 818, 666, 518 };

        #region 金蛋奖励
        private static readonly string[] eggsLuckC = {
           "C8","C6","C8","C6","C8","C6","C8","C6","C8","C6","C8","C6","C9","C6","C9","C6","C8","C6","C8","C6",
           "C8","C6","C8","C6","C9","C7","C9","C6","C8","C7","C9","C6","C9","C7","C8","C7","C8","C7","C8","C6",
           "C9","C6","C4","C6","C8","C6","C9","C7","C9","C7","C9","C7","C4","C6","C8","C7","C5","C7","C8","C6",
           "C5","C6","C4","C6","C9","C6","C9","C6","C8","C7","C9","C6","C8","C7","C3","C7","C8","C7","C9","C6",
           "C9","C6","C3","C7","C9","C7","C5","C7","C9","C7","C1","C7","C9","C7","C2","C7","C5","C7","C5","C6"
        };
        #endregion
        #region  银蛋奖励
        private static readonly string[] eggsLuckB = {
           "B9","B7","B9","B7","B9","B7","B9","B7","B9","B7","B9","B7","B6","B7","B9","B7","B9","B7","B9","B7",
           "B6","B7","B9","B7","B6","B7","B6","B7","B6","B7","B9","B7","B6","B8","B9","B7","B6","B7","B9","B7",
           "B6","B8","B6","B7","B6","B7","B9","B7","B6","B7","B6","B8","B9","B7","B5","B8","B6","B8","B8","B8",
           "B5","B7","B8","B8","B8","B7","B9","B8","B5","B8","B8","B8","B6","B8","B4","B8","B9","B7","B6","B7",
           "B5","B8","B4","B8","B4","B8","B5","B7","B3","B8","B2","B8","B3","B7","B9","B7","B8","B7","B1","B8"
        };
        #endregion
        #region 鸡蛋奖励
        private static readonly string[] eggsLuckA = {
          "A8","A7","A8","A7","A8","A7","A8","A7","A8","A7","A8","A7","A8","A7","A8","A7","A8","A7","A8","A7",
          "A8","A7","A8","A7","A5","A7","A3","A7","A5","A7","A5","A7","A5","A7","A8","A8","A6","A7","A6","A7",
          "A5","A7","A6","A7","A4","A8","A5","A7","A5","A8","A6","A8","A2","A8","A6","A7","A3","A7","A4","A7",
          "A9","A8","A4","A8","A6","A7","A4","A8","A4","A7","A8","A7","A5","A7","A4","A8","A3","A8","A9","A8",
          "A9","A7","A6","A7","A6","A8","A9","A8","A9","A7","A1","A7","A8","A7","A6","A8","A2","A8","A6","A7"
        };
        # endregion

        public ActivityRepository GetRepository()
        {
            return new ActivityRepository(DbName, MongoHost);
        }

        public EggsController()
        {
            _respoRepository = new SqlDataRepository(SqlConnectString);
            _config = EggsConfig.GetConfig();
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

                //是否为卿(S)渠道用户(排除月宝和季宝)
                var iswqwlCps = false;
                var memberChennel = _respoRepository.GetMemberChennel(userId);

                if (memberChennel?.Channel != null && memberChennel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase))
                    iswqwlCps = true;
                //else if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
                //  notFirst = true;

                var shares = _respoRepository.GetProductTypeShares(userId, _config.StartTime, _config.EndTime).ToList();

                if (iswqwlCps)
                    Logger.Info($"ChannelMemberBefore :MemberId : {UserInfo.Id} ,Data: {shares.ToJson()}");

                //排除月宝和季宝
                if (iswqwlCps && shares.Count > 0)
                {
                    foreach (var item in shares)
                    {
                        if (item.ProductTypeId == 5)
                            item.BuyShares = 0;
                        else if (item.ProductTypeId == 6)
                            item.BuyShares = 0;
                    }
                    Logger.Info($"ChannelMemberAfter :MemberId : {UserInfo.Id} ,Data: {shares.ToJson()}");
                }

                long count = 0;
                foreach (var r in shares)
                {
                    long add;
                    SwitchMethod(r, out add);
                    count += add > 0 ? add : 0;
                }

                //用户投资获得砸蛋次数
                var totalCnt = (int)count / 12 / 100;

                //使用次数
                var totalChanceModel = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == userId).FirstOrDefault();

                //好友给予次数
                var friendGiveCount = GetRepository().Query<FriendTotalChanceModel>(it => it.Key == Key && it.FriendId == userId).Sum(it => it.HelpCount);

                if (totalChanceModel != null)
                {
                    var useCount = totalChanceModel.Used;
                    //使用次数
                    total.Used = useCount;
                }

                total.Key = Key;
                total.MemberId = userId;
                total.Total = totalCnt + friendGiveCount;
                total.Score = friendGiveCount;
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

            var userChance = GetRepository().Query<TotalChanceModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).FirstOrDefault();

            canUse = 0;
            //Used 自己使用  NotUse 给别人使用
            if (userChance != null)
                canUse = userChance.Total - userChance.Used - userChance.NotUsed;

            return userChance;
        }

        /// <summary>
        /// 使用机会
        /// </summary>
        /// <param name="type"> 1, 金蛋 2, 银蛋  3,鸡蛋</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Give(int type = 0)
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
            {
                return Json(validateRes);
            }

            int canUse;
            var memberData = SelectCount(out canUse);
            if (type == 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse } });
            if (type < 1 || type > 3)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "请选择喜蛋种类~" });
            if (type > 0 && canUse <= 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "快去投资，砸开喜蛋吧~" });
            if ((type == 1 && canUse < 1) || (type == 2 && canUse < 10) || (type == 3 && canUse < 100))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "房金币不足哟~" });


            long random = 0;
            var useCount = 0;
            string[] luck = { };
            long[] luckStrSel = { };

            //Redis 取值从1开始
            switch (type)
            {
                case 3:
                    random = RedisManager.GetIncrement("activity :" + Key + "_eggsLuckA") - 1;
                    luck = eggsLuckA;
                    luckStrSel = luckStrArrA;
                    useCount = 1;
                    break;
                case 2:
                    random = RedisManager.GetIncrement("activity :" + Key + "_eggsLuckB") - 1;
                    luck = eggsLuckB;
                    luckStrSel = luckStrArrB;
                    useCount = 10;
                    break;
                case 1:
                    random = RedisManager.GetIncrement("activity :" + Key + "_eggsLuckC") - 1;
                    luck = eggsLuckC;
                    luckStrSel = luckStrArrC;
                    useCount = 100;
                    break;
            }

            long money = 0;
            long s = random % 100;
            var luckNum = luck[s];
            List<long> resultLuckData;
            int removeNum = 0;
            switch (luckNum)
            {
                case "A1": money = luckStrArrA[0]; removeNum = 0; break;
                case "A2": money = luckStrArrA[1]; removeNum = 1; break;
                case "A3": money = luckStrArrA[2]; removeNum = 2; break;
                case "A4": money = luckStrArrA[3]; removeNum = 3; break;
                case "A5": money = luckStrArrA[4]; removeNum = 4; break;
                case "A6": money = luckStrArrA[5]; removeNum = 5; break;
                case "A7": money = luckStrArrA[6]; removeNum = 6; break;
                case "A8": money = luckStrArrA[7]; removeNum = 7; break;
                case "A9": money = luckStrArrA[8]; removeNum = 8; break;

                case "B1": money = luckStrArrB[0]; removeNum = 0; break;
                case "B2": money = luckStrArrB[1]; removeNum = 1; break;
                case "B3": money = luckStrArrB[2]; removeNum = 2; break;
                case "B4": money = luckStrArrB[3]; removeNum = 3; break;
                case "B5": money = luckStrArrB[4]; removeNum = 4; break;
                case "B6": money = luckStrArrB[5]; removeNum = 5; break;
                case "B7": money = luckStrArrB[6]; removeNum = 6; break;
                case "B8": money = luckStrArrB[7]; removeNum = 7; break;
                case "B9": money = luckStrArrB[8]; removeNum = 8; break;

                case "C1": money = luckStrArrC[0]; removeNum = 0; break;
                case "C2": money = luckStrArrC[1]; removeNum = 1; break;
                case "C3": money = luckStrArrC[2]; removeNum = 2; break;
                case "C4": money = luckStrArrC[3]; removeNum = 3; break;
                case "C5": money = luckStrArrC[4]; removeNum = 4; break;
                case "C6": money = luckStrArrC[5]; removeNum = 5; break;
                case "C7": money = luckStrArrC[6]; removeNum = 6; break;
                case "C8": money = luckStrArrC[7]; removeNum = 7; break;
                case "C9": money = luckStrArrC[8]; removeNum = 8; break;
            }

            resultLuckData = GetRandomArray(8, 0, 9, removeNum, luckStrSel);

            //发放奖励
            var objId = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            new MemberRepository(SqlConnectString).GiveMoney(UserInfo.Id, money, 55, objId);

            GetRepository().Add(new LuckdrawModel
            {
                MemberId = UserInfo.Id,
                Phone = UserInfo.Phone,
                Prize = -1,
                Key = Key,
                Type = type.ToString(),
                Name = money + "元",
                Remark = "8月砸蛋活动-" + money + "元现金"
            });

            memberData.Used += useCount;
            memberData.Score += 1;
            GetRepository().Update(memberData);

            SelectCount(out canUse);
            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { LuckName = money, LuckNum = luckNum, Count = canUse, OtherData = resultLuckData } });
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
                default:
                    add = 0;
                    break;
            }
        }

        /// <summary>
        /// 乱序无重复集合
        /// </summary>
        /// <param name="number">生成数量</param>
        /// <param name="minNum">最小值</param>
        /// <param name="maxNum">最大值</param>
        /// <param name="removeNum">排除数</param>
        /// <param name="luckStrArr">返回剩余奖励集合</param>
        /// <returns></returns>
        public List<long> GetRandomArray(int number, int minNum, int maxNum, int removeNum, long[] luckStrArr)
        {
            //用于保存返回的结果     
            List<long> result = new List<long>();
            List<int> numList = new List<int>();
            Random random = new Random();
            int temp = 0;
            while (result.Count < number)
            {
                temp = random.Next(minNum, maxNum);
                if (!numList.Contains(temp) && temp != removeNum)
                {
                    result.Add(luckStrArr[temp]);
                    numList.Add(temp);
                }
            }
            return result;
        }

        /// <summary>
        /// 给好友赠送房金币
        /// </summary>
        /// <returns></returns>
        public ActionResult GiveFriend(int count = 0, string phone = "", string t = "")
        {
            var validateRes = Validate();
            if (validateRes.ErrorCode != ErrorCode.None)
                return Json(validateRes);

            if (count < 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "请输入正确的赠送数量哟~" });
            if (count > 0 && string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(t))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "参数有误~" });


            var friendInfo = new MemberModel();
            friendInfo = !string.IsNullOrEmpty(phone)
                ? _respoRepository.GetMemberId(phone)
                : new MemberRepository(SqlConnectString).GetMemberInfo(t);

            int canUse;
            var memberData = SelectCount(out canUse);

            if (count == 0)
                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse } });
            if(count > canUse)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "您的房金币不足哟~" });

            if (string.IsNullOrEmpty(friendInfo?.Phone))
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "好友已生成新的邀请链接~" });
            if (friendInfo.MemberId == UserInfo.Id)
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Data = "", Message = "不能赠送给自己哦O(∩_∩)O~~" });

            GetRepository().Add<FriendTotalChanceModel>(new FriendTotalChanceModel
            {
                MemberId = memberData.MemberId,
                Phone = long.Parse(memberData.Remark),
                Key = Key,
                FriendId = friendInfo.MemberId,
                FriendPhone = long.Parse(friendInfo.Phone),
                HelpCount = count,
                Date = DateTime.Now,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            });

            memberData.NotUsed = memberData.NotUsed + count;
            memberData.LastUpdateTime = DateTime.Now;

            GetRepository().Update<TotalChanceModel>(memberData);

            return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse - count } });
        }

        /// <summary>
        /// 奖励
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

                var resultData = GetRepository().Query<LuckdrawModel>(it => it.Key == Key && it.MemberId == UserInfo.Id).Select(it => new
                {
                    it.Type,
                    it.Name,
                    Date = it.CreateTime.ToString("yyyy.MM.dd")
                });

                return Json(new ResponseModel { ErrorCode = ErrorCode.None, Data = new { Count = canUse, RewardList = resultData } });
            }
            catch (Exception ex)
            {
                Logger.Error($"Eggs RewardList：{ex}");
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "服务繁忙~" });
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
    }
}
