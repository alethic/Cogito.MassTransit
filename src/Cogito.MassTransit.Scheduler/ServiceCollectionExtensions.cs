using System.Diagnostics.CodeAnalysis;

using Cogito.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.Scheduler
{

    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds the periodic scheduler to the services collection.
        /// </summary>
        /// <param name="services"></param>
#if NET
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PeriodicJob))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PeriodicScheduler))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PeriodicSchedulerJob))]
#endif
        public static IServiceCollection AddPeriodicJobScheduler(this IServiceCollection services)
        {
            services.AddFromAttributes(typeof(ServiceCollectionExtensions).Assembly);
            return services;
        }

    }

}
