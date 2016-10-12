
using System.Collections.Generic;
using System.Net;
using System.Text;
using FJW.Unit;

namespace FJW.SDK2Api.Member
{
    /// <summary>
    /// 账户
    /// </summary>
    public class AccountApi
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static ApiResponse<LoginResult> Login(string phone, string pwd)
        {
            var dict = new Dictionary<string, string> {
                { "Phone", phone},
                { "Pswd", pwd},
                { "MemberId","0" },
                { "Domain","Wechat" },
                { "DeviceType","4" },
                { "DeviceId","" },
            };


            var reqestData = new ApiRequestData
            {
                Method = "MemberService.Login",
                Data = dict.ToJson()
            };

            var conf = ApiConfig.Section.Value.Methods["Login"];
            Logger.Log("url:{0}", conf.EntryPoint);
            var result = HttpUnit.Post(conf.EntryPoint, reqestData.ToJson(), Encoding.UTF8);
            Logger.Log("req over:{0}",  result.ToJson());
            if (result.Code ==  HttpStatusCode.OK )
            {
                var responseData = result.Reponse.Deserialize<ApiResponse>();
                if (responseData != null )
                {
                    return responseData.Convert<LoginResult>();
                }
            }
            return new ApiResponse<LoginResult>();
        }

        public static ApiResponse Regist(string phone, string pwd, string vcode,string inviterPhone, string channel)
        {
            var dict = new Dictionary<string, string>
            {
                {"Phone", phone},
                {"VCode", vcode},
                {"InviterPhone", inviterPhone},
                {"Channel", channel},
                { "Pswd", pwd}
            };
            var reqestData = new ApiRequestData
            {
                Method = "MemberService.Regist",
                Data = dict.ToJson()
            };

            var conf = ApiConfig.Section.Value.Methods["Regist"];
            var result = HttpUnit.Post(conf.EntryPoint, reqestData.ToJson(), Encoding.UTF8);
            if (result.Code == HttpStatusCode.OK)
            {
                return result.Reponse.Deserialize<ApiResponse>();
            }
            return new ApiResponse();
        }

    }
}
