using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Rules;

namespace FJW.Wechat.Activity.Rules
{
    /// <summary>
    /// 开宝箱口令规则
    /// </summary>
    public class BoxWordRule: ITextRule
    {
        public ResultMessage Handle(RuleMessage msg)
        {
            var config = BoxWordConfig.GetConfig();
            if (msg.Content!= null && msg.Content.Equals("开宝箱") && config.StartTime <= DateTime.Now && DateTime.Now  > config.EndTime )
            {
                msg.IsHandled = true;
                var days = (int)(DateTime.Now.Date - config.StartTime.Date).TotalDays;
                var word = GetWord(days);
                return new ResultMessage { Type = RuleMessageType.Text, Content = word };
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
                    return "打造全球金融服务平台新标杆";
                case 6:
                    return "金融行业靠谱的房地产平台";
                case 7:
                    return "房地产行业靠谱的金融平台";
                case 8:
                    return "中国金融管理协会理事单位";
                case 9:
                    return "平安银行授信企业提供本息兑付担保";
                default:
                    return "";
            }
            
        }
    }


}
