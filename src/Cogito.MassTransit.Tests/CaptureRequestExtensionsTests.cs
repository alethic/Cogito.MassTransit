using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.Tests
{

    public class CaptureRequestExtensionsTests
    {

        public class StartRequest
        {
            public Guid CorrelationId { get; set; }
        }

        public class RequestResponse
        {
            public Guid CorrelationId { get; set; }
        }

        public class CaptureSaga : SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string? CurrentState { get; set; }
            public RequestToken<StartRequest>? CapturedToken { get; set; }
        }

        public class CaptureStateMachine : MassTransitStateMachine<CaptureSaga>
        {

            public CaptureStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Event(() => Started, x => x.CorrelateById(m => m.Message.CorrelationId));

                Initially(
                    When(Started)
                        .CaptureRequest((ctx, token) => ctx.Saga.CapturedToken = token)
                        .TransitionTo(Active));
            }

            public State Active { get; private set; } = null!;

            public Event<StartRequest> Started { get; private set; } = null!;

        }

        [Fact]
        public async Task CaptureRequest_populates_token_on_saga_when_message_is_a_request()
        {
            await using var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<CaptureStateMachine, CaptureSaga>().InMemoryRepository();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var sagaId = Guid.NewGuid();
            var clientFactory = provider.GetRequiredService<IClientFactory>();
            var client = clientFactory.CreateRequestClient<StartRequest>(timeout: TimeSpan.FromSeconds(30));

            // fire-and-forget the request from a separate task so we can let the saga capture it
            _ = Task.Run(async () =>
            {
                try
                {
                    await client.GetResponse<RequestResponse>(new StartRequest { CorrelationId = sagaId });
                }
                catch
                {
                    // request will time out because the saga doesn't reply; that's expected
                }
            });

            var sagaHarness = harness.GetSagaStateMachineHarness<CaptureStateMachine, CaptureSaga>();
            Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == sagaId));

            // wait until the saga reaches the Active state
            var instance = await sagaHarness.Exists(sagaId, x => x.Active);
            Assert.NotNull(instance);

            var saga = sagaHarness.Sagas.Contains(sagaId);
            Assert.NotNull(saga.CapturedToken);
            Assert.NotNull(saga.CapturedToken!.RequestId);
            Assert.NotNull(saga.CapturedToken.ResponseAddress);
            Assert.Equal(sagaId, saga.CapturedToken.Request!.CorrelationId);
        }

        [Fact]
        public async Task CaptureRequest_does_not_throw_when_message_is_not_a_request()
        {
            await using var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<CaptureStateMachine, CaptureSaga>().InMemoryRepository();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var sagaId = Guid.NewGuid();
            await harness.Bus.Publish(new StartRequest { CorrelationId = sagaId });

            var sagaHarness = harness.GetSagaStateMachineHarness<CaptureStateMachine, CaptureSaga>();
            Assert.NotNull(await sagaHarness.Exists(sagaId, x => x.Active));

            // when the message is not a request the captured token has no RequestId
            var saga = sagaHarness.Sagas.Contains(sagaId);
            Assert.NotNull(saga.CapturedToken);
            Assert.Null(saga.CapturedToken!.RequestId);
        }

        [Fact]
        public void CaptureRequest_throws_when_callback_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CaptureRequestExtensions.CaptureRequest<CaptureSaga, StartRequest>(null!, null!));
        }

        [Fact]
        public void CaptureRequestAsync_throws_when_callback_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CaptureRequestExtensions.CaptureRequestAsync<CaptureSaga, StartRequest>(null!, null!));
        }

    }

}
