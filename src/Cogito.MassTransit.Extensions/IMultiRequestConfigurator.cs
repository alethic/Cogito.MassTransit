using System;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Configures a multirequest declaration.
    /// </summary>
    public interface IMultiRequestConfigurator
    {

        /// <summary>
        /// The address of the service to which requests are sent.
        /// </summary>
        Uri ServiceAddress { set; }

        /// <summary>
        /// The amount of time to wait for a response before raising a timeout.
        /// </summary>
        TimeSpan Timeout { set; }

        /// <summary>
        /// Should the request state be cleared on finish?
        /// </summary>
        bool ClearOnFinish { set; }

    }

}
