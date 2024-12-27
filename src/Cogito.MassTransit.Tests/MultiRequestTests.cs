using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Events;

using Cogito.MassTransit.Automatonymous;
using Cogito.MassTransit.Automatonymous.Events;

using FluentAssertions;

using MassTransit;
using MassTransit.Saga;
using MassTransit.Testing;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.MassTransit.Tests
{

    [TestClass]
    public class MultiRequestTests
    {

        /// <summary>
        /// Represents the state of the saga.
        /// </summary>
        public class TestSagaState : SagaStateMachineInstance
        {

            public Guid CorrelationId { get; set; }

            public string CurrentState { get; set; }

            public RequestToken<TestSagaRequest> Request { get; set; }

            public List<TestSagaRequestState> InProgress { get; set; }

        }

        /// <summary>
        /// Represents a single request state item.
        /// </summary>
        public class TestSagaRequestState
        {

            public Guid RequestId { get; set; }

            public MultiRequestItemStatus Status { get; set; }

            public TestConsumerResponse Response { get; set; }

            public Fault<TestConsumerRequest> Fault { get; set; }

            public RequestTimeoutExpired<TestConsumerRequest> TimeoutExpired { get; set; }

        }

        /// <summary>
        /// Provides an adaptor to manage request state items.
        /// </summary>
        public class TestSagaRequestStateAccessor : IMultiRequestStateAccessor<TestSagaState, TestSagaRequestState, TestConsumerRequest, TestConsumerResponse>
        {

            public TestSagaRequestState Insert(InstanceContext<TestSagaState> context, TestConsumerRequest request, Guid requestId)
            {
                context.Instance.InProgress ??= new List<TestSagaRequestState>();
                var state = new TestSagaRequestState() { RequestId = requestId, Status = MultiRequestItemStatus.Pending };
                context.Instance.InProgress.Add(state);
                return state;
            }

            public MultiRequestItemStatus GetStatus(InstanceContext<TestSagaState> context, TestSagaRequestState state)
            {
                return state.Status;
            }

            public TestConsumerResponse GetResponse(InstanceContext<TestSagaState> context, TestSagaRequestState state)
            {
                return state.Response;
            }

            public Fault<TestConsumerRequest> GetFault(InstanceContext<TestSagaState> context, TestSagaRequestState state)
            {
                return state.Fault;
            }

            public void SetCompleted(InstanceContext<TestSagaState> context, TestSagaRequestState state, TestConsumerResponse response)
            {
                state.Response = response;
                state.Status = MultiRequestItemStatus.Completed;
            }

            public void SetFaulted(InstanceContext<TestSagaState> context, TestSagaRequestState state, Fault<TestConsumerRequest> fault)
            {
                state.Fault = fault;
                state.Status = MultiRequestItemStatus.Faulted;
            }

            public void SetTimeoutExpired(InstanceContext<TestSagaState> context, TestSagaRequestState state, RequestTimeoutExpired<TestConsumerRequest> timeout)
            {
                state.TimeoutExpired = timeout;
                state.Status = MultiRequestItemStatus.TimeoutExpired;
            }

            public Task Clear(InstanceContext<TestSagaState> context)
            {
                context.Instance.InProgress?.Clear();
                return Task.CompletedTask;
            }

        }

        /// <summary>
        /// Sample saga state machine.
        /// </summary>
        public class TestSagaStateMachine : Cogito.MassTransit.Automatonymous.MassTransitStateMachine<TestSagaState>
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            public TestSagaStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Event(() => Message);

                MultiRequest(() => SendMultiRequest, instance => instance.InProgress, state => state.RequestId, new TestSagaRequestStateAccessor(), c =>
                {
                    c.Timeout = TimeSpan.FromMinutes(1);
                    c.ServiceAddress = new Uri("loopback://localhost/consumer");
                    c.ClearOnFinish = true;
                });

                Initially(
                    When(Message)
                        .CaptureRequest((context, request) => { Console.WriteLine(this); context.Instance.Request = request; })
                        .MultiRequest(SendMultiRequest, context => context.Data.Values.Select(i => new TestConsumerRequest() { Value = i }))
                        .TransitionTo(SendMultiRequest.Pending));

                During(SendMultiRequest.Pending,
                    When(SendMultiRequest.Completed)
                        .Then(i => Console.WriteLine("Complete: {0}", i.Data.Value)),
                    When(SendMultiRequest.Faulted)
                        .Then(i => Console.WriteLine("Faulted: {0}", i.Data.Message)),
                    When(SendMultiRequest.TimeoutExpired)
                        .Then(i => Console.WriteLine("TimeoutExpired: {0}", i.Data.RequestId)),
                    When(SendMultiRequest.Finished)
                        .RespondTo(context => context.Instance.Request, context => new TestSagaResponse() { Values = context.Data.Items.Select(i => i.Value.Response.Value).ToArray() })
                        .Finalize());
            }

            /// <summary>
            /// Initiates the saga.
            /// </summary>
            public Event<TestSagaRequest> Message { get; set; }

            /// <summary>
            /// Issues multiple requests.
            /// </summary>
            public MultiRequest<TestSagaState, TestSagaRequestState, TestConsumerRequest, TestConsumerResponse> SendMultiRequest { get; set; }

        }

        public class TestSagaRequest : CorrelatedBy<Guid>
        {

            public Guid CorrelationId { get; set; }

            public Guid[] Values { get; set; }

        }

        public class TestSagaResponse
        {

            public Guid[] Values { get; set; }

        }

        public class TestConsumer : IConsumer<TestConsumerRequest>
        {

            public Task Consume(ConsumeContext<TestConsumerRequest> context)
            {
                context.Respond(new TestConsumerResponse() { Value = context.Message.Value });
                return Task.CompletedTask;
            }

        }

        public class TestConsumerRequest
        {

            public Guid Value { get; set; }

        }

        public class TestConsumerResponse
        {

            public Guid Value { get; set; }

        }

        [TestMethod]
        public async Task Test_thing()
        {
            var harness = new InMemoryTestHarness();
            harness.OnConfigureInMemoryBus += Harness_OnConfigureInMemoryBus;
            var saga = harness.StateMachineSaga(new TestSagaStateMachine(), new InMemorySagaRepository<TestSagaState>(), "saga");
            var consumer = harness.Consumer<TestConsumer>("consumer");

            await harness.Start();
            try
            {
                var values = Enumerable.Range(0, 16).Select(i => Guid.NewGuid()).ToArray();
                var response = await harness.Bus.Request<TestSagaRequest, TestSagaResponse>(new TestSagaRequest() { CorrelationId = Guid.NewGuid(), Values = values });
                response.Message.Values.Should().HaveCount(16);
            }
            finally
            {
                await harness.Stop();
            }
        }

        void Harness_OnConfigureInMemoryBus(IInMemoryBusFactoryConfigurator obj)
        {
            obj.UseInMemoryScheduler();
        }

    }

}
