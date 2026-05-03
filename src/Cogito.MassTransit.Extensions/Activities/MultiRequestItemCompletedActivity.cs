using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Executed when an item from a multi-request is completed.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class MultiRequestItemCompletedActivity<TSaga, TState, TRequest, TResponse> : IStateMachineActivity<TSaga, TResponse>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TSaga, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemCompletedActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemCompleted");
        }

        public Task Execute(BehaviorContext<TSaga, TResponse> context, IBehavior<TSaga, TResponse> next)
        {
            request.Accessor.SetCompleted(context, request.GetItem(context, context.RequestId.Value), context.Message);
            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, TResponse, TException> context, IBehavior<TSaga, TResponse> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
