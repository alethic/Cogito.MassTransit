using System;
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var h = await client.GetResponse<TestMessage>(new TestMessage());
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

}
