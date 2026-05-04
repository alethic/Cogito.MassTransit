using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions.Activities;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cogito.MassTransit.Extensions
{

    /// <summary>
    /// Provides extension methods for logging from inside a saga state machine event activity using an
    /// <see cref="ILogger"/>. The logger is invoked inside a scope populated with contextual information about the
    /// current saga instance and message.
    /// </summary>
    public static class LogExtensions
    {

        /// <summary>
        /// Resolves an <see cref="ILogger"/> for <typeparamref name="TSaga"/> from the consume context's service
        /// provider, falling back to <see cref="NullLogger{TSaga}.Instance"/> when none is available.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        static ILogger ResolveLogger<TSaga, TMessage>(BehaviorContext<TSaga, TMessage> context)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (context.TryGetPayload<IServiceProvider>(out var serviceProvider))
            {
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                if (loggerFactory is not null)
                    return loggerFactory.CreateLogger(typeof(TSaga).FullName ?? typeof(TSaga).Name);
            }

            return NullLogger<TSaga>.Instance;
        }

        /// <summary>
        /// Logs a message using an <see cref="ILogger"/> resolved from the consume context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="logLevel"></param>
        /// <param name="messageFactory"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> Log<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, LogLevel logLevel, Func<BehaviorContext<TSaga, TMessage>, string> messageFactory)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (messageFactory == null)
                throw new ArgumentNullException(nameof(messageFactory));

            return source.Add(new LogActivity<TSaga, TMessage>(
                ResolveLogger,
                (logger, context) =>
                {
                    if (logger.IsEnabled(logLevel))
                        logger.Log(logLevel, messageFactory(context));
                    return Task.CompletedTask;
                }));
        }

        /// <summary>
        /// Logs a message using an <see cref="ILogger"/> resolved from the consume context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> Log<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, LogLevel logLevel, string message)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            return Log(source, logLevel, _ => message);
        }

        /// <summary>
        /// Invokes the supplied callback with an <see cref="ILogger"/> resolved from the consume context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> Log<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, Action<ILogger, BehaviorContext<TSaga, TMessage>> action)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return source.Add(new LogActivity<TSaga, TMessage>(
                ResolveLogger,
                (logger, context) =>
                {
                    action(logger, context);
                    return Task.CompletedTask;
                }));
        }

        /// <summary>
        /// Invokes the supplied asynchronous callback with an <see cref="ILogger"/> resolved from the consume context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> LogAsync<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, Func<ILogger, BehaviorContext<TSaga, TMessage>, Task> action)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return source.Add(new LogActivity<TSaga, TMessage>(ResolveLogger, action));
        }

        /// <summary>
        /// Invokes the supplied callback with the specified <see cref="ILogger"/>.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="logger"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> Log<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, ILogger logger, Action<ILogger, BehaviorContext<TSaga, TMessage>> action)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return source.Add(new LogActivity<TSaga, TMessage>(
                _ => logger,
                (l, context) =>
                {
                    action(l, context);
                    return Task.CompletedTask;
                }));
        }

        /// <summary>
        /// Invokes the supplied asynchronous callback with the specified <see cref="ILogger"/>.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="logger"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> LogAsync<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, ILogger logger, Func<ILogger, BehaviorContext<TSaga, TMessage>, Task> action)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return source.Add(new LogActivity<TSaga, TMessage>(_ => logger, action));
        }

    }

}
