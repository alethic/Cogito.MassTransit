using System;

using Cogito.MassTransit.Registration;

using MassTransit;

using Microsoft.Extensions.Options;

namespace Cogito.MassTransit.InMemory.Registration
{

    /// <summary>
    /// Applies named configuration to a InMemory bus configurator.
    /// </summary>
    public class InMemoryBusConfiguration : IBusConfiguration
    {

        readonly IOptionsSnapshot<InMemoryBusOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="options"></param>
        public InMemoryBusConfiguration(IOptionsSnapshot<InMemoryBusOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Applies the configuration for the given bus to the configurator.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="configurator"></param>
        public void Apply(string busName, IBusFactoryConfigurator configurator)
        {
            var c = configurator as IInMemoryBusFactoryConfigurator;
            if (c == null)
                return;

            var o = options.Get(busName);
            if (o == null)
                return;

            // apply options
            if (o.BaseAddress != null)
                c.Host(o.BaseAddress);
            else
                c.Host();
        }
    }

}
