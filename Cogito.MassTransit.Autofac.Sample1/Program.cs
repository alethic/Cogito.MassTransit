using System.Threading.Tasks;

using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac;
using Cogito.MassTransit.InMemory.Autofac;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Autofac.Sample1
{

    public static class Program
    {

        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(b => b
                    .RegisterMassTransitBus("", b => b.UsingInMemoryBus().WithHostedService())
                    .RegisterMassTransitBus("bus1", b => b.UsingInMemoryBus().WithHostedService())
                    .RegisterAllAssemblyModules()))
                .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .Build()
                .RunAsync();
        }

    }

}
