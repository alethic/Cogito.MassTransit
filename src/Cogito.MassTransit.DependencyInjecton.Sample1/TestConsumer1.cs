using System.Threading.Tasks;

using Cogito.MassTransit.DependencyInjection;
using Cogito.MassTransit.DependencyInjecton.Sample1;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Cogito.MassTransit.Autofac.Sample1
{

    [AddConsumer<IBus1>(EndpointName = "test_endpoint")]
    public class TestConsumer1 : IConsumer<TestMessage>
    {

        readonly ILogger logger;

        public TestConsumer1(ILogger<TestConsumer1> logger)
        {
            this.logger = logger;
        }

        public Task Consume(ConsumeContext<TestMessage> context)
        {
            logger.LogInformation("Received");
            return Task.CompletedTask;
        }

    }

}
