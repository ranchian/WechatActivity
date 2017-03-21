﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using FJW.Wechat.Activity.Controllers;
using FJW.Wechat.Activity.TaskJobs;
using FJW.Wechat.Cache;
using Quartz;

namespace FJW.Wechat.WebApp
{
    public class JobConfig
    {
        private static IScheduler _scheduler = null;

        public static void Start()
        {
            _scheduler = Quartz.Impl.StdSchedulerFactory.GetDefaultScheduler();

            //IScheduler _scheduler = Quartz.Impl.StdSchedulerFactory.GetDefaultScheduler();

            _scheduler.Start();

            var config = GetConfig();


            IJobDetail updateJob = JobBuilder.Create<ArbordayTaskJob>().WithIdentity("UpdateJob", "MinuteGroup").Build();

            ITrigger upDateTrigger = TriggerBuilder.Create()
                    .WithIdentity("UpdateJob", "MinuteGroup")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(config.Hour, config.Minute))
                    .ForJob(updateJob)
                    .Build();

            _scheduler.ScheduleJob(updateJob, upDateTrigger);

            IJobDetail keepliveJob = JobBuilder.Create<KeepliveJob>().WithIdentity("KeepliveJob", "KeepliveGroup").Build();
            ITrigger keepliveTrigger = TriggerBuilder.Create()
                    .WithIdentity("KeepliveJob", "KeepliveGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(600).RepeatForever()).ForJob(keepliveJob)
                    .Build();

            _scheduler.ScheduleJob(keepliveJob, keepliveTrigger);
        }

        public static void Stop()
        {
            if (_scheduler != null)
            {
                _scheduler.Shutdown();
            }
        }

        //获取奖励配置
        private static ArbordayConfig GetConfig()
        {
            return JsonConfig.GetJson<ArbordayConfig>("Config/activity.arbordayvalue.json");
        }


    }
}