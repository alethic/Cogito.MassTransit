using System.Threading.Tasks;

using Cogito.DependencyInjection;
using Cogito.MassTransit.DependencyInjecton.Sample1;

using MassTransit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cogito.MassTransit.Autofac.Sample1
{

    public static class Program
    {

        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices(s =>
                {
                    s.AddMassTransit(c =>
                    {
                        c.UsingInMemory((ctx, cfg) =>
                        {
                            cfg.ConfigureEndpoints(ctx);
                        });
                    });
                    s.AddMassTransit<IBus1>(c =>
                    {
                        c.UsingInMemory((ctx, cfg) =>
                        {
                            cfg.ConfigureEndpoints(ctx);
                        });
                    });
                    s.AddMassTransit<IBus2>(c =>
                    {
                        c.UsingInMemory((ctx, cfg) =>
                        {
                            cfg.ConfigureEndpoints(ctx);
                        });
                    });
                    s.AddFromAttributes(typeof(Program).Assembly);
                })
                .ConfigureLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .Build()
                .RunAsync();
        }

    }

}
