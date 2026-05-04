using System;

using MassTransit;

using NSubstitute;

namespace Cogito.MassTransit.Tests
{

    public class BusExtensionsTests
    {

        static IBus CreateBus(Uri address)
        {
            var bus = Substitute.For<IBus>();
            bus.Address.Returns(address);
            return bus;
        }

        [Fact]
        public void GetUri_returns_root_of_bus_address()
        {
            var bus = CreateBus(new Uri("loopback://localhost/some-queue"));

            var uri = bus.GetUri();

            Assert.Equal(new Uri("loopback://localhost/"), uri);
        }

        [Fact]
        public void GetUri_throws_when_bus_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => BusExtensions.GetUri(null!));
        }

        [Fact]
        public void GetAbsoluteEndpointUri_resolves_relative_uri_against_bus_root()
        {
            var bus = CreateBus(new Uri("loopback://localhost/some-queue"));

            var uri = bus.GetAbsoluteEndpointUri(new Uri("queue", UriKind.Relative));

            Assert.Equal(new Uri("loopback://localhost/queue"), uri);
        }

        [Fact]
        public void GetAbsoluteEndpointUri_returns_absolute_uri_unchanged()
        {
            var bus = CreateBus(new Uri("loopback://localhost/some-queue"));

            var uri = bus.GetAbsoluteEndpointUri(new Uri("rabbitmq://other/q"));

            Assert.Equal(new Uri("rabbitmq://other/q"), uri);
        }

        [Fact]
        public void GetAbsoluteEndpointUri_throws_when_bus_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => BusExtensions.GetAbsoluteEndpointUri(null!, new Uri("queue", UriKind.Relative)));
        }

        [Fact]
        public void GetAbsoluteEndpointUri_throws_when_endpoint_uri_is_null()
        {
            var bus = CreateBus(new Uri("loopback://localhost/some-queue"));

            Assert.Throws<ArgumentNullException>(() => bus.GetAbsoluteEndpointUri(null!));
        }

    }

}
