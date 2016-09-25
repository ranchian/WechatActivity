using System;
using System.ComponentModel.DataAnnotations.Schema;

using FJW.Model.MongoDb;

namespace FJW.Wechat.Data
{
    [Table("WeChatUser")]
    public class WeChatUserModel : BaseModel
    {
        public long MemberId { get; set; }

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

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        /// <summary>
        /// 到期时间（秒）
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// 最后授权时间
        /// </summary>
        public DateTime LastAuthorizeTime { get; set; }
    }
}