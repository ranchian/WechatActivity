using StackExchange.Redis;
using System;

namespace FJW.Unit
{
    public static class RedisManager
    {
        private static Lazy<ConnectionMultiplexer> _redisConnection;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Init(string connectionString)
        {
            _redisConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }

        /// <summary>
        /// 释放 Redis连接
        /// </summary>
        public static void Disponse()
        {
            if (_redisConnection != null)
            {
                _redisConnection.Value.Dispose();
            }
        }

        private static IDatabase GetDataBase()
        {
            return _redisConnection.Value.GetDatabase();
        }

        private static IDatabase GetDataBase(int db)
        {
            return _redisConnection.Value.GetDatabase(db);
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
        /// 获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : class
        {
            var str = Get(key);
            if (str.IsNullOrEmpty())
            {
                return null;
            }
            return str.Deserialize<T>();
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
        public static bool Set(string key, string value, int expireSeconds = 0)
        {
            var db = GetDataBase();
            return expireSeconds > 0 ? db.StringSet(key, value, TimeSpan.FromSeconds(expireSeconds)) : db.StringSet(key, value);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        public static bool Set(string key, object obj, int expireSeconds = 0)
        {
            return Set(key, obj.ToJson(), expireSeconds);
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

        /// <summary>
        /// 获取自增数字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long GetIncrement(string key)
        {
            return GetDataBase().StringIncrement(key);
        }
    }
}