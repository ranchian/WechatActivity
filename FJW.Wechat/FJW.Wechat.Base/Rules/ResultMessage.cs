
namespace FJW.Wechat.Rules
{
    /// <summary>
    /// 结果消息
    /// </summary>
    public class ResultMessage
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public RuleMessageType Type { get; set; }
    }
}
