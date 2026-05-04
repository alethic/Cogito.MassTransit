using System;

namespace Cogito.MassTransit.Extensions.Internal
{

    /// <summary>
    /// Default <see cref="IMultiRequestConfigurator"/> implementation that also exposes the configured values as <see cref="MultiRequestSettings"/>.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    class StateMachineMultiRequestConfigurator<TRequest> : IMultiRequestConfigurator, MultiRequestSettings
        where TRequest : class
    {

        /// <inheritdoc cref="MultiRequestSettings.ServiceAddress"/>
        public Uri ServiceAddress { get; set; }

        /// <inheritdoc cref="MultiRequestSettings.Timeout"/>
        public TimeSpan Timeout { get; set; }

        /// <inheritdoc cref="MultiRequestSettings.ClearOnFinish"/>
        public bool ClearOnFinish { get; set; }

    }

}
