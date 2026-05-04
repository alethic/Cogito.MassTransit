using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Activity that resolves an <see cref="ILogger"/> and invokes a user supplied callback inside a logging scope
    /// populated with contextual information about the current saga instance and message.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    class LogActivity<TSaga, TMessage> : IStateMachineActivity<TSaga, TMessage>
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
    {

        readonly Func<BehaviorContext<TSaga, TMessage>, ILogger?> loggerFactory;
        readonly Func<ILogger, BehaviorContext<TSaga, TMessage>, Task> action;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="action"></param>
        public LogActivity(Func<BehaviorContext<TSaga, TMessage>, ILogger?> loggerFactory, Func<ILogger, BehaviorContext<TSaga, TMessage>, Task> action)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public void Probe(ProbeContext context)
        {
            context.CreateScope("log");
        }

        /// <inheritdoc/>
        public async Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
        {
            var logger = loggerFactory(context);
            if (logger is null)
            {
                LogContext.Debug?.Log(
                    "Log skipped on saga {SagaType} for message {MessageType}: logger factory returned null.",
                    typeof(TSaga).Name,
                    typeof(TMessage).Name);
            }
            else
            {
                using (BeginSagaScope(logger, context))
                    await action(logger, context).ConfigureAwait(false);
            }

            await next.Execute(context).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, TMessage, TException> context, IBehavior<TSaga, TMessage> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        /// <summary>
        /// Begins a logging scope populated with contextual information drawn from the saga and the incoming message.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        static IDisposable? BeginSagaScope(ILogger logger, BehaviorContext<TSaga, TMessage> context)
        {
            return logger.BeginScope(new Dictionary<string, object?>
            {
                ["SagaType"] = typeof(TSaga).FullName,
                ["SagaId"] = context.Saga.CorrelationId,
                ["MessageType"] = typeof(TMessage).FullName,
                ["MessageId"] = context.MessageId,
                ["CorrelationId"] = context.CorrelationId,
                ["ConversationId"] = context.ConversationId,
                ["RequestId"] = context.RequestId,
            });
        }

    }

}
