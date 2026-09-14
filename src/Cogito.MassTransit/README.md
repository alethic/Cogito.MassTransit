# Cogito.MassTransit

Endpoint address helpers for [MassTransit](https://masstransit.io/), and a set of named schedule
intervals.

## Why

Building an endpoint URI by string concatenation is easy to get subtly wrong, and the rules differ by
transport. These helpers resolve an endpoint's address the way the bus would.

## Install

```shell
dotnet add package Cogito.MassTransit
```

## Use

```csharp
var uri = bus.GetAbsoluteEndpointUri("order-submitted");
```

The interval types (`PT1M`, `PT5M`, `PT1H`, `P1D` and the rest) name the schedules used by
`Cogito.MassTransit.Scheduler`, so a recurring job states its period in its type rather than in a
cron string.

## License

MIT.
