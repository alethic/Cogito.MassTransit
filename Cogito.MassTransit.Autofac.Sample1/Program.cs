using System.Threading.Tasks;

using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac;
using Cogito.MassTransit.Azure.ServiceBus.Autofac;

using MassTransit;

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
                    .RegisterMassTransitBus("bus1", b => b
                        .UsingAzureServiceBus(o => o.ConnectionString = ""))
                    .RegisterAllAssemblyModules()))
                .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .Build()
                .RunAsync();
        }

    }

}
