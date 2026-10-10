using System;
using System.Threading.Tasks;

using Quartz;

namespace Cogito.MassTransit.Scheduler.QuartzIntegration
{

    /// <summary>
    /// Publishes the periodic message for the interval of the trigger that fired.
    /// </summary>
    public class PeriodicJob : IJob
    {

        /// <summary>
        /// The job data key holding the interval of a trigger, as a <see cref="TimeSpan"/> string.
        /// </summary>
        public const string IntervalKey = "Interval";

        readonly IPeriodicMessagePublisher publisher;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="publisher"></param>
        public PeriodicJob(IPeriodicMessagePublisher publisher)
        {
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        /// <inheritdoc/>
        public Task Execute(IJobExecutionContext context)
        {
            var value = (string)context.MergedJobDataMap[IntervalKey];
            var interval = PeriodicInterval.FromInterval(TimeSpan.Parse(value)) ??
                throw new InvalidOperationException($"No periodic interval is {value} long.");

            var message = interval.CreateMessage(
                context.FireTimeUtc,
                context.ScheduledFireTimeUtc,
                context.PreviousFireTimeUtc,
                context.NextFireTimeUtc);

            return publisher.Publish(message, context.CancellationToken);
        }

    }

}
