using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known <see cref="IBusConfiguration"/> instances.
    /// </summary>
    public class BusConfigurationProvider
    {

        readonly IEnumerable<IBusConfigurationSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public BusConfigurationProvider(IEnumerable<IBusConfigurationSource> sources)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known <see cref="IBusConfiguration"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBusConfiguration> GetConfigurations(string busName) => sources.SelectMany(i => i.Query(busName));

    }

}
