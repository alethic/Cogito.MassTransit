using System;

using Cogito.MassTransit.Registration;

using MassTransit;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Snippet of bus configuration.
    /// </summary>
    class DelegateBusConfiguration : IBusConfiguration
    {

        readonly string busName;
        readonly Action<IBusFactoryConfigurator> action;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="action"></param>
        public DelegateBusConfiguration(string busName, Action<IBusFactoryConfigurator> action)
        {
            this.busName = busName ?? throw new ArgumentNullException(nameof(busName));
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(string busName, IBusFactoryConfigurator configurator)
        {
            if (busName == this.busName)
                action?.Invoke(configurator);
        }

    }

}
