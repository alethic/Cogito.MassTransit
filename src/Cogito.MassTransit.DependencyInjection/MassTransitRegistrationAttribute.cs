using System;

using Cogito.DependencyInjection;

using MassTransit;
using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Base for attributes that register a type with MassTransit when the assembly is added with
    /// <c>services.AddFromAttributes(assembly)</c>.
    /// </summary>
    public abstract class MassTransitRegistrationAttribute : Attribute, IServiceRegistrationAttribute
    {

        Type? bus;

        /// <summary>
        /// The bus to register onto: <see cref="IBus"/>, the default, or the interface given to
        /// <c>AddMassTransit&lt;TBus&gt;</c>.
        /// </summary>
        public Type? Bus
        {
            get => bus;
            set
            {
                if (value != null && (value.IsInterface == false || typeof(IBus).IsAssignableFrom(value) == false))
                    throw new ArgumentException($"The bus must be IBus or an interface that extends it, but was {value}.", nameof(value));

                bus = value;
            }
        }

        /// <inheritdoc />
        public virtual int RegistrationOrder => 0;

        /// <inheritdoc />
        public void Add(IServiceCollection services, Type implementationType)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            Add(services, CreateRegistrar(services), implementationType);
        }

        /// <summary>
        /// Registers <paramref name="implementationType"/> through <paramref name="registrar"/>, which binds the
        /// registration to the selected bus.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="registrar"></param>
        /// <param name="implementationType"></param>
        protected abstract void Add(IServiceCollection services, IContainerRegistrar registrar, Type implementationType);

        /// <summary>
        /// Creates the registrar for the selected bus.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        IContainerRegistrar CreateRegistrar(IServiceCollection services)
        {
            if (bus == null || bus == typeof(IBus))
                return new DependencyInjectionContainerRegistrar(services);

            return (IContainerRegistrar)Activator.CreateInstance(typeof(DependencyInjectionContainerRegistrar<>).MakeGenericType(bus), services)!;
        }

    }

}
