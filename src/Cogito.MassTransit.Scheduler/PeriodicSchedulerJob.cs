using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cogito.DependencyInjection;
using Cogito.MassTransit.Scheduler.Util;

using Quartz;

namespace Cogito.MassTransit.Scheduler
{

    /// <summary>
    /// Periodically sets up the Periodic jobs.
    /// </summary>
    [AddTransientService<PeriodicSchedulerJob>()]
    public class PeriodicSchedulerJob : IJob
    {

        const int VERSION = 2;
        const string JOB_GROUP = "Cogito.MassTransit.Scheduler";
        const string JOB_NAME = "Periodic";
        const string TRIGGER_GROUP = "Cogito.MassTransit.Scheduler.Periodic";

        static readonly JobKey JOB_KEY = new JobKey(JOB_NAME, JOB_GROUP);

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PeriodicSchedulerJob()
        {

        }

        /// <summary>
        /// Periodically refreshes the configured pulse schedules.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            await InitializeSchedules(context);
        }

        /// <summary>
        /// Configures the various static schedules.
        /// </summary>
        /// <returns></returns>
        async Task InitializeSchedules(IJobExecutionContext context)
        {
            // find or create job
            var job = await context.Scheduler.GetJobDetail(JOB_KEY) ??
                JobBuilder.Create<PeriodicJob>()
                    .WithIdentity(JOB_KEY)
                    .UsingJobData("Version", VERSION.ToString())
                    .StoreDurably(true)
                    .Build();

            // ensure job is added
            if (await context.Scheduler.CheckExists(JOB_KEY) == false)
                await context.Scheduler.AddJob(job, false);

            // if the job version is unknown, simply ignore
            // downstream scheduler versions ignore
            if (int.Parse((string)job.JobDataMap["Version"] ?? "0") > VERSION)
                return;

            // replace existing job if upgrading
            if (int.Parse((string)job.JobDataMap["Version"] ?? "0") < VERSION)
                await context.Scheduler.AddJob(job, true);

            // schedule job only if triggers do not match
            var n = GetTriggers().OrderBy(i => i.Key).ToList();
            var o = (await context.Scheduler.GetTriggersOfJob(JOB_KEY)).OrderBy(i => i.Key).ToList();
            if (Enumerable.SequenceEqual(n, o, new TriggerEqualityComparer()) == false)
            {
                // reschedule job
                await context.Scheduler.UnscheduleJobs(o.Select(i => i.Key).ToList());
                await context.Scheduler.ScheduleJob(job, n, true);
            }
            else
            {
                // resume any errored triggers
                foreach (var t in o)
                    if (await context.Scheduler.GetTriggerState(t.Key) == TriggerState.Error)
                        await context.Scheduler.ResumeTrigger(t.Key);
            }
        }

        /// <summary>
        /// Gets the various pulse triggers to be scheduled.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ITrigger> GetTriggers()
        {
            yield return CreateTrigger("PT1M", TimeSpan.FromMinutes(1));
            yield return CreateTrigger("PT5M", TimeSpan.FromMinutes(5));
            yield return CreateTrigger("PT15M", TimeSpan.FromMinutes(15));
            yield return CreateTrigger("PT30M", TimeSpan.FromMinutes(30));
            yield return CreateTrigger("PT1H", TimeSpan.FromHours(1));
            yield return CreateTrigger("PT2H", TimeSpan.FromHours(2));
            yield return CreateTrigger("PT6H", TimeSpan.FromHours(6));
            yield return CreateTrigger("PT12H", TimeSpan.FromHours(12));
            yield return CreateTrigger("P1D", TimeSpan.FromDays(1));
        }

        /// <summary>
        /// Creates a new periodic trigger.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        ITrigger CreateTrigger(string key, TimeSpan interval)
        {
            var now = DateTime.Now;

            return TriggerBuilder.Create()
                .ForJob(JOB_KEY)
                .WithIdentity(new TriggerKey(key, TRIGGER_GROUP))
                .UsingJobData("Interval", interval.ToString())
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever().WithMisfireHandlingInstructionNextWithRemainingCount())
                .StartAt(new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero))
                .Build();
        }

    }

}
