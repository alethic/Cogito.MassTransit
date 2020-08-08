using System;
using System.Collections.Generic;
using System.Linq;

using MassTransit;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Configures the registered <see cref="IBusObserver"/>s.
    /// </summary>
    public class ContainerBusObserverConfiguration : IBusConfiguration
    {

        readonly IEnumerable<IBusObserver> observers;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="observers"></param>
        public ContainerBusObserverConfiguration(IEnumerable<IBusObserver> observers)
        {
            this.observers = observers?.ToList() ?? throw new ArgumentNullException(nameof(observers));
        }

        /// <summary>
        /// Applies the configuration to the bus.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="configurator"></param>
        public void Apply(string busName, IBusFactoryConfigurator configurator)
        {
            foreach (var observer in observers)
                if (observer != null)
                    configurator.ConnectBusObserver(observer);
        }

    }

}
