using System;
using System.Collections.Concurrent;

using MassTransit;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Creates bus instances from registered configuration.
    /// </summary>
    public class BusProvider
    {

        readonly BusRegistrationFactory factory;
        readonly ConcurrentDictionary<string, IBusControl> cache = new ConcurrentDictionary<string, IBusControl>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="factory"></param>
        public BusProvider(BusRegistrationFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Gets the bus instance by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IBusControl GetBus(string name)
        {
            return cache.GetOrAdd(name, factory.CreateBus);
        }

        /// <summary>
        /// Gets the default bus instance.
        /// </summary>
        /// <returns></returns>
        public IBusControl GetBus()
        {
            return GetBus("");
        }

    }

}
