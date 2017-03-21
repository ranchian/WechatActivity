using Quartz;
using System;

using FJW.Unit;

using System.Web.Configuration;

namespace FJW.Wechat.Activity.TaskJobs
{
    public class KeepliveJob : IJob
    {
        /// <summary>
        /// 防止IIS回收
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                string url = WebConfigurationManager.AppSettings["ArborDayUrl"];
                HttpUnit.GetString(url);
                Logger.Info("KeepliveJob: OK");
            }
            catch (Exception ex)
            {
                Logger.Info("KeepliveJob:{0}", ex.ToString());
            }
        }


    }
}
