using System;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Events;

using GreenPipes;

using MassTransit;
using MassTransit.Metadata;

namespace Cogito.MassTransit.Automatonymous.Activities
{

    /// <summary>
    /// Provides the base implementation of a MultiRequest activity.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class MultiRequestActivityImpl<TInstance, TState, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly IMultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        protected MultiRequestActivityImpl(IMultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Sends an individual request to the specified service address and records the results.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="consumeContext"></param>
        /// <param name="requestMessage"></param>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        protected async Task SendRequest(BehaviorContext<TInstance> context, ConsumeContext consumeContext, TRequest requestMessage, Uri serviceAddress)
        {
            var pipe = new SendRequestPipe(consumeContext.ReceiveContext.InputAddress);

            if (serviceAddress != null)
            {
                // specific service address specfied, send
                var endpoint = await consumeContext.GetSendEndpoint(serviceAddress).ConfigureAwait(false);
                await endpoint.Send(requestMessage, pipe).ConfigureAwait(false);
            }
            else
            {
                // no service address specified, publish
                await consumeContext.Publish(requestMessage, pipe).ConfigureAwait(false);
            }

            // add new state item
            request.Accessor.Insert(context, pipe.RequestId);

            if (request.Settings.Timeout > TimeSpan.Zero)
            {
                var now = DateTime.UtcNow;
                var expirationTime = now + request.Settings.Timeout;

                var message = new TimeoutExpired<TRequest>(now, expirationTime, context.Instance.CorrelationId, pipe.RequestId);

                if (consumeContext.TryGetPayload<MessageSchedulerContext>(out var schedulerContext))
                    await schedulerContext.ScheduleSend(expirationTime, message).ConfigureAwait(false);
                else
                    throw new ConfigurationException("A request timeout was specified but no message scheduler was specified or available.");
            }
        }

        public virtual void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("request");
            scope.Add("requestType", TypeMetadataCache<TRequest>.ShortName);
            scope.Add("responseType", TypeMetadataCache<TResponse>.ShortName);
            scope.Set(request.Settings);
        }

        /// <summary>
        /// Handles the sending of a request to the endpoint specified
        /// </summary>
        class SendRequestPipe : IPipe<SendContext<TRequest>>
        {

            readonly Uri responseAddress;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="responseAddress"></param>
            public SendRequestPipe(Uri responseAddress)
            {
                this.responseAddress = responseAddress ?? throw new ArgumentNullException(nameof(responseAddress));
                RequestId = NewId.NextGuid();
            }

            public Guid RequestId { get; }

            void IProbeSite.Probe(ProbeContext context)
            {

            }

            Task IPipe<SendContext<TRequest>>.Send(SendContext<TRequest> context)
            {
                context.RequestId = RequestId;
                context.ResponseAddress = responseAddress;
                return Task.CompletedTask;
            }

        }

        /// <summary>
        /// Internal <see cref="RequestTimeoutExpired{TRequest}"/> implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class TimeoutExpired<T> : RequestTimeoutExpired<T>
            where T : class
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="timestamp"></param>
            /// <param name="expirationTime"></param>
            /// <param name="correlationId"></param>
            /// <param name="requestId"></param>
            public TimeoutExpired(DateTime timestamp, DateTime expirationTime, Guid correlationId, Guid requestId)
            {
                Timestamp = timestamp;
                ExpirationTime = expirationTime;
                CorrelationId = correlationId;
                RequestId = requestId;
            }

            public DateTime Timestamp { get; }

            public DateTime ExpirationTime { get; }

            public Guid CorrelationId { get; }

            public Guid RequestId { get; }

        }

    }

}
