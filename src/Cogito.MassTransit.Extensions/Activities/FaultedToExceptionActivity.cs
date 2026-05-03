using System;
using System.Threading.Tasks;

using MassTransit;
using MassTransit.Events;
using MassTransit.Metadata;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Activity invoked from inside a <c>Catch</c> handler that emits a fault to the captured request's response or fault address,
    /// with typed access to both the caught exception and the underlying <see cref="BehaviorExceptionContext{TSaga, TMessage, TException}"/>.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    class FaultedToExceptionActivity<TSaga, TMessage, TException, TRequest> : IStateMachineActivity<TSaga, TMessage>
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
        where TException : Exception
    {

        readonly AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory;
        readonly AsyncCatchExceptionFactory<TSaga, TMessage, TException, TRequest> exceptionFactory;
        readonly Action<SendContext<FaultEvent<TRequest>>>? contextCallback;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        public FaultedToExceptionActivity(AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncCatchExceptionFactory<TSaga, TMessage, TException, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback)
        {
            this.requestTokenFactory = requestTokenFactory ?? throw new ArgumentNullException(nameof(requestTokenFactory));
            this.exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            this.contextCallback = contextCallback;
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("faultedTo");
        }

        public async Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
        {
            // ExceptionActivityBinder dispatches a BehaviorExceptionContext to the registered IStateMachineActivity<TSaga, TMessage>;
            // this cast is the single, contained point at which we recover the typed exception view.
            if (context is not BehaviorExceptionContext<TSaga, TMessage, TException> exceptionContext)
            {
                LogContext.Warning?.Log(
                    "FaultedTo (exception) skipped on saga {SagaType} for request {RequestType}: activity invoked outside of a Catch<{ExceptionType}> scope.",
                    typeof(TSaga).Name,
                    typeof(TRequest).Name,
                    typeof(TException).Name);

                await next.Execute(context).ConfigureAwait(false);
                return;
            }

            var requestToken = await requestTokenFactory.Invoke(exceptionContext);
            if (requestToken is null)
            {
                LogContext.Debug?.Log(
                    "FaultedTo (exception) skipped on saga {SagaType} for request {RequestType}: request token factory returned null (no captured request available).",
                    typeof(TSaga).Name,
                    typeof(TRequest).Name);
            }
            else
            {
                var address = requestToken.FaultAddress ?? requestToken.ResponseAddress;
                if (address is null)
                {
                    LogContext.Debug?.Log(
                        "FaultedTo (exception) skipped on saga {SagaType} for request {RequestType}: captured request token has neither FaultAddress nor ResponseAddress (RequestId={RequestId}).",
                        typeof(TSaga).Name,
                        typeof(TRequest).Name,
                        requestToken.RequestId);
                }
                else
                {
                    var sendEndpoint = await context.GetSendEndpoint(address);
                    var exception = await exceptionFactory.Invoke(exceptionContext);
                    var fault = new FaultEvent<TRequest>(requestToken.Request, requestToken.MessageId, HostMetadataCache.Host, exception, new string[0]);

                    await sendEndpoint.Send(fault, ctx =>
                    {
                        ctx.CorrelationId = requestToken.CorrelationId;
                        ctx.ConversationId = requestToken.ConversationId;
                        ctx.RequestId = requestToken.RequestId;
                        contextCallback?.Invoke(ctx);
                    });
                }
            }

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TOtherException>(BehaviorExceptionContext<TSaga, TMessage, TOtherException> context, IBehavior<TSaga, TMessage> next) where TOtherException : Exception
        {
            return next.Faulted(context);
        }

    }

}
