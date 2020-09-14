using System;

namespace Cogito.MassTransit.Automatonymous.SagaConfigurators
{

    public class StateMachineMultiRequestConfigurator<TRequest> : IMultiRequestConfigurator, MultiRequestSettings where TRequest : class
    {

        public MultiRequestSettings Settings => this;

        public Uri ServiceAddress { get; set; }

        public TimeSpan Timeout { get; set; }

        public bool ClearOnFinish { get; set; }

    }

}
