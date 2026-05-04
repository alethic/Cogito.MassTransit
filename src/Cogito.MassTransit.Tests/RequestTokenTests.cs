using System;

namespace Cogito.MassTransit.Tests
{

    public class RequestTokenTests
    {

        [Fact]
        public void Default_instance_has_all_properties_unset()
        {
            var token = new RequestToken<string>();

            Assert.Null(token.Request);
            Assert.Null(token.MessageId);
            Assert.Null(token.RequestId);
            Assert.Null(token.CorrelationId);
            Assert.Null(token.ConversationId);
            Assert.Null(token.ResponseAddress);
            Assert.Null(token.FaultAddress);
        }

        [Fact]
        public void Properties_round_trip_through_setters()
        {
            var messageId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var responseAddress = new Uri("loopback://localhost/response");
            var faultAddress = new Uri("loopback://localhost/fault");

            var token = new RequestToken<string>
            {
                Request = "hello",
                MessageId = messageId,
                RequestId = requestId,
                CorrelationId = correlationId,
                ConversationId = conversationId,
                ResponseAddress = responseAddress,
                FaultAddress = faultAddress,
            };

            Assert.Equal("hello", token.Request);
            Assert.Equal(messageId, token.MessageId);
            Assert.Equal(requestId, token.RequestId);
            Assert.Equal(correlationId, token.CorrelationId);
            Assert.Equal(conversationId, token.ConversationId);
            Assert.Equal(responseAddress, token.ResponseAddress);
            Assert.Equal(faultAddress, token.FaultAddress);
        }

        [Fact]
        public void Records_with_same_values_are_equal()
        {
            var messageId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var a = new RequestToken<string>
            {
                Request = "x",
                MessageId = messageId,
                RequestId = requestId,
            };

            var b = new RequestToken<string>
            {
                Request = "x",
                MessageId = messageId,
                RequestId = requestId,
            };

            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Records_with_different_values_are_not_equal()
        {
            var a = new RequestToken<string> { Request = "x" };
            var b = new RequestToken<string> { Request = "y" };

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Implements_IRequestToken_and_IRequestTokenSetter()
        {
            var token = new RequestToken<string>();

            Assert.IsAssignableFrom<IRequestToken<string>>(token);
            Assert.IsAssignableFrom<IRequestTokenSetter<string>>(token);
        }

    }

}
