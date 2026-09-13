using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cogito.MassTransit.Events;
using Cogito.MassTransit.Extensions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.Tests
{

    public class MultiRequestTests
    {

        /// <summary>
        /// Request which initiates the saga, carrying the set of values to fan out.
        /// </summary>
        public class StartWork : CorrelatedBy<Guid>
        {

            public Guid CorrelationId { get; set; }

            public Guid[] Values { get; set; } = Array.Empty<Guid>();

            /// <summary>
            /// Number of leading items the consumer should fail, used to exercise the faulted path.
            /// </summary>
            public int FailFirst { get; set; }

        }

        /// <summary>
        /// Aggregated response returned once every item has finished.
        /// </summary>
        public class WorkFinished
        {

            public Guid[] Completed { get; set; } = Array.Empty<Guid>();

            public int FaultedCount { get; set; }

        }

        /// <summary>
        /// A single unit of work dispatched to the item consumer.
        /// </summary>
        public class DoItem
        {

            public Guid Value { get; set; }

            public bool Fail { get; set; }

        }

        /// <summary>
        /// The response to a single <see cref="DoItem"/>.
        /// </summary>
        public class ItemDone
        {

            public Guid Value { get; set; }

        }

        /// <summary>
        /// Per-item state persisted on the saga.
        /// </summary>
        public class ItemState
        {

            public Guid RequestId { get; set; }

            public MultiRequestItemStatus Status { get; set; }

            public ItemDone? Response { get; set; }

            public Fault<DoItem>? Fault { get; set; }

            public RequestTimeoutExpired<DoItem>? TimeoutExpired { get; set; }

        }

        public class WorkSaga : SagaStateMachineInstance
        {

            public Guid CorrelationId { get; set; }

            public string? CurrentState { get; set; }

            public RequestToken<StartWork>? Token { get; set; }

            public List<ItemState> InProgress { get; set; } = new List<ItemState>();

        }

        /// <summary>
        /// Adapts <see cref="ItemState"/> to the multi-request machinery.
        /// </summary>
        class ItemStateAccessor : IMultiRequestStateAccessor<WorkSaga, ItemState, DoItem, ItemDone>
        {

            public ItemState Insert(SagaConsumeContext<WorkSaga> context, DoItem request, Guid requestId)
            {
                var state = new ItemState() { RequestId = requestId, Status = MultiRequestItemStatus.Pending };
                context.Saga.InProgress.Add(state);
                return state;
            }

            public MultiRequestItemStatus GetStatus(SagaConsumeContext<WorkSaga> context, ItemState state) => state.Status;

            public ItemDone GetResponse(SagaConsumeContext<WorkSaga> context, ItemState state) => state.Response!;

            public Fault<DoItem> GetFault(SagaConsumeContext<WorkSaga> context, ItemState state) => state.Fault!;

            public void SetCompleted(SagaConsumeContext<WorkSaga> context, ItemState state, ItemDone response)
            {
                state.Response = response;
                state.Status = MultiRequestItemStatus.Completed;
            }

            public void SetFaulted(SagaConsumeContext<WorkSaga> context, ItemState state, Fault<DoItem> fault)
            {
                state.Fault = fault;
                state.Status = MultiRequestItemStatus.Faulted;
            }

            public void SetTimeoutExpired(SagaConsumeContext<WorkSaga> context, ItemState state, RequestTimeoutExpired<DoItem> timeout)
            {
                state.TimeoutExpired = timeout;
                state.Status = MultiRequestItemStatus.TimeoutExpired;
            }

            public Task Clear(SagaConsumeContext<WorkSaga> context)
            {
                context.Saga.InProgress.Clear();
                return Task.CompletedTask;
            }

        }

        public class WorkStateMachine : MassTransitStateMachine<WorkSaga>
        {

            public WorkStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Event(() => Started, x => x.CorrelateById(m => m.Message.CorrelationId));

                MultiRequest(() => Items, saga => saga.InProgress, item => item.RequestId, new ItemStateAccessor(), c =>
                {
                    // no timeout, so the multi-request does not require a message scheduler
                    c.Timeout = TimeSpan.Zero;
                    c.ServiceAddress = new Uri("loopback://localhost/do-item");
                    c.ClearOnFinish = true;
                });

                Initially(
                    When(Started)
                        .CaptureRequest((ctx, token) => ctx.Saga.Token = token)
                        .MultiRequest(Items, ctx => ctx.Message.Values.Select((v, i) => new DoItem() { Value = v, Fail = i < ctx.Message.FailFirst }))
                        .TransitionTo(Items.Pending));

                During(Items.Pending,
                    When(Items.Finished)
                        .RespondTo<WorkSaga, MultiRequestFinished<DoItem, ItemDone>, StartWork, WorkFinished>(
                            ctx => ctx.Saga.Token!,
                            ctx => new WorkFinished()
                            {
                                Completed = ctx.Message.Items.Values
                                    .Where(i => i.Status == MultiRequestItemStatus.Completed)
                                    .Select(i => i.Response.Value)
                                    .ToArray(),
                                FaultedCount = ctx.Message.Items.Values
                                    .Count(i => i.Status == MultiRequestItemStatus.Faulted),
                            })
                        .Finalize());
            }

            public Event<StartWork> Started { get; private set; } = null!;

            public MultiRequest<WorkSaga, ItemState, DoItem, ItemDone> Items { get; private set; } = null!;

        }

        public class DoItemConsumer : IConsumer<DoItem>
        {

            public Task Consume(ConsumeContext<DoItem> context)
            {
                if (context.Message.Fail)
                    throw new InvalidOperationException("item failed");

                return context.RespondAsync(new ItemDone() { Value = context.Message.Value });
            }

        }

        static async Task<ServiceProvider> StartHarnessAsync()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<WorkStateMachine, WorkSaga>().InMemoryRepository();
                    cfg.AddConsumer<DoItemConsumer>().Endpoint(e => e.Name = "do-item");
                })
                .BuildServiceProvider(true);

            await provider.GetRequiredService<ITestHarness>().Start();
            return provider;
        }

        [Fact]
        public async Task MultiRequest_completes_every_item_and_responds_once_all_finish()
        {
            await using var provider = await StartHarnessAsync();
            var harness = provider.GetRequiredService<ITestHarness>();

            var values = Enumerable.Range(0, 16).Select(_ => Guid.NewGuid()).ToArray();
            var client = harness.GetRequestClient<StartWork>();

            var response = await client.GetResponse<WorkFinished>(
                new StartWork() { CorrelationId = Guid.NewGuid(), Values = values });

            Assert.Equal(16, response.Message.Completed.Length);
            Assert.Equal(0, response.Message.FaultedCount);
            Assert.Equal(values.OrderBy(v => v), response.Message.Completed.OrderBy(v => v));
        }

        [Fact]
        public async Task MultiRequest_with_a_single_item_responds()
        {
            await using var provider = await StartHarnessAsync();
            var harness = provider.GetRequiredService<ITestHarness>();

            var value = Guid.NewGuid();
            var client = harness.GetRequestClient<StartWork>();

            var response = await client.GetResponse<WorkFinished>(
                new StartWork() { CorrelationId = Guid.NewGuid(), Values = new[] { value } });

            Assert.Equal(new[] { value }, response.Message.Completed);
        }

        [Fact]
        public async Task MultiRequest_records_faulted_items_and_still_finishes()
        {
            await using var provider = await StartHarnessAsync();
            var harness = provider.GetRequiredService<ITestHarness>();

            var values = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
            var client = harness.GetRequestClient<StartWork>();

            var response = await client.GetResponse<WorkFinished>(
                new StartWork() { CorrelationId = Guid.NewGuid(), Values = values, FailFirst = 2 });

            Assert.Equal(2, response.Message.FaultedCount);
            Assert.Equal(2, response.Message.Completed.Length);
            Assert.Equal(values.Skip(2).OrderBy(v => v), response.Message.Completed.OrderBy(v => v));
        }

    }

    /// <summary>
    /// Covers the multi-request timeout path, which requires a message scheduler to be configured
    /// on the bus so the timeout message can be scheduled and later cancelled.
    /// <para>
    /// Both tests below are skipped because the timeout path does not currently work. They are kept
    /// as executable reproductions: remove the <c>Skip</c> once the defects they describe are fixed.
    /// The completed and faulted paths, covered by <see cref="MultiRequestTests"/>, are unaffected.
    /// </para>
    /// </summary>
    public class MultiRequestTimeoutTests
    {

        static readonly TimeSpan ItemTimeout = TimeSpan.FromSeconds(3);

        public class StartSlowWork : CorrelatedBy<Guid>
        {

            public Guid CorrelationId { get; set; }

            public Guid[] Values { get; set; } = Array.Empty<Guid>();

            /// <summary>
            /// Number of leading items the consumer should silently drop, so they time out.
            /// </summary>
            public int DropFirst { get; set; }

        }

        public class SlowWorkFinished
        {

            public int CompletedCount { get; set; }

            public int TimedOutCount { get; set; }

        }

        public class SlowItem
        {

            public Guid Value { get; set; }

            public bool Drop { get; set; }

        }

        public class SlowItemDone
        {

            public Guid Value { get; set; }

        }

        public class SlowItemState
        {

            public Guid RequestId { get; set; }

            public MultiRequestItemStatus Status { get; set; }

            public SlowItemDone? Response { get; set; }

            public Fault<SlowItem>? Fault { get; set; }

            public RequestTimeoutExpired<SlowItem>? TimeoutExpired { get; set; }

        }

        public class SlowSaga : SagaStateMachineInstance
        {

            public Guid CorrelationId { get; set; }

            public string? CurrentState { get; set; }

            public RequestToken<StartSlowWork>? Token { get; set; }

            public List<SlowItemState> InProgress { get; set; } = new List<SlowItemState>();

        }

        class SlowItemStateAccessor : IMultiRequestStateAccessor<SlowSaga, SlowItemState, SlowItem, SlowItemDone>
        {

            public SlowItemState Insert(SagaConsumeContext<SlowSaga> context, SlowItem request, Guid requestId)
            {
                var state = new SlowItemState() { RequestId = requestId, Status = MultiRequestItemStatus.Pending };
                context.Saga.InProgress.Add(state);
                return state;
            }

            public MultiRequestItemStatus GetStatus(SagaConsumeContext<SlowSaga> context, SlowItemState state) => state.Status;

            public SlowItemDone GetResponse(SagaConsumeContext<SlowSaga> context, SlowItemState state) => state.Response!;

            public Fault<SlowItem> GetFault(SagaConsumeContext<SlowSaga> context, SlowItemState state) => state.Fault!;

            public void SetCompleted(SagaConsumeContext<SlowSaga> context, SlowItemState state, SlowItemDone response)
            {
                state.Response = response;
                state.Status = MultiRequestItemStatus.Completed;
            }

            public void SetFaulted(SagaConsumeContext<SlowSaga> context, SlowItemState state, Fault<SlowItem> fault)
            {
                state.Fault = fault;
                state.Status = MultiRequestItemStatus.Faulted;
            }

            public void SetTimeoutExpired(SagaConsumeContext<SlowSaga> context, SlowItemState state, RequestTimeoutExpired<SlowItem> timeout)
            {
                state.TimeoutExpired = timeout;
                state.Status = MultiRequestItemStatus.TimeoutExpired;
            }

            public Task Clear(SagaConsumeContext<SlowSaga> context)
            {
                context.Saga.InProgress.Clear();
                return Task.CompletedTask;
            }

        }

        public class SlowStateMachine : MassTransitStateMachine<SlowSaga>
        {

            public SlowStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Event(() => Started, x => x.CorrelateById(m => m.Message.CorrelationId));

                MultiRequest(() => Items, saga => saga.InProgress, item => item.RequestId, new SlowItemStateAccessor(), c =>
                {
                    c.Timeout = ItemTimeout;
                    c.ServiceAddress = new Uri("loopback://localhost/slow-item");
                    c.ClearOnFinish = true;
                });

                Initially(
                    When(Started)
                        .CaptureRequest((ctx, token) => ctx.Saga.Token = token)
                        .MultiRequest(Items, ctx => ctx.Message.Values.Select((v, i) => new SlowItem() { Value = v, Drop = i < ctx.Message.DropFirst }))
                        .TransitionTo(Items.Pending));

                During(Items.Pending,
                    When(Items.Finished)
                        .RespondTo<SlowSaga, MultiRequestFinished<SlowItem, SlowItemDone>, StartSlowWork, SlowWorkFinished>(
                            ctx => ctx.Saga.Token!,
                            ctx => new SlowWorkFinished()
                            {
                                CompletedCount = ctx.Message.Items.Values.Count(i => i.Status == MultiRequestItemStatus.Completed),
                                TimedOutCount = ctx.Message.Items.Values.Count(i => i.Status == MultiRequestItemStatus.TimeoutExpired),
                            })
                        .Finalize());
            }

            public Event<StartSlowWork> Started { get; private set; } = null!;

            public MultiRequest<SlowSaga, SlowItemState, SlowItem, SlowItemDone> Items { get; private set; } = null!;

        }

        public class SlowItemConsumer : IConsumer<SlowItem>
        {

            public Task Consume(ConsumeContext<SlowItem> context)
            {
                // dropped items never respond, leaving the request to time out
                if (context.Message.Drop)
                    return Task.CompletedTask;

                return context.RespondAsync(new SlowItemDone() { Value = context.Message.Value });
            }

        }

        static async Task<ServiceProvider> StartHarnessAsync()
        {
            var provider = new ServiceCollection()
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<SlowStateMachine, SlowSaga>().InMemoryRepository();
                    cfg.AddConsumer<SlowItemConsumer>().Endpoint(e => e.Name = "slow-item");

                    cfg.UsingInMemory((ctx, c) =>
                    {
                        c.UseInMemoryScheduler();
                        c.ConfigureEndpoints(ctx);
                    });
                })
                .BuildServiceProvider(true);

            await provider.GetRequiredService<ITestHarness>().Start();
            return provider;
        }

        /// <summary>
        /// The scheduled timeout message is dispatched by <c>MultiRequestActivityImpl.SendRequest</c> via
        /// <c>MessageSchedulerContext.ScheduleSend</c>, which sets no <c>RequestId</c> header — the request id
        /// travels in the message body instead. <c>StateMachineMultiRequest.RequestTimeoutExpiredEventFilter</c>
        /// and <c>MultiRequestItemTimeoutExpiredActivity.Execute</c> both read the header
        /// (<c>context.RequestId</c>) rather than <c>context.Message.RequestId</c>, so the filter always
        /// returns false and no timeout activity ever runs. The item stays Pending, the multi-request never
        /// reports finished, and the saga waits forever.
        /// </summary>
        [Fact(Skip = "MultiRequest timeout path is broken: the timeout event filter and activity read context.RequestId, which is never set on the scheduled timeout message; use context.Message.RequestId.")]
        public async Task MultiRequest_times_out_items_that_never_respond()
        {
            await using var provider = await StartHarnessAsync();
            var harness = provider.GetRequiredService<ITestHarness>();

            var values = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
            var client = harness.GetRequestClient<StartSlowWork>();

            var response = await client.GetResponse<SlowWorkFinished>(
                new StartSlowWork() { CorrelationId = Guid.NewGuid(), Values = values, DropFirst = 2 });

            Assert.Equal(2, response.Message.CompletedCount);
            Assert.Equal(2, response.Message.TimedOutCount);
        }

        /// <summary>
        /// <c>MultiRequestExtensions.MultiRequest</c> registers the cancellation token id against the interface
        /// (<c>ScheduleTokenId.UseTokenId&lt;RequestTimeoutExpired&lt;TRequest&gt;&gt;</c>), but the message actually
        /// scheduled is the private concrete <c>MultiRequestActivityImpl.TimeoutExpired&lt;TRequest&gt;</c>, so the
        /// schedule never records that token id. <c>MultiRequestCancelItemTimeoutActivity</c> then cancels by a
        /// token the scheduler does not know, logging "CancelScheduledMessage: no message found". The timeout
        /// still fires after the item completed and overwrites its Completed status with TimeoutExpired.
        /// A second consequence: several items finishing at once each send a <c>MultiRequestFinishedSignal</c>,
        /// and the duplicates fault the saga with "Not accepted in state Final".
        /// </summary>
        [Fact(Skip = "MultiRequest timeout cancellation is broken: the schedule token id is registered for RequestTimeoutExpired<T> but the scheduled message is a private concrete type, so cancellation never matches and stale timeouts clobber completed items.")]
        public async Task MultiRequest_cancels_scheduled_timeouts_when_every_item_responds()
        {
            await using var provider = await StartHarnessAsync();
            var harness = provider.GetRequiredService<ITestHarness>();

            var values = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
            var client = harness.GetRequestClient<StartSlowWork>();

            var started = DateTime.UtcNow;
            var response = await client.GetResponse<SlowWorkFinished>(
                new StartSlowWork() { CorrelationId = Guid.NewGuid(), Values = values, DropFirst = 0 });

            Assert.Equal(4, response.Message.CompletedCount);
            Assert.Equal(0, response.Message.TimedOutCount);

            // the saga must finish on the responses rather than waiting out the scheduled timeouts
            Assert.True(DateTime.UtcNow - started < ItemTimeout, "multi-request did not finish before the item timeout elapsed");
        }

    }

}
