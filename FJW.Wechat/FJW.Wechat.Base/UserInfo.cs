using FJW.Wechat.Wx;

namespace FJW.Wechat
{
    public class UserInfo
    {
        public long Id { get; set; }

        public string Phone { get; set; }

        public string Token { get; set; }

        public string OpenId { get; set; }

        #region 微信

        public WxUserInfo WxUserInfo { get; set; }

        #endregion 微信
    }
}