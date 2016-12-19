using System;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Wx;

namespace FJW.Wechat.WebApp.Providers
{
    public class MongoWxAuthenRepository : IWxAuthenRepository
    {
        public WxUserInfo GetUserInfo(string openId)
        {
            var wechatRepository = new WeChatRepository("Wechat", Config.WechatConfig.MongoHost);
            var r = wechatRepository.GetByOpenId(openId);
            if (r == null)
            {
                return null;
            }
            return new WxUserInfo
            {
                MemberId = r.MemberId,
                AccessToken = r.AccessToken,
                City = r.City,
                Country = r.Country,
                ExpiresIn = r.ExpiresIn,
                HeadimgUrl = r.HeadimgUrl,
                LastAuthorizeTime = r.LastAuthorizeTime,
                NickName = r.NickName,
                OpenId = r.OpenId,
                Privilege = r.Privilege,
                Province = r.Province,
                UnionId = r.UnionId,
                Sex = r.Sex
            };
        }

        public void Add(WxUserInfo u)
        {
            var wechatRepository = new WeChatRepository("Wechat", Config.WechatConfig.MongoHost);
            var model = new WeChatUserModel
            {
                MemberId = u.MemberId,
                AccessToken = u.AccessToken,
                City = u.City,
                Country = u.Country,
                ExpiresIn = u.ExpiresIn,
                HeadimgUrl = u.HeadimgUrl,
                LastAuthorizeTime = u.LastAuthorizeTime,
                NickName = u.NickName,
                OpenId = u.OpenId,
                Privilege = u.Privilege,
                Province = u.Province,
                UnionId = u.UnionId,
                Sex = u.Sex
            };
            wechatRepository.Add(model);
        }

    }
}