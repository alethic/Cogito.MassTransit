using System;

using Autofac;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides a builder to build a named bus instance.
    /// </summary>
    public class BusRegistrationBuilder
    {


        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        public BusRegistrationBuilder(ContainerBuilder builder, string name)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the underlying <see cref="ContainerBuilder"/>.
        /// </summary>
        public ContainerBuilder Builder { get; }

        /// <summary>
        /// Gets the name of the bus.
        /// </summary>
        public string Name { get; }

    }

}