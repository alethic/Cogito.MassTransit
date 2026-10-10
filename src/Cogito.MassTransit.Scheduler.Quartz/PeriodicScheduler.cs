using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;

namespace Cogito.MassTransit.Scheduler.QuartzIntegration
{

    /// <summary>
    /// Schedules a set of periodic Pulse messages on the bus.
    /// </summary>
    public class PeriodicScheduler : IHostedService
    {

        const string JOB_GROUP = "Cogito.MassTransit.Scheduler";
        const string JOB_NAME = "PeriodicScheduler";
        const string TRIGGER_GROUP = "Cogito.MassTransit.Scheduler";
        const string TRIGGER_NAME = "PeriodicScheduler";

        static readonly JobKey JOB_KEY = new JobKey(JOB_NAME, JOB_GROUP);
        static readonly TriggerKey TRIGGER_KEY = new TriggerKey(TRIGGER_NAME, TRIGGER_GROUP);

        readonly ISchedulerFactory _schedulerFactory;
        readonly ILogger _logger;

        Task? _runTask;
        CancellationTokenSource? _runCancel;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="logger"></param>
        public PeriodicScheduler(ISchedulerFactory schedulerFactory, ILogger<PeriodicScheduler> logger)
        {
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_runTask == null)
            {
                _runCancel = new CancellationTokenSource();
                _runTask = Task.Run(() => RunAsync(_runCancel.Token));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // stopping is idempotent: a host may stop the service more than once
            if (_runCancel != null && _runTask != null)
            {
                _runCancel.Cancel();
                await _runTask;
                _runCancel.Dispose();
                _runCancel = null;
                _runTask = null;
            }
        }

        /// <summary>
        /// Periodically refreshes the configured pulse schedules.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task RunAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    await InitializeJobSchedulerJob(cancellationToken);
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unhandled exception occurred.");
                }
            }
        }

        /// <summary>
        /// Configures the various static schedules.
        /// </summary>
        /// <returns></returns>
        async Task InitializeJobSchedulerJob(CancellationToken cancellationToken)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            // add the job, or replace a stored job of another type, such as one stored by an earlier version of
            // this package, in which the job type lived in the Cogito.MassTransit.Scheduler assembly
            var existing = await QuartzJobs.TryGetJobDetail(scheduler, JOB_KEY, cancellationToken);
            if (existing == null || existing.JobType != typeof(PeriodicSchedulerJob))
                await scheduler.AddJob(
                    JobBuilder.Create<PeriodicSchedulerJob>()
                        .WithIdentity(JOB_KEY)
                        .StoreDurably(true)
                        .Build(),
                    true,
                    cancellationToken);

            // all currently registered triggers
            var triggers = await scheduler.GetTriggersOfJob(JOB_KEY);

            // remove any unnecessary triggers
            foreach (var t in triggers.Where(i => i.Key != TRIGGER_KEY))
                await scheduler.UnscheduleJob(t.Key);

            // remove any additional triggers
            foreach (var t in triggers.Skip(1))
                await scheduler.UnscheduleJob(t.Key);

            // find existing matching trigger
            var trigger = triggers.FirstOrDefault();
            if (trigger == null)
            {
                trigger = TriggerBuilder.Create()
                    .ForJob(JOB_KEY)
                    .WithIdentity(TRIGGER_KEY)
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(5)).RepeatForever().WithMisfireHandlingInstructionIgnoreMisfires())
                    .StartNow()
                    .Build();

                // schedule new trigger
                await scheduler.ScheduleJob(trigger);
            }
        }

    }

}
