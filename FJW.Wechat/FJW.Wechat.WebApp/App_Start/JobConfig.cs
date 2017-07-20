
using FJW.Wechat.Activity.TaskJobs;
using Quartz;
using FJW.Wechat.Activity.Controllers.TaskJobs;

namespace FJW.Wechat.WebApp
{
    public class JobConfig
    {
        private static IScheduler _scheduler = null;

        public static void Start()
        {
            _scheduler = Quartz.Impl.StdSchedulerFactory.GetDefaultScheduler();

            _scheduler.Start();

            IJobDetail updateJob = JobBuilder.Create<FlowersTaskJob>().WithIdentity("UpdateJob", "MinuteGroup").Build();

            ITrigger upDateTrigger = TriggerBuilder.Create()
                    .WithIdentity("UpdateJob", "MinuteGroup")
                    .WithCronSchedule("0 0 0 31 * ? ")
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

    }
}