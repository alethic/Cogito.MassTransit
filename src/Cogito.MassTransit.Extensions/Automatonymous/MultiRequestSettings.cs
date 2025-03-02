
using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    /// <summary>
    /// Describes the settings applied to a multirequest.
    /// </summary>
    public interface MultiRequestSettings : RequestSettings
    {

        /// <summary>
        /// Should the request state be cleared on finish?
        /// </summary>
        bool ClearOnFinish { get; }

    }

}
