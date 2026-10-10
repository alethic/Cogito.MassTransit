using System;
using System.Threading;
using System.Threading.Tasks;

using Cogito.MassTransit.Scheduling.Periodic;

using MassTransit;

namespace Cogito.MassTransit.Scheduler
{

    /// <summary>
    /// Publishes periodic messages to the bus.
    /// </summary>
    public class PeriodicMessagePublisher : IPeriodicMessagePublisher
    {

        readonly IBus bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        public PeriodicMessagePublisher(IBus bus)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public Task Publish(P message, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return bus.Publish(message, message.GetType(), context =>
            {
                // message is transient and should expire at interval length
                context.Durable = false;
                context.TimeToLive = message.Interval;
            }, cancellationToken);
        }

    }

}
