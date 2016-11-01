namespace FJW.Wechat.WebApp.Base
{
    public class UserInfo
    {
        public long Id { get; set; }

        public string Phone { get; set; }

        public string Token { get; set; }

        #region 微信

        /// <summary>
        /// 微信用户的唯一标识
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 微信用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 性别( 1:男 2:女)
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }

        public string Country { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string HeadimgUrl { get; set; }

        public string Privilege { get; set; }

        /// <summary>
        /// 只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段
        /// </summary>
        public string UnionId { get; set; }

        #endregion 微信
    }
}