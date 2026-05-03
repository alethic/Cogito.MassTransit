using System;

namespace Cogito.MassTransit.Extensions.Internal
{

    class StateMachineMultiRequestConfigurator<TRequest> : IMultiRequestConfigurator, MultiRequestSettings
        where TRequest : class
    {

        public Uri ServiceAddress { get; set; }

        public TimeSpan Timeout { get; set; }

        public bool ClearOnFinish { get; set; }

    }

}
