using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.Scheduler
{

    /// <summary>
    /// Configures the periodic job scheduler. A scheduler package, such as Cogito.MassTransit.Scheduler.Quartz or
    /// Cogito.MassTransit.Scheduler.Hangfire, adds the method that selects it.
    /// </summary>
    public interface IPeriodicJobSchedulerConfigurator
    {

        /// <summary>
        /// Gets the services being configured.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Records the scheduler that publishes the periodic messages. Only one scheduler may be used.
        /// </summary>
        /// <param name="name"></param>
        void UseScheduler(string name);

    }

}
