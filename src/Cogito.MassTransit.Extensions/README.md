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

Configure a multi-request on a saga state machine:

```csharp
public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public MultiRequest<OrderState, CheckStock, StockChecked> CheckStock { get; set; }
}
```

Send it, and the machine continues when every reply is in:

```csharp
During(Checking,
    When(CheckStock.Completed)
        .Then(context => ...));
```

Each item's outcome is reported as a `MultiRequestFinishedItem` with a `MultiRequestItemStatus`, so a
partial failure is visible rather than indistinguishable from success. `MultiRequestSettings` covers
timeouts, and `RequestTimeoutExpired` is raised when one elapses.

`CaptureRequestExtensions` and `IRequestToken` carry the originating request through a conversation;
`FaultedToExtensions` routes faults, and `ExceptionInfoException` turns a serialised MassTransit
`ExceptionInfo` back into a throwable exception.

## License

MIT.
