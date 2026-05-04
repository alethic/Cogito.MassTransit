using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Executed when an item from a multi-request faults.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class MultiRequestItemFaultedActivity<TSaga, TState, TRequest, TResponse> : IStateMachineActivity<TSaga, Fault<TRequest>>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TSaga, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemFaultedActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemFaulted");
        }

        /// <inheritdoc/>
        public Task Execute(BehaviorContext<TSaga, Fault<TRequest>> context, IBehavior<TSaga, Fault<TRequest>> next)
        {
            request.Accessor.SetFaulted(context, request.GetItem(context, (Guid)context.RequestId), context.Message);
            return next.Execute(context);
        }

        /// <inheritdoc/>
        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, Fault<TRequest>, TException> context, IBehavior<TSaga, Fault<TRequest>> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
