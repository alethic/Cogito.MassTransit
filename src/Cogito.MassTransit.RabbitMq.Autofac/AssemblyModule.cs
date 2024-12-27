using Autofac;

using Cogito.Autofac;
using Cogito.MassTransit.RabbitMq.Registration;
using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.RabbitMq.Autofac
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.RegisterType<RabbitMqBusConfiguration>().As<IBusConfiguration>().SingleInstance();
        }

    }

}
