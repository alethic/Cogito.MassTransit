using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cogito.MassTransit.Scheduler
{

    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds the periodic job scheduler, which publishes a periodic message on each of the
        /// <see cref="PeriodicInterval"/>s, using the scheduler selected by <paramref name="configure"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure">Selects the scheduler, for instance <c>c => c.UseQuartz()</c>.</param>
        /// <returns></returns>
        public static IServiceCollection AddPeriodicJobScheduler(this IServiceCollection services, Action<IPeriodicJobSchedulerConfigurator> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            services.TryAddSingleton<IPeriodicMessagePublisher, PeriodicMessagePublisher>();

            var configurator = new PeriodicJobSchedulerConfigurator(services);
            configure(configurator);

            if (configurator.Scheduler == null)
                throw new InvalidOperationException("No periodic job scheduler was selected. Select one, for instance AddPeriodicJobScheduler(c => c.UseQuartz()).");

            return services;
        }

        class PeriodicJobSchedulerConfigurator : IPeriodicJobSchedulerConfigurator
        {

            public PeriodicJobSchedulerConfigurator(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }

            public string? Scheduler { get; private set; }

            public void UseScheduler(string name)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("The scheduler must have a name.", nameof(name));
                if (Scheduler != null)
                    throw new InvalidOperationException($"The periodic job scheduler already uses {Scheduler}; only one scheduler may be used.");

                Scheduler = name;
            }

        }

    }

}
