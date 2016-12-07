
using System.IO;
using System.Net;
using FJW.Unit;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using Newtonsoft.Json;


namespace FJW.Wechat.Activity.Unit
{
    public class QiniuHelper
    {

        private static readonly Mac MacKey = new Mac("byw00Jg2TMdEA4RW2nhYc0BdbdaItyGxpeRmdhVC",
            "kZaexHDVGsfDOlLQGjdlci844KIwnEvCTsiqYE_I");

        public static string UploadToken(int deleteAfterDays = 15, string scope = "upload-file", int expires = 60)
        {
            var putPolicy = new PutPolicy {Scope = scope, DeleteAfterDays = deleteAfterDays };
            putPolicy.SetExpires(expires);
            return Auth.createUploadToken(putPolicy, MacKey);
        }

        public static void UploadStream(Stream fs, string key)
        {
            var manager = new UploadManager();
            manager.uploadStream(fs, key, UploadToken(), null , delegate (string fileKey, ResponseInfo respInfo, string response)
            {
                Logger.Dedug("fileKey:{0}, respInfo:{1}, response:{2}", fileKey, respInfo.ToJson(), response);
            });
        }

        public static void UploadData(byte[] bytes, string key)
        {
            var manager = new UploadManager();
            manager.uploadData(bytes, key, UploadToken(), null, delegate (string fileKey, ResponseInfo respInfo, string response)
            {
                Logger.Dedug("fileKey:{0}, respInfo:{1}, response:{2}", fileKey, respInfo.ToJson(), response);
            });
        }

        public static FaceResult CheckFace(string key, string domain = "http://static.fangjinnet.com")
        {
            var url = $"{domain}/{key}?tusdk-face/detection";
            var result = HttpUnit.GetString(url);
            if (!string.IsNullOrEmpty(result))
            {
                Logger.Dedug("key:{1}, tusdk-face/detection:{0}, url:{2}", result, key, url);
                return result.Deserialize<FaceResult>()?? new FaceResult();
            }
            return new FaceResult();
        }
    }

    public class FaceResult
    {
        [JsonProperty("ret")]
        public int Code { get; set; }


        [JsonProperty("message")]
        public string Message { get; set; }


        [JsonProperty("data")]
        public bool Data { get; set; }


        [JsonProperty("ttp")]
        public long Timestamp { get; set; }
    }
}
