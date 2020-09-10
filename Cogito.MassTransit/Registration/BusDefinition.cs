using System;

using MassTransit;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Defines a registered bus.
    /// </summary>
    public class BusDefinition
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="factory"></param>
        public BusDefinition(string busName, Func<Action<IBusFactoryConfigurator>, IBusControl> factory)
        {
            BusName = busName;
            Factory = factory;
        }

        /// <summary>
        /// Gets the name of the bus.
        /// </summary>
        public string BusName { get; }

        /// <summary>
        /// The configurator method of the bus type.
        /// </summary>
        public Func<Action<IBusFactoryConfigurator>, IBusControl> Factory { get; }

    }

}
