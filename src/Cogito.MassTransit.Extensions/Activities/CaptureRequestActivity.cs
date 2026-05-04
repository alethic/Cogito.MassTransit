using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    class CaptureRequestActivity<TSaga, TMessage, TToken> : IStateMachineActivity<TSaga, TMessage>
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
        where TToken : IRequestTokenSetter<TMessage>, new()
    {

        readonly Func<BehaviorContext<TSaga, TMessage>, TToken, Task> captured;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="captured"></param>
        public CaptureRequestActivity(Func<BehaviorContext<TSaga, TMessage>, TToken, Task> captured)
        {
            this.captured = captured ?? throw new ArgumentNullException(nameof(captured));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public void Probe(ProbeContext context)
        {
            context.CreateScope("captureRequest");
        }

        /// <inheritdoc/>
        public async Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
        {
            var token = new TToken();
            context.CaptureRequestToken(token);

            await captured(context, token).ConfigureAwait(false);

            await next.Execute(context).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, TMessage, TException> context, IBehavior<TSaga, TMessage> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
