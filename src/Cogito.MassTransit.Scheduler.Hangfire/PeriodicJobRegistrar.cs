using System;
using System.Threading;
using System.Threading.Tasks;

using Hangfire;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Scheduler.HangfireIntegration
{

    /// <summary>
    /// Adds or updates a Hangfire recurring job for each periodic interval when the host starts. Recurring jobs are
    /// identified by ID, so every node may do this: each job is stored once, and fires once across the nodes.
    /// </summary>
    public class PeriodicJobRegistrar : IHostedService
    {

        /// <summary>
        /// Gets the ID of the recurring job for the interval.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static string GetRecurringJobId(PeriodicInterval interval)
        {
            if (interval == null)
                throw new ArgumentNullException(nameof(interval));

            return $"Cogito.MassTransit.Scheduler.Periodic.{interval.Name}";
        }

        /// <summary>
        /// Gets the cron expression on which the interval fires, in UTC.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static string GetCronExpression(PeriodicInterval interval)
        {
            if (interval == null)
                throw new ArgumentNullException(nameof(interval));

            var length = interval.Interval;

            if (length < TimeSpan.FromHours(1))
                return length == TimeSpan.FromMinutes(1) ? "* * * * *" : $"*/{(int)length.TotalMinutes} * * * *";

            if (length < TimeSpan.FromDays(1))
                return length == TimeSpan.FromHours(1) ? "0 * * * *" : $"0 */{(int)length.TotalHours} * * *";

            return "0 0 * * *";
        }

        readonly IRecurringJobManager manager;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="manager"></param>
        public PeriodicJobRegistrar(IRecurringJobManager manager)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var interval in PeriodicInterval.All)
            {
                var name = interval.Name;

                manager.AddOrUpdate<PeriodicJob>(
                    GetRecurringJobId(interval),
                    job => job.Execute(name, null, CancellationToken.None),
                    GetCronExpression(interval),
                    new RecurringJobOptions() { TimeZone = TimeZoneInfo.Utc });
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }

}
