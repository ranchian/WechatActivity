
using Newtonsoft.Json;

namespace FJW.SDK2Api
{
    public class ApiRequestData
    {
        /// <summary>
        /// 业务数据
        /// </summary>
        [JsonProperty("D")]
        public string Data { get; set; }

        /// <summary>
        /// 设备ID
        /// DeviceID
        /// </summary>
        [JsonProperty("Did")]
        public string DeviceId { get; set; }


        /// <summary>
        /// 调用方法名
        /// Method
        /// </summary>
        [JsonProperty("M")]
        public string Method { get; set; }

        /// <summary>
        /// 服务间调用用MemberID
        /// </summary>
        [JsonProperty("Mid")]
        public long MemberId { get; set; }

        /// <summary>
        /// 请求随机数
        /// </summary>
        [JsonProperty("G")]
        public string Guid { get; set; }

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool Ie { get; set; }


        /// <summary>
        /// 加密版本号
        /// </summary>
        public string E { get; set; }


        /// <summary>
        /// Token
        /// </summary>
        [JsonProperty("T")]
        public string Token { get; set; }


        /// <summary>
        /// 客户端ip
        /// </summary>
        [JsonProperty("remoteip")]
        public string Remoteip { get; set; }

        /// <summary>
        /// 平台标识(IOS或ANDROID或API或SERVICE)
        /// </summary>
        [JsonProperty("P")]
        public int Platform { get; set; }

        /// <summary>
        /// 客户端版本号
        /// </summary>
        [JsonProperty("version")]
        public int V { get; set; }

        /// <summary>
        /// 低频数据缓存版本号IOS用（IOSDataVersion）
        /// </summary>
        public string Idv { get; set; }

        /// <summary>
        /// 低频数据缓存版本号Android用（AndroidDataVersion）
        /// </summary>
        public string Adv { get; set; }
    }
}
