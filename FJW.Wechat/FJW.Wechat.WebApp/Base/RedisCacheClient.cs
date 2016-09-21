
using System;
using StackExchange.Redis;
 
namespace FJW.Wechat.WebApp.Base
{
    public static class RedisCacheClient
    {
        private static readonly Lazy<ConnectionMultiplexer> RedisConnection;
        private static readonly object LockObj;
        static RedisCacheClient()
        {
            LockObj = new object();
            RedisConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect( Config.RedisConfig.ConnectionString));
        }

        private static IDatabase GetDataBase()
        {
            return RedisConnection.Value.GetDatabase();
        }

        private static IDatabase GetDataBase(int db)
        {
            return RedisConnection.Value.GetDatabase(db);
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            return GetDataBase().StringGet(key);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            return GetDataBase().KeyDelete(key);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireSeconds">秒</param>
        /// <returns></returns>
        public static bool Set(string key,string value, int expireSeconds = 0)
        {
            var db = GetDataBase();
            return expireSeconds > 0 ? db.StringSet(key, value, TimeSpan.FromSeconds(expireSeconds)) : db.StringSet(key, value);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasKey(string key)
        {
            return GetDataBase().KeyExists(key);
        }

        public static long GetInter(string key)
        {
            return GetDataBase().StringIncrement(key);
        }
    }
}