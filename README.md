# Cogito.MassTransit

Extensions and utilities for working with [MassTransit](https://masstransit.io/).

This repository ships a small family of NuGet packages that layer additional
helpers on top of MassTransit 8.5.x:

| Package                              | Description                                                                                          | Targets            |
| ------------------------------------ | ---------------------------------------------------------------------------------------------------- | ------------------ |
| **Cogito.MassTransit**               | Core utilities — `IBus` extensions and a typed periodic-pulse message hierarchy (`P`, `PT1M`, …).    | `net472`, `net8.0` |
| **Cogito.MassTransit.Extensions**    | Saga state-machine helpers — capture an incoming request and `RespondTo` / `FaultedTo` it later.     | `net472`, `net8.0` |
| **Cogito.MassTransit.Scheduler**     | A Quartz-backed hosted service that publishes the periodic pulse messages on a fixed cadence.        | `net472`, `net8.0` |

A runnable sample (`Cogito.MassTransit.Scheduler.Sample1`) and a test project
(`Cogito.MassTransit.Tests`) are also included.

---

## Repository layout

```
Cogito.MassTransit.sln
Directory.Build.props
GitVersion.yml
global.json
.github/workflows/Cogito.MassTransit.yml
src/
  Cogito.MassTransit/                   # Core: IBus extensions + Pulse messages
  Cogito.MassTransit.Extensions/        # Saga state-machine: RespondTo / FaultedTo
  Cogito.MassTransit.Scheduler/         # Quartz-driven periodic publisher
  Cogito.MassTransit.Scheduler.Sample1/ # Runnable sample host
  Cogito.MassTransit.Tests/             # Unit tests
  dist-nuget/                           # NuGet packing project
  dist-tests/                           # Test packing/distribution
```

---

## Cogito.MassTransit

Core utilities. References:

- `MassTransit` 8.5.9
- `Cogito.DependencyInjection` 1.0.0
- `Cogito.Extensions.Options` 4.0.0
- `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.2
- `Microsoft.Extensions.Hosting.Abstractions` 8.0.1

### `BusExtensions` — `IBus` helpers

Namespace: `Cogito.MassTransit`

```csharp
using Cogito.MassTransit;
using MassTransit;

// IBus bus = ...;

// The bus's root URI (drops any path/queue from bus.Address).
Uri root = bus.GetUri();

// Resolve a relative endpoint URI against the bus root,
// or pass an absolute URI through unchanged.
Uri absolute = bus.GetAbsoluteEndpointUri(new Uri("queue:my-queue", UriKind.Relative));
```

| Method                                          | Purpose                                                                  |
| ----------------------------------------------- | ------------------------------------------------------------------------ |
| `Uri GetUri(this IBus bus)`                     | Returns `new Uri(bus.Address, "/")` — the bus's root.                    |
| `Uri GetAbsoluteEndpointUri(IBus, Uri)`         | Resolves a relative endpoint URI against the bus root.                   |

### Periodic pulse messages

Namespace: `Cogito.MassTransit.Scheduling.Periodic`

A small set of strongly-typed message classes representing periodic
"heartbeats" emitted by the scheduler. All derive from the abstract base
class `P` and carry timing metadata from the underlying Quartz trigger:

```csharp
public abstract class P
{
    public TimeSpan        Interval             { get; set; }
    public DateTimeOffset? FireTimeUtc          { get; set; }
    public DateTimeOffset? NextFireTimeUtc      { get; set; }
    public DateTimeOffset? PreviousFireTimeUtc  { get; set; }
    public DateTimeOffset? ScheduledFireTimeUtc { get; set; }
}
```

Concrete messages (one per supported interval, named after their ISO‑8601
duration):

| Type    | Interval    |
| ------- | ----------- |
| `PT1M`  | 1 minute    |
| `PT5M`  | 5 minutes   |
| `PT15M` | 15 minutes  |
| `PT30M` | 30 minutes  |
| `PT1H`  | 1 hour      |
| `PT2H`  | 2 hours     |
| `PT6H`  | 6 hours     |
| `PT12H` | 12 hours    |
| `P1D`   | 1 day       |

Consumers simply implement `IConsumer<PT1M>` (etc.) and MassTransit will
deliver the pulse on every tick.

```csharp
using Cogito.MassTransit.Scheduling.Periodic;
using MassTransit;

public class CleanupConsumer : IConsumer<PT15M>
{
    public Task Consume(ConsumeContext<PT15M> context)
    {
        // Runs every 15 minutes.
        return Task.CompletedTask;
    }
}
```

---

## Cogito.MassTransit.Scheduler

Adds a Quartz-driven hosted service that keeps the periodic-pulse jobs
registered and publishes the appropriate `P`-derived message on each tick.

References:

- `Cogito.Core` 3.1.36
- `Cogito.Quartz` 2.1.0
- `MassTransit.Quartz` 8.5.9
- `Microsoft.Extensions.Hosting.Abstractions` 8.0.1

### Registration

Namespace: `Cogito.MassTransit.Scheduler`

```csharp
using Cogito.MassTransit.Scheduler;
using Microsoft.Extensions.DependencyInjection;

services.AddPeriodicJobScheduler();
```

`AddPeriodicJobScheduler` registers the three components that drive the
feature via `Cogito.DependencyInjection` attribute discovery:

| Type                   | Lifetime  | Role                                                                                       |
| ---------------------- | --------- | ------------------------------------------------------------------------------------------ |
| `PeriodicScheduler`    | Singleton `IHostedService` | Background loop that ensures the bootstrap Quartz job/trigger exists.     |
| `PeriodicSchedulerJob` | Scoped Quartz `IJob`       | Reconciles the per-interval Quartz triggers (versioned, idempotent).      |
| `PeriodicJob`          | Scoped Quartz `IJob`       | Fires for each interval and `Publish`es the matching `PT*` / `P1D` message. |

Published pulses are sent with `Durable = false` and a `TimeToLive` equal to
the pulse interval, so missed pulses don't stack up in queues.

### End-to-end host wiring

The complete wiring (Quartz + MassTransit + `AddPeriodicJobScheduler`) is
shown by `Cogito.MassTransit.Scheduler.Sample1`:

```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(s =>
    {
        // Quartz must be available — any provider works.
        s.AddQuartz(c => c.UseInMemoryStore());
        s.AddQuartzHostedService();

        // Cogito.MassTransit.Scheduler
        s.AddPeriodicJobScheduler();

        s.AddMassTransit(c =>
        {
            c.AddPublishMessageScheduler();
            c.AddQuartzConsumers();

            c.AddConsumer<PeriodicJobConsumer>();

            c.UsingInMemory((ctx, cfg) =>
            {
                cfg.UsePublishMessageScheduler();
                cfg.ConfigureEndpoints(ctx);
            });
        });
    })
    .ConfigureLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace))
    .Build()
    .RunAsync();
