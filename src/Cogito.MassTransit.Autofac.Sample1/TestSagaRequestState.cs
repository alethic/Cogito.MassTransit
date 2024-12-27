using System;

using Cogito.MassTransit.Automatonymous;

namespace Cogito.MassTransit.Autofac.Sample1
{

    public class TestSagaRequestState : IRequestToken<TestSagaRequest>, IRequestTokenSetter<TestSagaRequest>
    {

        public TestSagaRequest Request { get; set; }

        public Guid MessageId { get; set; }

        public Guid RequestId { get; set; }

        public Guid? CorrelationId { get; set; }

        public Guid? ConversationId { get; set; }

        public Uri ResponseAddress { get; set; }

        public Uri FaultAddress { get; set; }

    }

}