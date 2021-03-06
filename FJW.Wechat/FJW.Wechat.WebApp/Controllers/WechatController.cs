﻿using System;
using System.Web.Mvc;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.MvcExtension;
using Senparc.Weixin.MP.Entities.Request;

using FJW.Unit;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Helpers;

namespace FJW.Wechat.WebApp.Controllers
{
    [CrossDomainFilter]
    public class WechatController : WController
    {
        #region 认证

        // GET: Wechat
        public ActionResult Index(PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce,
                Config.WechatConfig.Token))
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                return
                    Content("failed:" + postModel.Signature + "," +
                            CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, Config.WechatConfig.Token) +
                            "。");
                /* + "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");*/
            }
        }

        [HttpPost]
        public ActionResult Index(PostModel postModel)
        {
            if (
                !CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce,
                    Config.WechatConfig.Token))
            {
                return Content("参数错误！");
            }
            
            postModel.Token = Config.WechatConfig.Token; //根据自己后台的设置保持一致
            postModel.EncodingAESKey = Config.WechatConfig.EncodingAesKey; //根据自己后台的设置保持一致
            postModel.AppId = Config.WechatConfig.AppId; //根据自己后台的设置保持一致

            //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
            var maxRecordCount = 10;
            /*
            var logPath = Server.MapPath(string.Format("~/App_Data/MP/{0}/", DateTime.Now.ToString("yyyy-MM-dd")));
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            */
            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var messageHandler = new CustomMessageHandler(Request.InputStream, postModel, maxRecordCount);
            try
            {
                //测试时可开启此记录，帮助跟踪数据，使用前请确保App_Data文件夹存在，且有读写权限。
                /* --- comment by chenpt
                messageHandler.RequestDocument.Save(Path.Combine(logPath, string.Format("{0}_Request_{1}.txt", _getRandomFileName(), messageHandler.RequestMessage.FromUserName)));
                if (messageHandler.UsingEcryptMessage)
                {
                    messageHandler.EcryptRequestDocument.Save(Path.Combine(logPath, string.Format("{0}_Request_Ecrypt_{1}.txt", _getRandomFileName(), messageHandler.RequestMessage.FromUserName)));
                }
                */
                /* 如果需要添加消息去重功能，只需打开OmitRepeatedMessage功能，SDK会自动处理。
                 * 收到重复消息通常是因为微信服务器没有及时收到响应，会持续发送2-5条不等的相同内容的RequestMessage*/
                messageHandler.OmitRepeatedMessage = true;

                //执行微信处理过程
                messageHandler.Execute();

                //测试时可开启，帮助跟踪数据

                //if (messageHandler.ResponseDocument == null)
                //{
                //    throw new Exception(messageHandler.RequestDocument.ToString());
                //}

                //return Content(messageHandler.ResponseDocument.ToString());//v0.7-
                //return new FixWeixinBugWeixinResult(messageHandler);//为了解决官方微信5.0软件换行bug暂时添加的方法，平时用下面一个方法即可
                return new WeixinResult(messageHandler); //v0.8+
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Content("");
            }
        }

        #endregion


        #region javascript share config
        [CrossDomainFilter]
        public ActionResult JsConfig(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return Json(new ResponseModel { Message = "无效地址", ErrorCode = ErrorCode.Other});
                }
                if (url.IndexOf("#", StringComparison.Ordinal) > -1)
                {
                    url = url.Substring(0, url.IndexOf("#", StringComparison.Ordinal) + 1);
                }
                
                var timestamp = JSSDKHelper.GetTimestamp();
                var nonceStr = JSSDKHelper.GetNoncestr();
                var appid = Config.WechatConfig.AppId;
                var jstoken = JsApiTicketContainer.GetJsApiTicket(appid); //CommonApi.GetTicket(appid, Config.WechatConfig.AppSecret).ticket;
                var signature = JSSDKHelper.GetSignature(jstoken, nonceStr, timestamp, url);
                //var tickt = CommonApi.GetTicket(appid, Config.WechatConfig.AppSecret);
                var d = new ResponseModel
                {
                    Data = new
                    {
                        appid,
                        timestamp,
                        nonceStr,
                        signature
                    }
                };
                return Json(d) ;
            }
            catch (Exception ex)
            { 
                Logger.Error(ex);
                return Json(new ResponseModel { Message = ex.Message, ErrorCode = ErrorCode.Exception});
            }
        }
        #endregion

    }
}