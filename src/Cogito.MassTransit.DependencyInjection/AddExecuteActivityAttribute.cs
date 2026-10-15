using System;

using MassTransit;
using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Registers the execute-only routing slip activity with MassTransit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AddExecuteActivityAttribute : EndpointRegistrationAttribute
    {

        /// <summary>
        /// A definition for the activity, which closes <c>IExecuteActivityDefinition&lt;TActivity, TArguments&gt;</c>.
        /// </summary>
        public Type? Definition { get; set; }

        /// <inheritdoc />
        protected override bool DefaultConfigureConsumeTopology => false;

        /// <inheritdoc />
        protected override void Add(IServiceCollection services, IContainerRegistrar registrar, Type implementationType)
        {
            var registration = services.RegisterExecuteActivity(registrar, implementationType, Definition);
            var argumentsType = ActivityTypes.GetArgumentsType(implementationType);
            ApplyEndpoint(services, registrar, registration, typeof(ExecuteActivityEndpointDefinition<,>).MakeGenericType(implementationType, argumentsType), typeof(IExecuteActivity<>).MakeGenericType(argumentsType), implementationType);
        }

    }

}
