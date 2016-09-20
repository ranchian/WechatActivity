using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Unit.Log
{
    public interface ILogger
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
    }
}
