using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Quartz;

namespace Cogito.MassTransit.Scheduler.QuartzIntegration
{

    /// <summary>
    /// Periodically sets up the Periodic jobs.
    /// </summary>
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
            var job = JobBuilder.Create<PeriodicJob>()
                .WithIdentity(JOB_KEY)
                .UsingJobData("Version", VERSION.ToString())
                .StoreDurably(true)
                .Build();

            var existing = await QuartzJobs.TryGetJobDetail(context.Scheduler, JOB_KEY, context.CancellationToken);

            // a newer version of the scheduler owns the job, so leave it alone
            if (existing != null && GetVersion(existing) > VERSION)
                return;

            // add the job, or replace one of an earlier version or of another type, such as one stored by an earlier
            // version of this package, in which the job type lived in the Cogito.MassTransit.Scheduler assembly
            if (existing == null || existing.JobType != typeof(PeriodicJob) || GetVersion(existing) < VERSION)
                await context.Scheduler.AddJob(job, true, context.CancellationToken);

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
        static int GetVersion(IJobDetail job)
        {
            return job.JobDataMap.TryGetValue("Version", out var value) && int.TryParse(value as string, out var version) ? version : 0;
        }

        IEnumerable<ITrigger> GetTriggers()
        {
            return PeriodicInterval.All.Select(i => CreateTrigger(i.Name, i.Interval));
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
                .UsingJobData(PeriodicJob.IntervalKey, interval.ToString())
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever().WithMisfireHandlingInstructionNextWithRemainingCount())
                .StartAt(new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero))
                .Build();
        }

    }

}
