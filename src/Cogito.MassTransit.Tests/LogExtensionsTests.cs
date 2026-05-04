using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cogito.MassTransit.Tests
{

    public class LogExtensionsTests
    {

        public class Ping
        {
            public Guid CorrelationId { get; set; }
        }

        public class LogSaga : SagaStateMachineInstance
        {
            public Guid CorrelationId { get; set; }
            public string? CurrentState { get; set; }
        }

        public class LogStateMachine : MassTransitStateMachine<LogSaga>
        {

            public LogStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Event(() => Started, x => x.CorrelateById(m => m.Message.CorrelationId));

                Initially(
                    When(Started)
                        .Log(LogLevel.Information, "static message")
                        .Log(LogLevel.Information, ctx => $"dynamic {ctx.Message.CorrelationId}")
                        .Log((logger, ctx) => logger.LogWarning("action invoked"))
                        .LogAsync((logger, ctx) =>
                        {
                            logger.LogError("async invoked");
                            return Task.CompletedTask;
                        })
                        .TransitionTo(Active));
            }

            public State Active { get; private set; } = null!;
            public Event<Ping> Started { get; private set; } = null!;

        }

        class CapturingLoggerProvider : ILoggerProvider
        {

            public List<(LogLevel Level, string Message)> Entries { get; } = new();

            public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

            public void Dispose() { }

            class CapturingLogger : ILogger
            {

                readonly CapturingLoggerProvider _provider;

                public CapturingLogger(CapturingLoggerProvider provider)
                {
                    _provider = provider;
                }

                public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
                public bool IsEnabled(LogLevel logLevel) => true;
                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                    lock (_provider.Entries)
                        _provider.Entries.Add((logLevel, formatter(state, exception)));
                }

            }

        }

        [Fact]
        public async Task Log_overloads_dispatch_to_resolved_logger()
        {
            var capture = new CapturingLoggerProvider();

            await using var provider = new ServiceCollection()
                .AddLogging(b =>
                {
                    b.SetMinimumLevel(LogLevel.Trace);
                    b.AddProvider(capture);
                })
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<LogStateMachine, LogSaga>().InMemoryRepository();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var sagaId = Guid.NewGuid();
            await harness.Bus.Publish(new Ping { CorrelationId = sagaId });

            var sagaHarness = harness.GetSagaStateMachineHarness<LogStateMachine, LogSaga>();
            Assert.NotNull(await sagaHarness.Exists(sagaId, x => x.Active));

            lock (capture.Entries)
            {
                Assert.Contains(capture.Entries, e => e.Level == LogLevel.Information && e.Message == "static message");
                Assert.Contains(capture.Entries, e => e.Level == LogLevel.Information && e.Message == $"dynamic {sagaId}");
                Assert.Contains(capture.Entries, e => e.Level == LogLevel.Warning && e.Message == "action invoked");
                Assert.Contains(capture.Entries, e => e.Level == LogLevel.Error && e.Message == "async invoked");
            }
        }

        [Fact]
        public void Log_messageFactory_throws_when_factory_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => LogExtensions.Log<LogSaga, Ping>(null!, LogLevel.Information, (Func<BehaviorContext<LogSaga, Ping>, string>)null!));
        }

        [Fact]
        public void Log_action_throws_when_action_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => LogExtensions.Log<LogSaga, Ping>(null!, (Action<ILogger, BehaviorContext<LogSaga, Ping>>)null!));
        }

        [Fact]
        public void LogAsync_throws_when_action_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => LogExtensions.LogAsync<LogSaga, Ping>(null!, (Func<ILogger, BehaviorContext<LogSaga, Ping>, Task>)null!));
        }

        [Fact]
        public void Log_with_logger_throws_when_logger_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => LogExtensions.Log<LogSaga, Ping>(null!, (ILogger)null!, (l, c) => { }));
        }

        [Fact]
        public void Log_with_logger_throws_when_action_is_null()
        {
            using var factory = LoggerFactory.Create(b => { });
            var logger = factory.CreateLogger("x");
            Assert.Throws<ArgumentNullException>(() => LogExtensions.Log<LogSaga, Ping>(null!, logger, (Action<ILogger, BehaviorContext<LogSaga, Ping>>)null!));
        }

        [Fact]
        public void LogAsync_with_logger_throws_when_logger_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => LogExtensions.LogAsync<LogSaga, Ping>(null!, (ILogger)null!, (l, c) => Task.CompletedTask));
        }

        [Fact]
        public void LogAsync_with_logger_throws_when_action_is_null()
        {
            using var factory = LoggerFactory.Create(b => { });
            var logger = factory.CreateLogger("x");
            Assert.Throws<ArgumentNullException>(() => LogExtensions.LogAsync<LogSaga, Ping>(null!, logger, (Func<ILogger, BehaviorContext<LogSaga, Ping>, Task>)null!));
        }

    }

}
