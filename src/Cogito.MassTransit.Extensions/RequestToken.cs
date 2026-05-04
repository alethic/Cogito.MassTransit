using System;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Describes a serialized form of a request.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public record RequestToken<TMessage> : IRequestToken<TMessage>, IRequestTokenSetter<TMessage>
    {

        /// <inheritdoc cref="IRequestToken{TRequest}.Request"/>
        public TMessage? Request { get; set; }

        /// <inheritdoc cref="IRequestToken{TRequest}.MessageId"/>
        public Guid? MessageId { get; set; }

        /// <inheritdoc cref="IRequestToken{TRequest}.RequestId"/>
        public Guid? RequestId { get; set; }

        /// <inheritdoc cref="IRequestToken{TRequest}.CorrelationId"/>
        public Guid? CorrelationId { get; set; }

        /// <inheritdoc cref="IRequestToken{TRequest}.ConversationId"/>
        public Guid? ConversationId { get; set; }

        /// <inheritdoc cref="IRequestToken{TRequest}.ResponseAddress"/>
        public Uri? ResponseAddress { get; set; }

        /// <inheritdoc cref="IRequestToken{TRequest}.FaultAddress"/>
        public Uri? FaultAddress { get; set; }

    }

}
