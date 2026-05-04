using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.Tests
{

    public class RespondToFaultedToExtensionsTests
    {

        public class DoWork
        {
            public Guid CorrelationId { get; set; }
            public bool Fail { get; set; }
        }

        public class WorkDone
        {
            public Guid CorrelationId { get; set; }
            public string? Result { get; set; }
        }

        public class WorkSaga : SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string? CurrentState { get; set; }
            public RequestToken<DoWork>? Token { get; set; }
        }

        public class WorkStateMachine : MassTransitStateMachine<WorkSaga>
        {

            public WorkStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Event(() => Started, x => x.CorrelateById(m => m.Message.CorrelationId));

                Initially(
                    When(Started)
                        .CaptureRequest((ctx, token) => ctx.Saga.Token = token)
                        .IfElse(ctx => ctx.Message.Fail,
                            fail => fail.FaultedTo<WorkSaga, DoWork, DoWork>(
                                ctx => ctx.Saga.Token!,
                                new InvalidOperationException("nope"))
                                .TransitionTo(Faulted),
                            ok => ok.RespondTo<WorkSaga, DoWork, DoWork, WorkDone>(
                                ctx => ctx.Saga.Token!,
                                ctx => new WorkDone { CorrelationId = ctx.Saga.CorrelationId, Result = "ok" })
                                .TransitionTo(Completed)));
            }

            public State Completed { get; private set; } = null!;
            public State Faulted { get; private set; } = null!;

            public Event<DoWork> Started { get; private set; } = null!;

        }

        static async Task<ServiceProvider> StartHarnessAsync()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<WorkStateMachine, WorkSaga>().InMemoryRepository();
                })
                .BuildServiceProvider(true);

            await provider.GetRequiredService<ITestHarness>().Start();
            return provider;
        }

        [Fact]
        public async Task RespondTo_sends_response_to_request_client()
        {
            await using var provider = await StartHarnessAsync();
            var harness = provider.GetRequiredService<ITestHarness>();

            var sagaId = Guid.NewGuid();
            var client = harness.GetRequestClient<DoWork>();

            var response = await client.GetResponse<WorkDone>(new DoWork { CorrelationId = sagaId, Fail = false });

            Assert.Equal(sagaId, response.Message.CorrelationId);
            Assert.Equal("ok", response.Message.Result);
        }

        [Fact]
        public async Task FaultedTo_sends_fault_to_request_client()
        {
            await using var provider = await StartHarnessAsync();
            var harness = provider.GetRequiredService<ITestHarness>();

            var sagaId = Guid.NewGuid();
            var client = harness.GetRequestClient<DoWork>();

            var ex = await Assert.ThrowsAsync<RequestFaultException>(async () =>
            {
                await client.GetResponse<WorkDone>(new DoWork { CorrelationId = sagaId, Fail = true });
            });

            Assert.Contains("nope", ex.Message);
        }

    }

}