```

Combined with a simple consumer:

```csharp
public class PeriodicJobConsumer : IConsumer<PT1M>
{
    readonly ILogger _logger;

    public PeriodicJobConsumer(ILogger<PeriodicJobConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<PT1M> context)
    {
        _logger.LogInformation("Received PT1M");
        return Task.CompletedTask;
    }
}
```

Run the sample:

```pwsh
dotnet run --project src\Cogito.MassTransit.Scheduler.Sample1
```

---

## Cogito.MassTransit.Extensions

Helpers built on top of MassTransit's saga state-machine
(`MassTransitStateMachine<T>`) support. The package exposes two
complementary feature sets:

1. **Request tokens** — capture everything needed to reply to an inbound
   request (`MessageId`, `RequestId`, `CorrelationId`, `ConversationId`,
   `ResponseAddress`, `FaultAddress`, plus the original message) into a
   serializable saga property.
2. **`RespondTo` / `FaultedTo` activities** — later in the saga lifecycle,
   send a normal response or a `FaultEvent<TRequest>` to the captured
   addresses with the correct correlation headers.

Namespace: `Cogito.MassTransit`

### Request tokens

```csharp
public interface IRequestToken<TRequest>
{
    TRequest Request          { get; }
    Guid     MessageId        { get; }
    Guid     RequestId        { get; }
    Guid?    CorrelationId    { get; }
    Guid?    ConversationId   { get; }
    Uri      ResponseAddress  { get; }
    Uri      FaultAddress     { get; }
}

// Default implementation that's both readable and writable; safe to store on a saga.
public record RequestToken<TMessage> : IRequestToken<TMessage>, IRequestTokenSetter<TMessage> { ... }
```

`IRequestTokenSetter<TRequest>` is the write-side companion used by the
capture helpers — implement it (or use the supplied `RequestToken<T>`)
on whatever property you persist on your saga state.

### Capturing a request

`RespondToExtensions` provides several ways to capture an incoming
request from inside a state machine:

```csharp
using Cogito.MassTransit;

// 1. Capture into a brand-new RequestToken<TMessage>:
var token = context.CaptureRequestToken();

// 2. Capture into a custom token type (must implement IRequestTokenSetter<TMessage>
//    and have a parameterless constructor):
var custom = context.CaptureRequestToken<MySaga, SubmitOrder, MyToken>();

