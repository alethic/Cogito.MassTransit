using System;
using System.Threading;
using System.Threading.Tasks;

using Cogito.DependencyInjection;

using MassTransit;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Autofac.Sample1
{

    [AddSingletonService<IHostedService>()]
    public class PeriodicService : BackgroundService
    {

        readonly IBus bus;

        public PeriodicService(IBus bus)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                await bus.Publish(new TestMessage());
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

    }

}
