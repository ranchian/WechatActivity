using FJW.Unit.Log;
using System;

namespace FJW.Unit
{
    public static class Logger
    {
        private static readonly ILogger _log;

        static Logger()
        {
            _log = new NLogger();
        }

        public static void Dedug(string message, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _log.Debug(message);
            }
            else
            {
                _log.Debug(string.Format(message, args));
            }
        }

        public static void Info(string message, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _log.Info(message);
            }
            else
            {
                _log.Debug(string.Format(message, args));
            }
        }

        public static void Error(Exception exception)
        {
            _log.Error(exception.ToJson());
        }

        public static void Error(string message, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _log.Error(message);
            }
            else
            {
                _log.Error(string.Format(message, args));
            }
        }
    }
}