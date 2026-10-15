using System;

using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Registers the saga with MassTransit. The saga needs a repository: apply a repository attribute, such as
    /// <see cref="InMemorySagaRepositoryAttribute"/>, or set a saga repository provider on the bus and add the
    /// attributes before <c>AddMassTransit</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AddSagaAttribute : EndpointRegistrationAttribute
    {

        /// <summary>
        /// A saga definition for the saga, which closes <c>ISagaDefinition&lt;TSaga&gt;</c>.
        /// </summary>
        public Type? Definition { get; set; }

        /// <inheritdoc />
        protected override void Add(IServiceCollection services, IContainerRegistrar registrar, Type implementationType)
        {
            var registration = services.RegisterSaga(registrar, implementationType, Definition);
            ApplyEndpoint(services, registrar, registration, typeof(SagaEndpointDefinition<>).MakeGenericType(registration.Type), registration.Type, implementationType);
            SagaRepositoryValidator.Require(services, registration.Type);
        }

    }

}
