using System;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Describes the settings applied to a multirequest.
    /// </summary>
    public interface MultiRequestSettings
    {

        /// <summary>
        /// The address of the service to which requests are sent.
        /// </summary>
        Uri ServiceAddress { get; }

        /// <summary>
        /// The amount of time to wait for a response before raising a timeout.
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        /// Should the request state be cleared on finish?
        /// </summary>
        bool ClearOnFinish { get; }

    }

}
