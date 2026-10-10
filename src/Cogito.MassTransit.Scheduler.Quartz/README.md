# Cogito.MassTransit.Scheduler.Quartz

Keeps the schedule of Cogito.MassTransit.Scheduler's periodic messages in Quartz.

## Install

```shell
dotnet add package Cogito.MassTransit.Scheduler.Quartz
```

## Use

Configure Quartz, and select it for the periodic job scheduler:

```csharp
services.AddQuartz(q => ...);
services.AddQuartzHostedService();
services.AddPeriodicJobScheduler(c => c.UseQuartz());
```

Quartz's job store decides where the schedule lives. With a clustered persistent store, each periodic
message is published once across the nodes.

Each node keeps the schedule in step every five minutes. It stores a `Periodic` job, in the
`Cogito.MassTransit.Scheduler` group, with a trigger per interval, and resumes any trigger in error.

The package depends on Quartz 3, the version MassTransit.Quartz 8 uses.

## Upgrading from Cogito.MassTransit.Scheduler 2.x

The Quartz jobs moved into this package, so a persistent job store holds them under their old type
names. On starting, the scheduler replaces any stored job of another type, or one the store cannot
load. Upgrade every node in a cluster together: a node still on 2.x cannot run the replaced jobs.

## License

MIT.
