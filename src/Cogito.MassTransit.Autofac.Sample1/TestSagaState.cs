using System;

using Automatonymous;

using Cogito.MassTransit.Automatonymous;

namespace Cogito.MassTransit.Autofac.Sample1
{

    public class TestSagaState : SagaStateMachineInstance
    {

        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public TestSagaRequestState Request { get; set; }

    }

}
