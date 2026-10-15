using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MassTransit.Saga;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Fails startup when a saga registered from attributes has no repository, rather than leaving it to fail on
    /// its first message.
    /// </summary>
    class SagaRepositoryValidator : IHostedService
    {

        /// <summary>
        /// Requires a repository for <paramref name="sagaType"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sagaType"></param>
        public static void Require(IServiceCollection services, Type sagaType)
        {
            services.AddSingleton(new SagaRepositoryRequirement(sagaType));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, SagaRepositoryValidator>());
        }

        readonly IServiceProvider provider;
        readonly IEnumerable<SagaRepositoryRequirement> requirements;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="requirements"></param>
        public SagaRepositoryValidator(IServiceProvider provider, IEnumerable<SagaRepositoryRequirement> requirements)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // without this the container cannot be asked, and the saga fails on its first message instead
            if (provider.GetService<IServiceProviderIsService>() is not { } isService)
                return Task.CompletedTask;

            var missing = requirements
                .Select(i => i.SagaType)
                .Distinct()
                .Where(i => isService.IsService(typeof(ISagaRepositoryContextFactory<>).MakeGenericType(i)) == false)
                .ToList();

            if (missing.Count > 0)
                throw new InvalidOperationException(
                    $"No saga repository is registered for {string.Join(", ", missing.Select(i => i.FullName))}. " +
                    "Apply a repository attribute, such as [InMemorySagaRepository], to the saga or its state machine, " +
                    "or set a saga repository provider on the bus and call AddFromAttributes before AddMassTransit.");

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }

    /// <summary>
    /// Records that a saga registered from attributes needs a repository.
    /// </summary>
    class SagaRepositoryRequirement
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sagaType"></param>
        public SagaRepositoryRequirement(Type sagaType)
        {
            SagaType = sagaType ?? throw new ArgumentNullException(nameof(sagaType));
        }

        /// <summary>
        /// The saga.
        /// </summary>
        public Type SagaType { get; }

    }

}
