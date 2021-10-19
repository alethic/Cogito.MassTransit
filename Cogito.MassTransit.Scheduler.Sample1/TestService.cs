using System.Threading;
using System.Threading.Tasks;

using Cogito.Autofac;

using MassTransit;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Scheduler.Sample1
{

    [RegisterAs(typeof(IHostedService))]
    public class TestService : IHostedService
    {

        public IRequestClient<TestMessage> client;

        public TestService(IRequestClient<TestMessage> client)
        {
            this.client = client;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
