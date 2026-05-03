using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions.Activities;

using MassTransit;

namespace Cogito.MassTransit.Extensions
{

    public static class CaptureRequestExtensions
    {

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static RequestToken<TMessage>? CaptureRequestToken<TSaga, TMessage>(this BehaviorContext<TSaga, TMessage> context)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            return CaptureRequestToken<TSaga, TMessage, RequestToken<TMessage>>(context);
        }

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TToken? CaptureRequestToken<TSaga, TMessage, TToken>(this BehaviorContext<TSaga, TMessage> context)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TToken : IRequestTokenSetter<TMessage>, new()
        {
            if (context.RequestId != null)
            {
                var token = new TToken();
                CaptureRequestToken(context, token);
                return token;
            } 
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static void CaptureRequestToken<TSaga, TMessage>(this BehaviorContext<TSaga, TMessage> context, IRequestTokenSetter<TMessage> token)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (context.RequestId != null)
            {
                token.Request = context.Message;
                token.MessageId = context.MessageId;
                token.RequestId = context.RequestId;
                token.ConversationId = context.ConversationId;
                token.CorrelationId = context.CorrelationId;
                token.FaultAddress = context.FaultAddress;
                token.ResponseAddress = context.ResponseAddress;
            }
        }

        /// <summary>
        /// Captures the incoming request into a <see cref="RequestToken{TRequest}"/>.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="captured"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> CaptureRequest<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, Action<BehaviorContext<TSaga, TMessage>, RequestToken<TMessage>> captured)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (captured == null)
                throw new ArgumentNullException(nameof(captured));

            return source.Add(new CaptureRequestActivity<TSaga, TMessage, RequestToken<TMessage>>((context, token) =>
            {
                captured(context, token);
                return Task.CompletedTask;
            }));
        }

        /// <summary>
        /// Captures the incoming request into a <see cref="RequestToken{TRequest}"/>, awaiting the supplied callback.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="captured"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> CaptureRequestAsync<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, Func<BehaviorContext<TSaga, TMessage>, RequestToken<TMessage>, Task> captured)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            if (captured == null)
                throw new ArgumentNullException(nameof(captured));

            return source.Add(new CaptureRequestActivity<TSaga, TMessage, RequestToken<TMessage>>(captured));
        }

    }

}
