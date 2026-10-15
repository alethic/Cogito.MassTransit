# Cogito.MassTransit.DependencyInjection

Registers MassTransit consumers, sagas and activities from attributes on the types themselves, onto any bus.

## Why

`AddMassTransit` wants every consumer and saga listed inside its configuration lambda, so the place a bus is
set up has to know every type that runs on it. With attributes, each type says which bus it runs on and how its
endpoint is named, and a host only adds the assemblies it wants. The types are registered through MassTransit's
own registration API, so they behave exactly as if they had been added in the lambda.

## Install

```shell
dotnet add package Cogito.MassTransit.DependencyInjection
```

## Use

Apply the attributes:

```csharp
[AddConsumer(EndpointName = "orders")]
public class SubmitOrderConsumer : IConsumer<SubmitOrder> { ... }

[AddSagaStateMachine(EndpointName = "order-state")]
[InMemorySagaRepository]
public class OrderStateMachine : MassTransitStateMachine<OrderState> { ... }
```

Then add the bus, and the assemblies whose types to register, with `AddFromAttributes` from
[Cogito.DependencyInjection](https://www.nuget.org/packages/Cogito.DependencyInjection):

```csharp
services.AddMassTransit(x => x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context)));
services.AddFromAttributes(typeof(Program).Assembly);
```

The types join the endpoints `ConfigureEndpoints` creates. It does not matter whether `AddFromAttributes` comes
before or after `AddMassTransit`, except for sagas without a repository attribute (below).

The registrations are made on the `IServiceCollection`, so an Autofac host that populates from it gets them too.

### Attributes

| Attribute | Registers |
|---|---|
| `[AddConsumer]` | A consumer. |
| `[AddSagaStateMachine]` | A saga state machine. |
| `[AddSaga]` | A saga class. |
| `[AddActivity]` | A routing slip activity, which executes and compensates. |
| `[AddExecuteActivity]` | An execute-only routing slip activity. |

Each takes:

- `Bus`, the bus to register onto. It is `IBus` unless set to the interface given to `AddMassTransit<TBus>`.
  Apply the attribute more than once to register a type onto more than one bus.
- `Definition`, a MassTransit definition for the type, such as a `ConsumerDefinition<T>`.
- `ExcludeFromConfigureEndpoints`, which leaves the type off the endpoints `ConfigureEndpoints` creates.

All but `[AddActivity]` also take the endpoint's `EndpointName`, `PrefetchCount`, `ConcurrentMessageLimit`,
`IsTemporary`, `InstanceId` and `ConfigureConsumeTopology`. `[AddActivity]` takes `ExecuteEndpointName` and
`CompensateEndpointName`; configure anything more about its endpoints with a definition. Endpoint properties that
are not set fall to the definition, or to the bus's endpoint name formatter.

```csharp
public interface IBackOfficeBus : IBus { }

[AddConsumer(Bus = typeof(IBackOfficeBus), EndpointName = "reports", ConcurrentMessageLimit = 4)]
public class ReportConsumer : IConsumer<RunReport> { ... }
```

### Saga repositories

A saga needs a repository. Apply a repository attribute to the saga, or to its state machine:

- `[InMemorySagaRepository]` keeps instances in memory.
- `[EntityFrameworkSagaRepository]`, from
  [Cogito.MassTransit.EntityFrameworkCore](https://www.nuget.org/packages/Cogito.MassTransit.EntityFrameworkCore),
  keeps them in Entity Framework Core.

Without one, the saga takes the repository provider set on the bus, such as `SetInMemorySagaRepositoryProvider()`,
but only if `AddFromAttributes` comes before `AddMassTransit`: MassTransit assigns the provider's repositories as
`AddMassTransit` completes. A saga registered from attributes that has no repository fails the host's startup,
rather than its first message.

## License

MIT.
