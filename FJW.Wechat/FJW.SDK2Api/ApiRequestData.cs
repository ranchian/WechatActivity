using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FJW.SDK2Api
{
    public class ApiRequestData
    {

        /// <summary>
        /// 数据 json格式
        /// Data
        /// </summary>

        public string Data { get; set; }

        /// <summary>
        /// 调用方法名
        /// Method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 客户端版本号
        /// </summary>
        public int V { get; set; }

        /// <summary>
        /// 设备ID
        /// DeviceID
        /// </summary>
        public string Did { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        public string G { get; set; }

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
        public string T { get; set; }

        /// <summary>
        /// 时间戳
        /// TimeStamp
        /// </summary>
        public long Ts { get; set; }

        /// <summary>
        /// 平台标识(PlatForm,1:Android;2:IOS)
        /// </summary>
        public int P { get; set; }

        /// <summary>
        /// 服务间调用用MemberID
        /// </summary>
        public int Mid { get; set; }

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
