using System;
using System.Threading.Tasks;

using Cogito.Autofac;
using Cogito.Autofac.DependencyInjection;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Autofac.Sample1
{

    public static class Program
    {

        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(b => b
                    .Populate(Populate3)
                    .RegisterAllAssemblyModules()))
                .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .Build()
                .RunAsync();
        }

        static void Populate3(IServiceCollection collection)
        {
            collection.AddMassTransit(c =>
            {
                c.UsingInMemory((ctx, cfg) => { });
                c.AddConsumer<TestConsumer>();
            });
        }

    }

}
