# Cogito.MassTransit.Scheduler

Publishes a message on a fixed period, so recurring work is a message rather than a timer.

## Why

Recurring work usually starts as a timer in one process, which then has to be prevented from running
in the other instances. Publishing on a schedule instead means the work is an ordinary consumer:
competing consumers handle it once, and it retries, logs and scales like everything else on the bus.

## Install

This package holds the schedule and the messages. Install it with the package for the scheduler that
keeps the schedule:

```shell
dotnet add package Cogito.MassTransit.Scheduler.Quartz
```

or

```shell
dotnet add package Cogito.MassTransit.Scheduler.Hangfire
```

## Use

Select the scheduler when adding the periodic job scheduler:

```csharp
services.AddPeriodicJobScheduler(c => c.UseQuartz());
```

or

```csharp
services.AddPeriodicJobScheduler(c => c.UseHangfire());
```

A message is then published on each of the intervals in `PeriodicInterval`: `PT1M`, `PT5M`,
`PT15M`, `PT30M`, `PT1H`, `PT2H`, `PT6H`, `PT12H` and `P1D`, aligned to midnight UTC. Consume the
message type for the interval you need:

```csharp
public class Cleanup : IConsumer<PT15M>
{
    public Task Consume(ConsumeContext<PT15M> context) => ...;
}
```

Each message carries its interval and its fire times. It is published non-durably, and expires after
its interval, by when the next is due.

The periodic messages are published directly, not through MassTransit's recurring message scheduling.
That fixes a message's time to live when the schedule is created rather than each time it fires, and
cannot carry the fire times in the message.

## Upgrading from 2.x

The scheduler no longer defaults to Quartz. Add `Cogito.MassTransit.Scheduler.Quartz`, and replace
`AddPeriodicJobScheduler()` with `AddPeriodicJobScheduler(c => c.UseQuartz())`.

## License

MIT.
