using System;

using Automatonymous;

using Cogito.MassTransit.Automatonymous;

namespace Cogito.MassTransit.Autofac.Sample1
{

    [RegisterSagaStateMachine("testsaga")]
    public class TestSaga : global::Cogito.MassTransit.Automatonymous.MassTransitStateMachine<TestSagaState>
    {

        public TestSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => RequestReceived, x => x.CorrelateById(c => c.RequestId.Value));

            Initially(
                When(RequestReceived)
                    .Then(context => context.Instance.Request = context.CaptureRequestToken<TestSagaState, TestSagaRequest, TestSagaRequestState>())
                    .FaultedTo(context => context.Instance.Request, context => new InvalidOperationException())
                    .Finalize());

            SetCompletedWhenFinalized();
        }

        public Event<TestSagaRequest> RequestReceived { get; }

    }

}
