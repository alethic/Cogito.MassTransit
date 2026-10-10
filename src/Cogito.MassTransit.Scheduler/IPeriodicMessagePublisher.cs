using System.Threading;
using System.Threading.Tasks;

using Cogito.MassTransit.Scheduling.Periodic;

namespace Cogito.MassTransit.Scheduler
{

    /// <summary>
    /// Publishes periodic messages to the bus on behalf of a scheduler.
    /// </summary>
    public interface IPeriodicMessagePublisher
    {

        /// <summary>
        /// Publishes the periodic message. The message is transient, and expires after its interval, by when the
        /// next one is due.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Publish(P message, CancellationToken cancellationToken = default);

    }

}
