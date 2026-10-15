# Cogito.MassTransit.EntityFrameworkCore

Keeps MassTransit saga instances in Entity Framework Core, registered from an attribute on the saga.

## Install

```shell
dotnet add package Cogito.MassTransit.EntityFrameworkCore
```

## Use

Apply `[EntityFrameworkSagaRepository]` to a saga, or a saga state machine, registered with
[Cogito.MassTransit.DependencyInjection](https://www.nuget.org/packages/Cogito.MassTransit.DependencyInjection),
naming the `DbContext` that maps the saga:

```csharp
[AddSagaStateMachine(EndpointName = "order-state")]
[EntityFrameworkSagaRepository(typeof(OrderDbContext))]
public class OrderStateMachine : MassTransitStateMachine<OrderState> { ... }
```

The `DbContext` must be registered in the container, for instance with `services.AddDbContext<OrderDbContext>(...)`,
and must map the saga's instance type.

The attribute takes:

- `ConcurrencyMode`: `Pessimistic`, the default, locks an instance when it is loaded; `Optimistic` checks it for
  changes when it is saved.
- `IsolationLevel` of the transaction: unless set, `Serializable` for pessimistic and `ReadCommitted` for optimistic.
- `Database`, whose statement locks pessimistic instances: `SqlServer`, the default, `Postgres`, `MySql`, `Sqlite`
  or `Oracle`.
- `Schema` the saga's table is in, when it is not the `DbContext`'s default.

The repository is registered directly, so it does not depend on whether the attributes are added before or after
`AddMassTransit`.

## License

MIT.
