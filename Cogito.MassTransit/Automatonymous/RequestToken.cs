using System;

namespace Cogito.MassTransit.Automatonymous
{

    /// <summary>
    /// Describes a serialized form of a request.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public class RequestToken<TRequest> : IRequestToken<TRequest>, IRequestTokenSetter<TRequest>
    {

        public TRequest Request { get; set; }

        public Guid MessageId { get; set; }

        public Guid RequestId { get; set; }

        public Guid? CorrelationId { get; set; }

        public Guid? ConversationId { get; set; }

        public Uri ResponseAddress { get; set; }

        public Uri FaultAddress { get; set; }

    }

}
