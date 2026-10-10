using System;

using Cogito.MassTransit.Scheduler.QuartzIntegration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cogito.MassTransit.Scheduler
{

    public static class QuartzPeriodicJobSchedulerConfiguratorExtensions
    {

        /// <summary>
        /// Keeps the periodic schedule in Quartz. Quartz itself is configured separately, through <c>AddQuartz</c>,
        /// which also selects its job store; a clustered persistent store publishes each message once across nodes.
        /// </summary>
        /// <param name="configurator"></param>
        /// <returns></returns>
        public static IPeriodicJobSchedulerConfigurator UseQuartz(this IPeriodicJobSchedulerConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.UseScheduler("Quartz");
            configurator.Services.AddHostedService<PeriodicScheduler>();
            configurator.Services.TryAddScoped<PeriodicSchedulerJob>();
            configurator.Services.TryAddScoped<PeriodicJob>();
            return configurator;
        }

    }

}
