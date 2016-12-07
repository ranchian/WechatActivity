
using Newtonsoft.Json;

namespace FJW.SDK2Api.Member
{
    public class LoginResult
    {
        /// <summary>
        /// 登录标识
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonProperty("member")]
        public MemberInfo Member { get; set; }
    }

    public class MemberInfo
    {
        #region 基础
        /// <summary>
        /// 头像
        /// </summary>
        [JsonProperty("headPhoto")]
        public string HeadPhoto { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// 实名认证
        /// </summary>
        [JsonProperty("realNameAuthen")]
        public int RealNameAuthen { get; set; }

        /// <summary>
        /// 银行卡是否绑定
        /// </summary>
        [JsonProperty("bankCardAuthen")]
        public int BankCardAuthen { get; set; }

        /// <summary>
        /// 邀请人手机号
        /// </summary>
        [JsonProperty("friendPhone")]
        public string FriendPhone { get; set; }

        /// <summary>
        /// 好友状态
        /// </summary>
        [JsonProperty("friendStatus")]
        public int FriendStatus { get; set; }

        /// <summary>
        /// deviceId
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// deviceInfo
        /// </summary>
        [JsonProperty("deviceInfo")]
        public string DeviceInfo { get; set; }
        #endregion

        #region 地址
        /// <summary>
        /// 收货地址
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        [JsonProperty("contactBoy")]
        public string ContactBoy { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [JsonProperty("contactPhone")]
        public string ContactPhone { get; set; }

        /// <summary>
        /// 邮编
        /// </summary>
        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }
        #endregion

        #region 实名
        /// <summary>
        /// 实名
        /// </summary>
        [JsonProperty("realName")]
        public string RealName { get; set; }

        /// <summary>
        /// 身份证
        /// </summary>
        [JsonProperty("cardId")]
        public string CardId { get; set; }

        /// <summary>
        /// 是否设置过交易密码
        /// </summary>
        [JsonProperty("existsTradePswd")]
        public bool ExistsTradePswd { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        #endregion

        #region 银行卡
        /// <summary>
        /// 银行卡
        /// </summary>
        [JsonProperty("bankCardId")]
        public string BankCardId { get; set; }

        /// <summary>
        /// 所属银行
        /// </summary>
        [JsonProperty("bankName")]
        public string BankName { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        [JsonProperty("openAccountName")]
        public string OpenAccountName { get; set; }

        /// <summary>
        /// 银行图标
        /// </summary>
        [JsonProperty("bankIcon")]
        public string BankIcon { get; set; }

        /// <summary>
        /// 单笔交易限额
        /// </summary>
        [JsonProperty("singlePrice")]
        public string SinglePrice { get; set; }

        /// <summary>
        /// 每日限额
        /// </summary>
        [JsonProperty("dayPrice")]
        public string DayPrice { get; set; }

        /// <summary>
        /// 银行简称
        /// </summary>
        [JsonProperty("payCode")]
        public string PayCode { get; set; }

        #endregion
    }
}
