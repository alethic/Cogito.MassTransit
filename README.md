# Cogito.MassTransit

[![Build](https://github.com/alethic/Cogito.MassTransit/actions/workflows/Cogito.MassTransit.yml/badge.svg)](https://github.com/alethic/Cogito.MassTransit/actions/workflows/Cogito.MassTransit.yml)

Extensions for MassTransit: fan a request out to many handlers from a saga, and publish recurring work on a schedule.

## Packages

**[Cogito.MassTransit](https://www.nuget.org/packages/Cogito.MassTransit)** — Endpoint address helpers for [MassTransit](https://masstransit.io/), and a set of named schedule intervals.

**[Cogito.MassTransit.Extensions](https://www.nuget.org/packages/Cogito.MassTransit.Extensions)** — Fan out a request to many handlers from inside a saga, and wait for all of them.

**[Cogito.MassTransit.Scheduler](https://www.nuget.org/packages/Cogito.MassTransit.Scheduler)** — Publishes a message on a fixed period, so recurring work is a message rather than a timer.

Each package carries its own README with the detail; the links above go to nuget.org.

## Building

```shell
dotnet restore Cogito.MassTransit.slnx
dotnet msbuild -p:Configuration=Release Cogito.MassTransit.dist.msbuildproj
```

Packages are staged into `dist/nuget` and test suites into `dist/tests`; run a suite with
`dotnet test -f <tfm> <path to its assembly>`.

## License

MIT — see [LICENSE](LICENSE).
