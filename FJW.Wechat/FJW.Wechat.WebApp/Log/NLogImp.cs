using System;
using NLog;
using Newtonsoft.Json;


namespace FJW.Wechat.WebApp
{
    class NLogImp:ILog
    {
        private static readonly NLog.Logger NLogger = LogManager.GetCurrentClassLogger();

        public void Debug(string msg)
        {
            NLogger.Debug(msg);
        }

        public void Error(Exception ex)
        {
            NLogger.Error(JsonConvert.SerializeObject(ex));
        }

        public void Error(string msg)
        {
            NLogger.Error(msg);
        }

        public void Database(string msg)
        {
            NLogger.Trace(msg);
        }
    }
}
