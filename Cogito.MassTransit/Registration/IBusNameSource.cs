using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides a set of bus names to be registered.
    /// </summary>
    public interface IBusNameSource
    {

        /// <summary>
        /// Gets a set of bus names to register.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetBusNames();

    }

}
