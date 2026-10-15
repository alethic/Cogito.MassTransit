# Cogito.MassTransit.EntityFrameworkCore.DependencyInjection

Keeps MassTransit saga instances in Entity Framework Core, registered from an attribute on the saga.

## Install

```shell
dotnet add package Cogito.MassTransit.EntityFrameworkCore.DependencyInjection
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
- `LockStatementProvider`, MassTransit's `ILockStatementProvider` whose statement locks pessimistic instances when
  they are loaded. Unless set, it is MassTransit's default, for SQL Server. On another database, name its provider:

  ```csharp
  [EntityFrameworkSagaRepository(typeof(OrderDbContext), LockStatementProvider = typeof(PostgresLockStatementProvider))]
  ```

  The provider is created with a constructor whose parameters are all optional, as MassTransit's providers have.
  For a schema other than the `DbContext`'s default, derive a provider that passes it to the base constructor.

The repository is registered directly, so it does not depend on whether the attributes are added before or after
`AddMassTransit`.

## License

MIT.
