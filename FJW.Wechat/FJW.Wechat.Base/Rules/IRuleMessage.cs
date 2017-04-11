
namespace FJW.Wechat.Rules
{
    /// <summary>
    /// 规则
    /// </summary>
    interface IRuleMessage
    {
       /// <summary>
       /// 类型
       /// </summary>
        RuleMessageType Type { get; set; }

        string FromUser { get; set; }

        /// <summary>
        /// 是否已经处理了
        /// </summary>
        bool IsHandled { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        string Content { get; set; }

    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum RuleMessageType
    {

        /// <summary>
        /// 文本
        /// </summary>
        Text,

        /// <summary>
        /// 图片
        /// </summary>
        Image,
    }
}
