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

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static T Deserialize<T>(this string str)
        {
            if (str == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}