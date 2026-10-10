using System;

using Cogito.MassTransit.Scheduler.HangfireIntegration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cogito.MassTransit.Scheduler
{

    public static class HangfirePeriodicJobSchedulerConfiguratorExtensions
    {

        /// <summary>
        /// Keeps the periodic schedule as Hangfire recurring jobs. Hangfire itself is configured separately, through
        /// <c>AddHangfire</c>, which selects its storage, and a Hangfire server must run on at least one node to fire
        /// the jobs.
        /// </summary>
        /// <param name="configurator"></param>
        /// <returns></returns>
        public static IPeriodicJobSchedulerConfigurator UseHangfire(this IPeriodicJobSchedulerConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.UseScheduler("Hangfire");
            configurator.Services.AddHostedService<PeriodicJobRegistrar>();
            configurator.Services.TryAddTransient<PeriodicJob>();
            return configurator;
        }

    }

}
