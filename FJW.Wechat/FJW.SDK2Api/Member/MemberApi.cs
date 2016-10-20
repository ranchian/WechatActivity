using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FJW.SDK2Api.Statics;
using FJW.Unit;

namespace FJW.SDK2Api.Member
{
    public class MemberApi
    {
        public static ApiResponse UploadAvator(long memberId, string avatorUrl, FileType type)
        {
            var dict = new Dictionary<string, string>();

            dict.Add("MemberId", memberId.ToString());

            dict.Add("Type", type.ToString());

            dict.Add("Url", avatorUrl);

            var reqestData = new ApiRequestData
            {
                Method = "MemberService.UploadAvator",
                Data = dict.ToJson()
            };

            var conf = ApiConfig.Section.Value.Methods["MemberService"];
#if DEBUG
            Logger.Log("url:{0}", conf.EntryPoint);
#endif

            var result = HttpUnit.Post(conf.EntryPoint, reqestData.ToJson(), Encoding.UTF8);
#if DEBUG
            Logger.Log("req over:{0}", result.ToJson());
#endif

            if (result.Code == HttpStatusCode.OK)
            {
                return result.Reponse.Deserialize<ApiResponse>();
            }
            return new ApiResponse { Status = ServiceResultStatus.Tip, ExceptionMessage = "内部网络错误"};
        }
    }

    
}
