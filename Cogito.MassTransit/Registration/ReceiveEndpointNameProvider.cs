using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known <see cref="IReceiveEndpointConfiguration"/> instances.
    /// </summary>
    public class ReceiveEndpointNameProvider
    {

        readonly IEnumerable<IReceiveEndpointNameSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public ReceiveEndpointNameProvider(IOrderedEnumerable<IReceiveEndpointNameSource> sources)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known <see cref="IReceiveEndpointConfiguration"/> instances for the given endpoint name.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetEndpointNames(string busName) => sources.SelectMany(i => i.GetEndpointNames(busName)).Distinct();

    }

}
