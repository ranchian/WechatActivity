namespace FJW.Wechat.Rules
{
    /// <summary>
    /// 文本处理规则
    /// </summary>
    public interface ITextRule
    {
        ResultMessage Handle(RuleMessage msg );
    }

    
}
