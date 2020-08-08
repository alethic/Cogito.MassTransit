using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Cogito.MassTransit.Autofac.Sample1
{

    [RegisterConsumer("bus2", "test_endpoint")]
    public class TestConsumer2 : IConsumer<TestMessage>
    {

        readonly ILogger logger;

        public TestConsumer2(ILogger logger)
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
