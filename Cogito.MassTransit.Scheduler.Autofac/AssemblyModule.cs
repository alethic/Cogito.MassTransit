
using Autofac;

using Cogito.Autofac;
using Cogito.MassTransit.Autofac;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Scheduler.Autofac
{

    /// <summary>
    /// Describes types required for this service.
    /// </summary>
    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<Cogito.Quartz.Autofac.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.RegisterType<PeriodicScheduler>().As<IHostedService>();
            builder.RegisterType<PeriodicSchedulerJob>().AsSelf();
            builder.RegisterType<PeriodicJob>().AsSelf();
            builder.RegisterType<ScheduledMessageJob>().AsSelf();
            builder.RegisterConsumer<ScheduledMessageConsumer>("scheduler");
            builder.RegisterConsumer<global::MassTransit.QuartzIntegration.ScheduleMessageConsumer>("scheduler");
            builder.RegisterConsumer<global::MassTransit.QuartzIntegration.CancelScheduledMessageConsumer>("scheduler");
        }
    }

}
