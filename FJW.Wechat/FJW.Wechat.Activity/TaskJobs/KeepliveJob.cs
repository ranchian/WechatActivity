using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.Unit;
using FJW.Wechat.Activity.Controllers;
using FJW.Wechat.Data;
using Quartz.Util;
using FJW.SDK2Api.CardCoupon;
using FJW.Wechat.Cache;
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
                string url = WebConfigurationManager.AppSettings["SplitloversUrl"];
                HttpUnit.GetString(url);
                Logger.Info("KeepliveJob");
            }
            catch (Exception ex)
            {
                Logger.Info("KeepliveJob:{0}", ex.ToString());
            }
        }


    }
}
