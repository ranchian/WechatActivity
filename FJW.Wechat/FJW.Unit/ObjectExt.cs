
using Newtonsoft.Json;

namespace FJW.Unit
{
    public static class ObjectExt
    {
        /// <summary>
        /// 转化为Json字符
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
