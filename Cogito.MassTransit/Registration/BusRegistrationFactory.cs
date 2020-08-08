using System;
using System.Linq;

using MassTransit;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Creates bus instances from registered configuration.
    /// </summary>
    public class BusRegistrationFactory
    {

        readonly BusDefinitionProvider definitions;
        readonly BusConfigurationProvider configurations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="names"></param>
        /// <param name="definitions"></param>
        /// <param name="configurations"></param>
        public BusRegistrationFactory(BusDefinitionProvider definitions, BusConfigurationProvider configurations)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            this.configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
        }

        /// <summary>
        /// Gets the bus instance by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IBusControl CreateBus(string name)
        {
            return definitions.GetDefinitions().Where(i => i.BusName == name).Select(i => i.Factory(b => Configure(name, b))).FirstOrDefault();
        }

        /// <summary>
        /// Gets the default bus instance.
        /// </summary>
        /// <returns></returns>
        public IBusControl CreateBus()
        {
            return CreateBus("");
        }

        /// <summary>
        /// Configures the bus instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configurator"></param>
        void Configure(string name, IBusFactoryConfigurator configurator)
        {
            foreach (var p in configurations.GetConfigurations(name))
                p.Apply(name, configurator);
        }

    }

}
