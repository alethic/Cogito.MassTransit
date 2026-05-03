using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    class CaptureRequestActivity<TInstance, TData, TToken> : IStateMachineActivity<TInstance, TData>
        where TInstance : class, SagaStateMachineInstance
        where TData : class
        where TToken : IRequestTokenSetter<TData>, new()
    {

        readonly Func<BehaviorContext<TInstance, TData>, TToken, Task> captured;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="captured"></param>
        public CaptureRequestActivity(Func<BehaviorContext<TInstance, TData>, TToken, Task> captured)
        {
            this.captured = captured ?? throw new ArgumentNullException(nameof(captured));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("captureRequestToken");
        }

        public async Task Execute(BehaviorContext<TInstance, TData> context, IBehavior<TInstance, TData> next)
        {
            var token = new TToken();
            context.CaptureRequestToken(token);

            await captured(context, token).ConfigureAwait(false);

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, TData, TException> context, IBehavior<TInstance, TData> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
