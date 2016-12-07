using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;

namespace FJW.Wechat.Cache
{
    public class JsonConfig
    {
      
        public static T GetJson<T>(string file) where T : class
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(root, file);
            var obj = HttpRuntime.Cache[file];
            if (obj != null)
            {
                return obj as T;
            }
            var jsonObj = ReadJson<T>(path);
            HttpRuntime.Cache.Insert(file, jsonObj, new CacheDependency(path));
            return jsonObj;
        }

        private static T ReadJson<T>(string path) where T : class
        {
            using (var steam = new FileStream(path, FileMode.Open))
            using (var reader = new StreamReader(steam))
            {
                var str = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(str))
                {
                    return JsonConvert.DeserializeObject<T>(str);
                }
                return null;
            }
        }
    }
}
