using System;
using System.Web.Configuration;

namespace FJW.Wechat.WebApp
{
    public static class Config
    {
        /// <summary>
        /// 微信配置
        /// </summary>
        public static WechatConfig WechatConfig { get; private set;}


        /// <summary>
        /// 活动配置
        /// </summary>
        public static ActivityConfig ActivityConfig { get; private set; }


        static Config()
        {
            var host = WebConfigurationManager.AppSettings["MongoHost"];
            WechatConfig = new WechatConfig
            {
                Token = WebConfigurationManager.AppSettings["WeixinToken"],
                EncodingAesKey = WebConfigurationManager.AppSettings["WeixinEncodingAESKey"],
                AppId = WebConfigurationManager.AppSettings["WeixinAppId"],
                AppSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"],
                MongoHost = host
            };

            ActivityConfig = new ActivityConfig
            {
                MongoHost = host,
                DbName = WebConfigurationManager.AppSettings["DbName"]
            };
        }

    }

    /// <summary>
    /// 微信配置
    /// </summary>
    public class WechatConfig
    {
        public string AppId { get; set; }

        public string EncodingAesKey { get; set; }

        public string Token { get; set; }

        public string AppSecret { get; set; }

        /// <summary>
        /// mongodb
        /// </summary>
        public string MongoHost { get; set; }
    }

    /// <summary>
    /// 活动配置
    /// </summary>
    public class ActivityConfig
    {
        public string MongoHost { get; set; }

        public string DbName { get; set; }

    }


  
    
}