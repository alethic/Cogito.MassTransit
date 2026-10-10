# Cogito.MassTransit.Scheduler.Hangfire

Keeps the schedule of Cogito.MassTransit.Scheduler's periodic messages as Hangfire recurring jobs.

## Install

```shell
dotnet add package Cogito.MassTransit.Scheduler.Hangfire
```

## Use

Configure Hangfire, and select it for the periodic job scheduler:

```csharp
services.AddHangfire(c => ...);
services.AddHangfireServer();
services.AddPeriodicJobScheduler(c => c.UseHangfire());
```

On starting, each node adds or updates a recurring job per interval, with an ID such as
`Cogito.MassTransit.Scheduler.Periodic.PT5M` and a cron expression in UTC. Recurring jobs are
identified by ID, so each is stored once, and fires once across the nodes. A Hangfire server must
run on at least one node.

Hangfire does not report when a recurring job was due. The time a message was scheduled for is
taken as the start of the interval in which Hangfire created its job, which is exact because every
interval is aligned to midnight UTC. A failed job is not retried, since its message would be stale
by then.

## License

MIT.
