using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Hangfire;
using Hangfire.Server;

namespace Cogito.MassTransit.Scheduler.HangfireIntegration
{

    /// <summary>
    /// Publishes the periodic message for an interval, when its recurring job fires.
    /// </summary>
    public class PeriodicJob
    {

        readonly IPeriodicMessagePublisher publisher;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="publisher"></param>
        public PeriodicJob(IPeriodicMessagePublisher publisher)
        {
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        /// <summary>
        /// Publishes the periodic message for the named interval. A failed message is not retried: by the time a retry
        /// ran, the message would be stale, and the next is already on its way.
        /// </summary>
        /// <param name="interval">The name of the interval, such as <c>PT5M</c>.</param>
        /// <param name="context">Supplied by Hangfire.</param>
        /// <param name="cancellationToken">Supplied by Hangfire.</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0)]
        [DisplayName("Publish periodic message {0}")]
        public Task Execute(string interval, PerformContext? context, CancellationToken cancellationToken)
        {
            var periodic = PeriodicInterval.FromName(interval) ??
                throw new InvalidOperationException($"No periodic interval is named {interval}.");

            // Hangfire does not report the time a recurring job was due, but creates its background job at or shortly
            // after that time; as every interval is aligned to midnight UTC, the time it was due is the start of the
            // interval in which the background job was created
            var now = DateTimeOffset.UtcNow;
            var created = context?.BackgroundJob?.CreatedAt is DateTime c ? new DateTimeOffset(DateTime.SpecifyKind(c, DateTimeKind.Utc)) : now;
            var scheduled = periodic.GetScheduledFireTime(created);

            var message = periodic.CreateMessage(
                now,
                scheduled,
                scheduled - periodic.Interval,
                scheduled + periodic.Interval);

            return publisher.Publish(message, cancellationToken);
        }

    }

}
