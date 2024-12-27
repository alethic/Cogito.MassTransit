using Autofac;

using Cogito.Autofac;
using Cogito.MassTransit.Azure.ServiceBus.Registration;
using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.Azure.ServiceBus.Autofac
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.RegisterType<ServiceBusBusConfiguration>().As<IBusConfiguration>().SingleInstance();
        }

    }

}
