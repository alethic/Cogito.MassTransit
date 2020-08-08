using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides registered <see cref="IBusConfiguration"/> instances.
    /// </summary>
    public class ContainerBusConfigurationSource : IBusConfigurationSource
    {

        readonly IEnumerable<IBusConfiguration> configuration;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="configuration"></param>
        public ContainerBusConfigurationSource(IOrderedEnumerable<IBusConfiguration> configuration)
        {
            this.configuration = configuration?.ToList() ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Finds all <see cref="IBusFactoryDefinition"/> registered with the container.
        /// </summary>
        /// <param name="busName"></param>
        /// <returns></returns>
        public IEnumerable<IBusConfiguration> Query(string busName) => configuration;

    }

}
