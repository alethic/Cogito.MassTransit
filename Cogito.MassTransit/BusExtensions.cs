using System;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Various extension methods for <see cref="IBus"/>.
    /// </summary>
    public static class BusExtensions
    {

        /// <summary>
        /// Gets the root URI of the bus.
        /// </summary>
        /// <param name="bus"></param>
        /// <returns></returns>
        public static Uri GetUri(this IBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            return new Uri(bus.Address, "/");
        }

        /// <summary>
        /// Gets the absolute endpoint URI for the given relative or absolute endpoint URI.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="endpointUri"></param>
        /// <returns></returns>
        public static Uri GetAbsoluteEndpointUri(this IBus bus, Uri endpointUri)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));
            if (endpointUri == null)
                throw new ArgumentNullException(nameof(endpointUri));
            
            return new Uri(GetUri(bus), endpointUri);
        }

    }

}
