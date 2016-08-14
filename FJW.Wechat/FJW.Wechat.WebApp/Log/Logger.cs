using System;

namespace FJW.Wechat.WebApp
{
    /// <summary>
    /// 日志
    /// </summary>
    public class Logger
    {
        private static readonly ILog Log = new NLogImp();

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="str"></param>
        public static void Debug(string str)
        {
            Log.Debug(str);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="str"></param>
        public static void Error(string str)
        {
            Log.Error(str);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ex"></param>
        public static void Error(Exception ex)
        {
            Log.Error(ex);
        }

        /// <summary>
        /// 数据库日志
        /// </summary>
        /// <param name="str"></param>
        public static void Database(string str)
        {
            Log.Database(str);
        }
    }
}
