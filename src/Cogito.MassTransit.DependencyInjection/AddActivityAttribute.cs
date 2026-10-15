using System;

using MassTransit;
using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Registers the routing slip activity, which executes and compensates on two endpoints, with MassTransit.
    /// Endpoint names that are left unset fall to the activity's definition, or to the bus's endpoint name
    /// formatter; configure anything more about the endpoints with a definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AddActivityAttribute : MassTransitRegistrationAttribute
    {

        /// <summary>
        /// A definition for the activity, which closes <c>IActivityDefinition&lt;TActivity, TArguments, TLog&gt;</c>.
        /// </summary>
        public Type? Definition { get; set; }

        /// <summary>
        /// The name of the endpoint the activity executes on.
        /// </summary>
        public string? ExecuteEndpointName { get; set; }

        /// <summary>
        /// The name of the endpoint the activity compensates on.
        /// </summary>
        public string? CompensateEndpointName { get; set; }

        /// <summary>
        /// Leaves the activity off the endpoints created by <c>ConfigureEndpoints</c>, so it can be configured onto
        /// endpoints by hand. Endpoint names cannot be set with this.
        /// </summary>
        public bool ExcludeFromConfigureEndpoints { get; set; }

        /// <inheritdoc />
        protected override void Add(IServiceCollection services, IContainerRegistrar registrar, Type implementationType)
        {
            var registration = services.RegisterActivity(registrar, implementationType, Definition);

            if (ExcludeFromConfigureEndpoints)
            {
                if (ExecuteEndpointName != null || CompensateEndpointName != null)
                    throw new InvalidOperationException($"{implementationType} is excluded from ConfigureEndpoints, so its endpoints cannot be configured.");

                registration.IncludeInConfigureEndpoints = false;
                return;
            }

            if (ExecuteEndpointName != null)
            {
                var argumentsType = ActivityTypes.GetArgumentsType(implementationType);
                EndpointRegistrar.RegisterEndpoint(services, registrar, registration,
                    typeof(ExecuteActivityEndpointDefinition<,>).MakeGenericType(implementationType, argumentsType),
                    typeof(IExecuteActivity<>).MakeGenericType(argumentsType),
                    new EndpointOptions() { Name = ExecuteEndpointName, ConfigureConsumeTopology = false });
            }

            if (CompensateEndpointName != null)
            {
                var logType = ActivityTypes.GetLogType(implementationType);
                EndpointRegistrar.RegisterEndpoint(services, registrar, registration,
                    typeof(CompensateActivityEndpointDefinition<,>).MakeGenericType(implementationType, logType),
                    typeof(ICompensateActivity<>).MakeGenericType(logType),
                    new EndpointOptions() { Name = CompensateEndpointName, ConfigureConsumeTopology = false });
            }
        }

    }

}
