
using Newtonsoft.Json;

namespace FJW.Wechat.WebApp.Models
{
    /// <summary>
    /// web请求 返回数据
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        public object Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("code")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}