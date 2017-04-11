using System;
using System.IO;
using System.Web.Mvc;
using FJW.Unit;
using FJW.Wechat.Rules;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Entities.Request;

namespace FJW.Wechat.WebApp
{
    public class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {

        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalWeixinContext.ExpireMinutes = 3。
            //WeixinContext.ExpireMinutes = 3;

        }


        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        }

       

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            //方法一（v0.1），此方法调用太过繁琐，已过时（但仍是所有方法的核心基础），建议使用方法二到四
            //var responseMessage =
            //    ResponseMessageBase.CreateFromRequestMessage(RequestMessage, ResponseMsgType.Text) as
            //    ResponseMessageText;

            //方法二（v0.4）
            //var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(RequestMessage);

            //方法三（v0.4），扩展方法，需要using Senparc.Weixin.MP.Helpers;
            //var responseMessage = RequestMessage.CreateResponseMessage<ResponseMessageText>();

            //方法四（v0.6+），仅适合在HandlerMessage内部使用，本质上是对方法三的封装
            //注意：下面泛型ResponseMessageText即返回给客户端的类型，可以根据自己的需要填写ResponseMessageNews等不同类型。
            var textRules = DependencyResolver.Current.GetServices(typeof(ITextRule));
            ResultMessage result = null;
            if (textRules != null)
            {
                var msg = new RuleMessage {Content = requestMessage.Content, Type = RuleMessageType.Text};
                foreach (var r in textRules)
                {
                    var t = r as ITextRule;
                    if (t != null)
                    {
                        result = t.Handle(msg);
                        if (msg.IsHandled)
                        {
                            break;
                        }
                    }
                }
            }
            if (result != null)
            {
                if (result.Type == RuleMessageType.Text)
                {
                    var response  = base.CreateResponseMessage<ResponseMessageText>();
                    response.Content = result.Content;
                    return response;
                }
                if (result.Type == RuleMessageType.Image)
                {
                    var response = base.CreateResponseMessage<ResponseMessageImage>();
                    response.Image = new Image {MediaId = result.Content};
                    return response;
                }
            }

            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            Logger.Dedug("OnTextRequest: " + requestMessage.ToJson());
            return responseMessage;
        }


        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            //所有没有被处理的消息会默认返回这里的结果
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "";
            Logger.Dedug("DefaultResponseMessage requestMessage:" +responseMessage.ToJson());
            return responseMessage;
        }
    }
}