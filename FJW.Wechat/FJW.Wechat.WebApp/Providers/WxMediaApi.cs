
using System.IO;
using FJW.Unit;
using FJW.Wechat.Wx;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Containers;

namespace FJW.Wechat.WebApp.Providers
{
    public class WxMediaApi : IWxMediaApi
    {
        public void Get(string url, Stream stream)
        {
            var token = AccessTokenContainer.GetAccessToken(Config.WechatConfig.AppId);
            Logger.Dedug("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, url);
            MediaApi.Get(token, url, stream);
        }

        public byte[] Get(string mediaId)
        {
            var token = AccessTokenContainer.GetAccessToken(Config.WechatConfig.AppId);
            var url = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}",
                token, mediaId);
            return HttpUnit.GetData(url);
        }
    }
}