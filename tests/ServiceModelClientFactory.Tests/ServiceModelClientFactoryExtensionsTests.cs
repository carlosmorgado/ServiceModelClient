using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace Morgados.ServiceModelClientFactory.Tests
{
    public static class ServiceModelClientFactoryExtensionsTests
    {
        [Fact]
        public static void AddServiceModelClientOfTChannel_WithNullServiceCollection_ThrowsArgumentNullException()
        {
            // Arrange

            var serviceCollection = default(ServiceCollection)!;
            var binding = Mock.Of<Binding>();
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress));
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithNullBinding_ThrowsArgumentNullException()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = default(Binding)!;
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress));
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithNullEndpointAddress_ThrowsArgumentNullException()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var remoteAddress = default(EndpointAddress)!;

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress));
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithoutRegisteredChannelFactory_RegistersChannelFactory()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress);
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<IChannelFactory<ITestContract>>();

            // Assert

            actual.ShouldNotBeNull();
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithRegisteredChannelFactory_DoesntRegisterChannelFactoryWithoutConfiguration()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expected = Mock.Of<IChannelFactory<ITestContract>>();
            serviceCollection.AddSingleton(expected);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress);
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<IChannelFactory<ITestContract>>();

            // Assert

            actual.ShouldBeSameAs(expected);
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithRegisteredChannelFactory_DoesntRegisterChannelFactoryWithConfiguration()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expected = Mock.Of<IChannelFactory<ITestContract>>();
            serviceCollection.AddSingleton(expected);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress, (sp, se) => { });
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<IChannelFactory<ITestContract>>();

            // Assert

            actual.ShouldBeSameAs(expected);
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_RegisteredWithConfiguration_ConfigurationExecutedOnceAndOnlyOnce()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);
            var configurationExecuted = 0;

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress, (sp, se) => configurationExecuted++);
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<IChannelFactory<ITestContract>>();
            serviceProvider.GetService<IChannelFactory<ITestContract>>();

            // Assert

            configurationExecuted.ShouldBe(1);
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_RetrievingServiceModelClientFromServiceProvider_InvokesServiceModelClientFactoryCreateServiceModelClient()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expected = Mock.Of<ITestContract>();
            var channelFactoryMock = new Mock<IChannelFactory<ITestContract>>();
            channelFactoryMock.Setup(m => m.CreateChannel(remoteAddress)).Returns(expected);

            serviceCollection.AddSingleton(channelFactoryMock.Object);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, remoteAddress);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<IServiceModelClient<ITestContract>>();

            // Assert

            actual.ShouldNotBeNull();
        }
    }
}
