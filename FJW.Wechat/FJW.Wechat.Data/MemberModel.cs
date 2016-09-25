namespace FJW.Wechat.Data
{
    /// <summary>
    /// 用户
    /// </summary>
    public class MemberModel
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long MemberId { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }
    }
}