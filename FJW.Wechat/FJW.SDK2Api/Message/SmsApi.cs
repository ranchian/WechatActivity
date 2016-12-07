using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using FJW.Unit;

namespace FJW.SDK2Api.Message
{
    public  class SmsApi
    {
        public static ApiResponse Send(string phone, string content)
        {
            var dict = new Dictionary<string, object> {
                { "Phone", phone},
                { "Content", content}
            };


            var reqestData = new ApiRequestData
            {
                Method = "BasicService.SendSmsText",
                Data = dict.ToJson()
            };

            var conf = ApiConfig.Section.Value.Methods["BasicService"];
            
            var result = HttpUnit.Post(conf.EntryPoint, reqestData.ToJson(), Encoding.UTF8);


            if (result.Code == HttpStatusCode.OK)
            {
                return result.Reponse.Deserialize<ApiResponse>();
            }
            return new ApiResponse { Status = ServiceResultStatus.Error };
        }
    }
}
