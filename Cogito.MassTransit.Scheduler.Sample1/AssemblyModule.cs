using Autofac;

using Cogito.Autofac;
using Cogito.Extensions.Options.Autofac;
using Cogito.MassTransit.RabbitMq.Registration;
using Cogito.Quartz.Options;

namespace Cogito.MassTransit.Scheduler.Sample1
{

    /// <summary>
    /// Describes types required for this service.
    /// </summary>
    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Configure<RabbitMqBusOptions>(o => { o.Password = "accutraq"; o.UserName = "accutraq"; });
            builder.Configure<QuartzOptions>(o => { });
        }

    }

}
