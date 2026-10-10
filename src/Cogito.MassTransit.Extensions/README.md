# Cogito.MassTransit.Extensions

Fan out a request to many handlers from inside a saga, and wait for all of them.

## Why

MassTransit's request/response covers one request and one reply. A saga that has to ask *n* services
and continue when the last answers has to track the outstanding set itself — correlating replies,
handling faults, and timing out — which is fiddly state-machine code written the same way each time.

## Install

```shell
dotnet add package Cogito.MassTransit.Extensions
```

## Use

A multi-request keeps one state item per outstanding request on the saga, and reaches it through an
`IMultiRequestStateAccessor` that you implement. Declare it on a state machine derived from
`Cogito.MassTransit.MassTransitStateMachine<TSaga>`, send it from an event, and continue on `Finished`
once every request has completed, faulted or timed out:

```csharp
public class OrderStateMachine : Cogito.MassTransit.MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => Submitted, x => x.CorrelateById(m => m.Message.OrderId));

        MultiRequest(() => CheckStock, saga => saga.Lines, line => line.RequestId, new LineAccessor(), c =>
        {
            c.ServiceAddress = new Uri("queue:check-stock");
            c.Timeout = TimeSpan.FromSeconds(30);
        });

        Initially(
            When(Submitted)
                .MultiRequest(CheckStock, ctx => ctx.Message.Lines.Select(l => new CheckStock(l.Sku)))
                .TransitionTo(CheckStock.Pending));

        During(CheckStock.Pending,
            When(CheckStock.Finished)
                .Then(ctx => ...));
    }

    public Event<SubmitOrder> Submitted { get; private set; }

    public MultiRequest<OrderState, LineState, CheckStock, StockChecked> CheckStock { get; private set; }
}
```

A state machine built without a class, through MassTransit's `MassTransitStateMachine<TSaga>.New`,
declares it on the modifier instead, and receives the multi-request as an `out` parameter:

```csharp
var machine = MassTransitStateMachine<OrderState>.New(cfg => cfg
    .InstanceState(x => x.CurrentState)
    .Event("Submitted", x => x.CorrelateById(m => m.Message.OrderId), out Event<SubmitOrder> submitted)
    .MultiRequest("CheckStock", saga => saga.Lines, line => line.RequestId, new LineAccessor(),
        c => c.ServiceAddress = new Uri("queue:check-stock"), out var checkStock)
    .Initially()
        .When(submitted, b => b
            .MultiRequest(checkStock, ctx => ctx.Message.Lines.Select(l => new CheckStock(l.Sku)))
            .TransitionTo(checkStock.Pending))
        .CommitActivities()
    .During(checkStock.Pending)
        .When(checkStock.Finished, b => b.Then(ctx => ...))
        .CommitActivities());
```

Either way, the multi-request's events and pending state are named after it: `CheckStock.Finished`,
`CheckStock.Pending` and so on. A timeout needs a message scheduler configured on the bus.

Each item's outcome is reported as a `MultiRequestFinishedItem` with a `MultiRequestItemStatus`, so a
partial failure is visible rather than indistinguishable from success. `MultiRequestSettings` covers
timeouts, and `RequestTimeoutExpired` is raised when one elapses.

`CaptureRequestExtensions` and `IRequestToken` carry the originating request through a conversation;
`FaultedToExtensions` routes faults, and `ExceptionInfoException` turns a serialised MassTransit
`ExceptionInfo` back into a throwable exception.

## License

MIT.
