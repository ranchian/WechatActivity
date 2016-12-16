using FJW.Wechat.Activity.Controllers;
using FJW.Wechat.Activity.TaskJobs;
using FJW.Wechat.Cache;
using Quartz;

using System.ServiceProcess;
using System.Threading;


namespace FJW.Wechat.WebApp
{
    public class JobService : ServiceBase
    {
        private CancellationToken _cancelToken;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly IScheduler _scheduler = Quartz.Impl.StdSchedulerFactory.GetDefaultScheduler();
        public JobService()
        {

            _cancelTokenSource = new CancellationTokenSource();
        }
        //获取奖励配置
        private static SplitloversConfig GetConfig()
        {
            return JsonConfig.GetJson<SplitloversConfig>("Config/activity.splitloversvalue.json");
        }

        protected override void OnStart(string[] args)
        {
            _cancelToken = _cancelTokenSource.Token;
            _scheduler.Start();

            var config = GetConfig(); 

            IJobDetail updateJob = JobBuilder.Create<ITaskJob>().WithIdentity("UpdateJob", "MinuteGroup").Build();

            ITrigger upDateTrigger = TriggerBuilder.Create()
                    .WithIdentity("UpdateJob", "MinuteGroup")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(config.Hour, config.Minute))
                    .ForJob(updateJob)
                    .Build();

            _scheduler.ScheduleJob(updateJob, upDateTrigger);

            IJobDetail keepliveJob = JobBuilder.Create<KeepliveJob>().WithIdentity("KeepliveJobJob", "KeepliveGroup").Build();
            ITrigger keepliveTrigger = TriggerBuilder.Create()
                    .WithIdentity("KeepliveJobJob", "KeepliveGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()).ForJob(keepliveJob)
                    .Build();

            _scheduler.ScheduleJob(keepliveJob, keepliveTrigger);

        }

        protected override void OnStop()
        {
            _scheduler.Shutdown();

            _cancelTokenSource.Cancel();
        }

        /// <summary>
        /// 调试
        /// </summary>
        /// <param name="args"></param>
        public void Debug(string[] args)
        {
            OnStart(args);
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Quit()
        {
            OnStop();
        }
    }
}