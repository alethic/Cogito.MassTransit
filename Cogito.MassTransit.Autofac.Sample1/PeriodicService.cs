using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Cogito.Autofac;
using Cogito.Components;

using MassTransit;

namespace Cogito.MassTransit.Autofac.Sample1
{

    [RegisterAs(typeof(IRunnable))]
    public class PeriodicService : IRunnable
    {

        readonly IBus bus;

        public PeriodicService(IBus bus)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
                await bus.Publish(new TestSagaMessage() { Id = NewId.NextGuid(), Value = DateTime.Now.ToString() });
            }
        }

    }

}
