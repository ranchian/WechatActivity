using System;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Rules;

namespace FJW.Wechat.Activity.Rules
{
    /// <summary>
    /// 开宝箱口令规则
    /// </summary>
    public class BoxWordRule : ITextRule
    {
        public ResultMessage Handle(RuleMessage msg)
        {
            var config = BoxWordConfig.GetConfig();
            if (msg.Content != null && config.StartTime <= DateTime.Now && DateTime.Now < config.EndTime)
            {
                msg.IsHandled = true;
                if (msg.Content.Equals("开宝箱"))
                {
                    var days = (int)(DateTime.Now.Date - config.StartTime.Date).TotalDays;
                    var word = GetWord(days);
                    var result = new ResultMessage { Type = RuleMessageType.Text, Content = word };
                    //Logger.Dedug("BoxWordRule Handled:"+ result.ToJson());
                    return result;
                }
                if (msg.Content =="开宝" || msg.Content == "宝箱" || msg.Content == "天天宝箱" || msg.Content == "天天开宝箱" || msg.Content == "开宝箱口令")
                {
                    var result = new ResultMessage { Type = RuleMessageType.Text, Content = "请核对关键字，输入正确关键字获取今日口令~" };
                    return result;
                }
            }
            
            return null;
        }

        public static string GetWord(int days)
        {
            switch (days)
            {
                case 0:
                    return "房金网实缴注册1个亿";
                case 1:
                    return "专业风控";
                case 2:
                    return "利润可观";
                case 3:
                    return "技术保障";
                case 4:
                    return "隐私保障";
                case 5:
                    return "始终坚持实事求是的理念";
                case 6:
                    return "打造全球金融服务平台新标杆";
                case 7:
                    return "中国金融管理协会理事单位";
                case 8:
                    return "平安银行授信企业提供本息兑付担保";
                case 9:
                    return "新手专享16%预期年化收益";
                default:
                    return "";
            }

        }
    }


}
