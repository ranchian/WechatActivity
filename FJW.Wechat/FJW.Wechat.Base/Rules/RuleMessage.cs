

namespace FJW.Wechat.Rules
{
    public class RuleMessage : IRuleMessage
    {

        public string FromUser { get; set; }

        public bool IsHandled { get; set; }

        public string Content { get; set; }

        public RuleMessageType Type { get; set; }
    }
}
