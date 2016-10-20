using System;
using Newtonsoft.Json;

namespace FJW.Wechat.WebApp.Models
{
    /// <summary>
    /// 上传图片
    /// </summary>
    public class UploadImgModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}