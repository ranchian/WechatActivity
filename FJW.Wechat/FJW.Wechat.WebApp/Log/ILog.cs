using System;

namespace FJW.Wechat.WebApp
{
    interface ILog
    {
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="msg"></param>
        void Debug(string msg);

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ex"></param>
        void Error(Exception ex);

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="msg"></param>
        void Error(string msg);

        /// <summary>
        /// 数据库日志
        /// </summary>
        /// <param name="msg"></param>
        void Database(string msg);
    }
}
