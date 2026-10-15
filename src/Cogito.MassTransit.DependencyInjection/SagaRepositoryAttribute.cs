using System;
using System.Linq;

using Cogito.DependencyInjection;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Base for attributes that register the repository of a saga. Apply it to the saga, or to the saga state
    /// machine, whose instance is then the saga. It registers the repository directly, so it does not depend on
    /// whether the attributes are added before or after <c>AddMassTransit</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public abstract class SagaRepositoryAttribute : Attribute, IServiceRegistrationAttribute
    {

        /// <inheritdoc />
        public virtual int RegistrationOrder => 0;

        /// <inheritdoc />
        public void Add(IServiceCollection services, Type implementationType)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            AddSagaRepository(services, GetSagaType(implementationType));
        }

        /// <summary>
        /// Registers the repository of <paramref name="sagaType"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sagaType"></param>
        protected abstract void AddSagaRepository(IServiceCollection services, Type sagaType);

        /// <summary>
        /// Gets the saga of the attributed type: the instance of a saga state machine, or the saga itself.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static Type GetSagaType(Type type)
        {
            var stateMachine = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(SagaStateMachine<>));
            if (stateMachine != null)
                return stateMachine.GetGenericArguments()[0];

            if (typeof(ISaga).IsAssignableFrom(type))
                return type;

            throw new InvalidOperationException($"{type} is neither a saga nor a saga state machine, so it cannot have a saga repository.");
        }

    }

}
