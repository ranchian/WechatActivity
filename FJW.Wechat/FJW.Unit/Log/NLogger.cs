﻿using System;

using NLog;

namespace FJW.Unit.Log
{
    internal class NLogger: ILogger
    {
        private static readonly NLog.Logger NLog = LogManager.GetCurrentClassLogger();
        public void Debug(string msg)
        {
            NLog.Debug(msg);
        }

        public void Error(Exception ex)
        {
            NLog.Error(ex.ToJson());
        }

        public void Error(string msg)
        {
            NLog.Error(msg);
        }

         
    }
}