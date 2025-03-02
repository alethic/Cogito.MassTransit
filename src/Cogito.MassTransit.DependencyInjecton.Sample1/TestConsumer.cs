using System.Threading.Tasks;

using Cogito.MassTransit.DependencyInjection;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Cogito.MassTransit.Autofac.Sample1
{

    [AddConsumer]
    public class TestConsumer : IConsumer<TestMessage>
    {

        readonly ILogger logger;

        public TestConsumer(ILogger<TestConsumer> logger)
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
