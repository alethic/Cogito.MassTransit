using System;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Provides a <see cref="IHostedService"/> implementation for a MassTransit bus.
    /// </summary>
    public class BusHostedService : IHostedService
    {

        readonly IBusControl bus;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        public BusHostedService(IBusControl bus)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return bus.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return bus.StopAsync(cancellationToken);
        }

    }

}
