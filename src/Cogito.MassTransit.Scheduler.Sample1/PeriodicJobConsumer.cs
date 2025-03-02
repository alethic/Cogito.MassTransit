using System.Threading.Tasks;

using Cogito.MassTransit.Scheduling.Periodic;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Cogito.MassTransit.Scheduler.Sample1
{

    public class PeriodicJobConsumer : IConsumer<PT1M>
    {

        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger"></param>
        public PeriodicJobConsumer(ILogger<PeriodicJobConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PT1M> context)
        {
            _logger.LogInformation("Received PT1M");
            return Task.CompletedTask;
        }

    }

}
