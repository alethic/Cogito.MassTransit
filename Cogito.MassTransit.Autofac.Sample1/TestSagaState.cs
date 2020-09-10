using System;

using Automatonymous;

namespace Cogito.MassTransit.Autofac.Sample1
{

    public class TestSagaState : SagaStateMachineInstance
    {

        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

    }

}
