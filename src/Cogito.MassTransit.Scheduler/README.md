# Cogito.MassTransit.Scheduler

Publishes a message on a fixed period, so recurring work is a message rather than a timer.

## Why

Recurring work usually starts as a timer in one process, which then has to be prevented from running
in the other instances. Publishing on a schedule instead means the work is an ordinary consumer:
competing consumers handle it once, and it retries, logs and scales like everything else on the bus.

## Install

```shell
dotnet add package Cogito.MassTransit.Scheduler
```

## Use

```csharp
services.AddPeriodicJobScheduler();
```

Declare a job against one of the named intervals from `Cogito.MassTransit` — `PT5M`, `PT1H`, `P1D`
and so on — and a message is published on that period for a consumer to pick up.

Built on Quartz for the triggering; `PeriodicScheduler` and `PeriodicSchedulerJob` are the pieces if
you need to drive it directly.

## License

MIT.
