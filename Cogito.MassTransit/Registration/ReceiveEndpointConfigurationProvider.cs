﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known <see cref="IReceiveEndpointConfiguration"/> instances.
    /// </summary>
    public class ReceiveEndpointConfigurationProvider
    {

        readonly IEnumerable<IReceiveEndpointConfigurationSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public ReceiveEndpointConfigurationProvider(IOrderedEnumerable<IReceiveEndpointConfigurationSource> sources)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known <see cref="IReceiveEndpointConfiguration"/> instances for the given endpoint name.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IReceiveEndpointConfiguration> GetConfigurations(string busName, string endpointName) => sources.SelectMany(i => i.GetConfiguration(busName, endpointName));

    }

}
