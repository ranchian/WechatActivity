
using Newtonsoft.Json;

namespace FJW.Wechat.WebApp.Models
{
    /// <summary>
    /// web请求 返回数据
    /// </summary>
    public class ResponseModel
    {

        public ResponseModel()
        {
            
        }

        public ResponseModel(ErrorCode code):this()
        {
            ErrorCode = code;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty("success")]
        public bool IsSuccess {
            get { return ErrorCode == ErrorCode.None; }
        }

        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        public object Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("code")]
        public ErrorCode ErrorCode { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public enum ErrorCode
    {
        /// <summary>
        /// 无错误，正常
        /// </summary>
        None,

        /// <summary>
        /// 未登录
        /// </summary>
        NotLogged,

        /// <summary>
        /// 未验证
        /// </summary>
        NotVerified,

        /// <summary>
        /// 异常
        /// </summary>
        Exception,

        /// <summary>
        /// 其他
        /// </summary>
        Other
    }
}