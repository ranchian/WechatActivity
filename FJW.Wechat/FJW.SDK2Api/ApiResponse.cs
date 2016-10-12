
using FJW.Unit;
using Newtonsoft.Json;

namespace FJW.SDK2Api
{
    public class ApiResponse 
    {
        /// <summary>
        /// 数据
        /// Data
        /// </summary>
        [JsonProperty("d")]
        public string Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tt")]
        public long Tt { get; set; }

        /// <summary>
        /// APP版本号（APPVersion）
        /// </summary>
        [JsonProperty("v")]
        public int Version { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        [JsonProperty("g")]
        public string Guid { get; set; }

        /// <summary>
        /// 是否加密
        /// </summary>
        [JsonProperty("ie")]
        public bool Ie { get; set; }

        /// <summary>
        /// 加密版本号
        /// </summary>
        [JsonProperty("e")]
        public string E { get; set; }

        /// <summary>
        /// 状态
        /// Status
        /// </summary>
        [JsonProperty("s")]
        public ServiceResultStatus Status { get; set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        [JsonProperty("es")]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// 低频数据缓存版本号IOS用（IOSDataVersion）
        /// </summary>
        [JsonProperty("idv")]
        public string Idv { get; set; }

        /// <summary>
        /// 低频数据缓存版本号Android用（AndroidDataVersion）
        /// </summary>
        [JsonProperty("adv")]
        public string Adv { get; set; }

        public bool IsOk
        {
            get { return Status == ServiceResultStatus.Ok; }
        }

        public static ApiResponse<T> Convert<T>(ApiResponse response) where T : class
        {
            return new ApiResponse<T>
            {
                Data = response.Data,
                ExceptionMessage = response.ExceptionMessage,
                Status = response.Status,
                Content = response.Status == ServiceResultStatus.Ok? response.Data.Deserialize<T>(): null
            };
        }

        public ApiResponse<T> Convert<T>() where T : class
        {
            return Convert<T>(this);
        }
    }

    public class ApiResponse<T> : ApiResponse where T: class 
    {
        
        public T Content { get; set; }
        
    }

    public enum ServiceResultStatus
    {
        Ok = 0,
        Tip = 1,
        Error = 2,
        VersionUpdate = 3,
        InvalidParameter = 4,
        InvalidLogic = 5,
        InvalidToken = 101,
    }
}
