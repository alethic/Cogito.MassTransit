using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known bus names.
    /// </summary>
    public class BusNameProvider
    {

        readonly IEnumerable<IBusNameSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public BusNameProvider(IOrderedEnumerable<IBusNameSource> sources)  
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known bus names.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetBusNames() => sources.SelectMany(i => i.GetBusNames());

    }

}
