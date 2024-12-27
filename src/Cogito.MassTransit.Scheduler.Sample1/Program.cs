using System.Threading.Tasks;

using Cogito.DependencyInjection;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;

namespace Cogito.MassTransit.Scheduler.Sample1
{

    public static class Program
    {

        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices(s =>
                {
                    s.AddQuartz(c => c.UseInMemoryStore());
                    s.AddQuartzHostedService();

                    s.AddPeriodicJobScheduler();
                    s.AddMassTransit(c =>
                    {
                        c.AddPublishMessageScheduler();
                        c.AddQuartzConsumers();

                        c.UsingInMemory((ctx, cfg) =>
                        {
                            cfg.UsePublishMessageScheduler();
                            cfg.ConfigureEndpoints(ctx);
                        });
                    });

                    s.AddFromAttributes(typeof(Program).Assembly);
                })
                .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .ConfigureLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .Build()
                .RunAsync();
        }

    }

}