// 3. Capture into an existing IRequestTokenSetter<TMessage> (e.g. the saga itself):
context.CaptureRequestToken(saga.PendingRequest);

// 4. Fluent capture inside an EventActivityBinder (sync callback):
Initially(
    When(SubmitOrder)
        .CaptureRequest((ctx, token) => ctx.Saga.PendingRequest = token)
        .TransitionTo(Processing));

// 5. Fluent capture with an awaited callback (e.g. persisting the token to an external store):
Initially(
    When(SubmitOrder)
        .CaptureRequestAsync(async (ctx, token) => await tokenStore.SaveAsync(token))
        .TransitionTo(Processing));
```

The fluent `CaptureRequest` / `CaptureRequestAsync` overloads are
implemented as a `CaptureRequestActivity<...>` and show up in saga
probes / visualizations under the scope name `"captureRequestToken"`.

### `RespondTo` — sending the success response

Both sync and async overloads are available; either return the response
message directly or via a factory:

```csharp
During(Processing,
    When(OrderCompleted)
        .RespondTo(
            requestTokenFactory: ctx => ctx.Saga.PendingRequest,
            messageFactory:     ctx => new OrderAccepted { OrderId = ctx.Saga.CorrelationId })
        .Finalize());

// Async variant — useful when the response or token has to be looked up.
.RespondToAsync<MySaga, OrderCompleted, SubmitOrder, OrderAccepted>(
    requestTokenFactory: async ctx => await store.LoadAsync(ctx.Saga.CorrelationId),
    messageFactory:     async ctx => await BuildResponseAsync(ctx),
    contextCallback:    ctx => ctx.TimeToLive = TimeSpan.FromMinutes(5))
```

Under the hood `RespondToActivity` resolves the send endpoint for
`token.ResponseAddress` and copies `RequestId`, `CorrelationId`, and
`ConversationId` from the captured token onto the outgoing message so
the original requester correlates the reply correctly.

### `FaultedTo` — sending a fault response

`FaultedToExtensions` mirrors `RespondTo` but emits a
`MassTransit.Events.FaultEvent<TRequest>` directed at the captured
`FaultAddress` (falling back to `ResponseAddress` when none was supplied):

```csharp
During(Processing,
    When(OrderRejected)
        .FaultedTo(
            requestTokenFactory: ctx => ctx.Saga.PendingRequest,
            exceptionFactory:    ctx => new InvalidOperationException("Order rejected."))
        .Finalize());

// Async variant
.FaultedToAsync(
    requestTokenFactory: async ctx => await store.LoadAsync(ctx.Saga.CorrelationId),
    exceptionFactory:    async ctx => await BuildExceptionAsync(ctx));
```

### Available delegates

| Delegate                                                  | Purpose                                                     |
| --------------------------------------------------------- | ----------------------------------------------------------- |
| `RequestTokenFactory<TSaga, TMessage, TRequest>`          | Synchronously resolve an `IRequestToken<TRequest>`.         |
| `RequestTokenFactory<TSaga, TRequest>`                    | Same, for `SagaConsumeContext<TSaga>`.                      |
| `AsyncRequestTokenFactory<TSaga, TMessage, TRequest>`     | Async resolution (e.g. database lookup).                    |
| `AsyncRequestTokenFactory<TSaga, TRequest>`               | Async resolution from `SagaConsumeContext<TSaga>`.          |
| `ExceptionFactory<TSaga, TMessage, TRequest>`             | Build an `Exception` for `FaultedTo`.                       |
| `AsyncExceptionFactory<TSaga, TMessage, TRequest>`        | Async exception construction.                               |

---

## Building

### Prerequisites

- Visual Studio 2022 / 2026 or the .NET SDK pinned in [`global.json`](global.json).
- The .NET Framework 4.7.2 targeting pack (for the `net472` target).

### Command line

```pwsh
dotnet restore Cogito.MassTransit.sln
dotnet build   Cogito.MassTransit.sln -c Release
dotnet test    Cogito.MassTransit.sln -c Release
```

### Versioning & packaging

- Versioning is driven by [`GitVersion.yml`](GitVersion.yml).
- NuGet packing is performed by [`src/dist-nuget/dist-nuget.csproj`](src/dist-nuget/dist-nuget.csproj).
- Test packaging is performed by [`src/dist-tests/dist-tests.csproj`](src/dist-tests/dist-tests.csproj).
- CI is defined in [`.github/workflows/Cogito.MassTransit.yml`](.github/workflows/Cogito.MassTransit.yml).

---

## License

See [`LICENSE`](LICENSE).
