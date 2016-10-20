using FJW.Unit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace FJW.SDK2Api.Statics
{
    public class UploadApi
    {
        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="t"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ApiResponse<UploadFileResult> UploadImg( Stream stream, FileType t, string fileName)
        {
            stream.Position = 0;
            var bytes = new byte[stream.Length];

            stream.Read(bytes, 0, bytes.Length);
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            
            var dict = new Dictionary<string, string>();
            dict.Add("fileName", fileName);
            dict.Add("uploader", t.ToString());
            dict.Add("stream", Convert.ToBase64String(bytes));
            
            
            var reqestData = new ApiRequestData
            {
                Method = "uploadFile",
                Data = dict.ToJson()
            };
            var conf = ApiConfig.Section.Value.Methods["UploadFile"];
#if DEBUG
            Logger.Log("url:{0}", conf.EntryPoint);
#endif

            var result = HttpUnit.Post(conf.EntryPoint, reqestData.ToJson(), Encoding.UTF8);
#if DEBUG
            Logger.Log("req over:{0}", result.ToJson());
#endif

            if (result.Code == HttpStatusCode.OK)
            {
                var responseData = result.Reponse.Deserialize<ApiResponse>();
                if (responseData != null)
                {
                    return responseData.Convert<UploadFileResult>();
                }
            }
            return new ApiResponse<UploadFileResult>();
        }
    }

    public enum FileType
    {
        /// <summary>
        /// 没有指定类型
        /// </summary>
        None ,

        /// <summary>
        /// 头像
        /// </summary>
        Avator = 1,

        /// <summary>
        /// 身份证
        /// </summary>
        CardId = 2,

        /// <summary>
        /// 银行卡
        /// </summary>
        BankCard = 3
    }
}
