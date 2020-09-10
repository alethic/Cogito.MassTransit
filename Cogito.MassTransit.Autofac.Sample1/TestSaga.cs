using Automatonymous;

namespace Cogito.MassTransit.Autofac.Sample1
{


    [RegisterSagaStateMachine("testsaga")]
    public class TestSaga : MassTransitStateMachine<TestSagaState>
    {

        public TestSaga()
        {
            InstanceState(x => x.CurrentState);
            Event(() => MessageReceived, x => x.CorrelateById(ctx => ctx.Message.Id));

            Initially(
                When(MessageReceived)
                    .TransitionTo(Accepted));

            SetCompletedWhenFinalized();
        }

        public State Accepted { get; }

        public Event<TestSagaMessage> MessageReceived { get; }

    }

}
