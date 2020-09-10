using Autofac;

using Cogito.Autofac;
using Cogito.MassTransit.InMemory.Registration;
using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.InMemory.Autofac
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.RegisterType<InMemoryBusConfiguration>().As<IBusConfiguration>().SingleInstance();
        }

    }

}
