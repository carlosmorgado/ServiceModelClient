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
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress));
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithNullBinding_ThrowsArgumentNullException()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = default(Binding)!;
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress));
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithNullEndpointAddress_ThrowsArgumentNullException()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = default(EndpointAddress)!;

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress));
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithoutRegisteredIServiceModelClientFactory_RegistersDefaultServiceModelClientFactory()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<IServiceModelClientFactory>();

            // Assert

            actual.ShouldBeOfType<DefaultServiceModelClientFactory>();
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithRegisteredIServiceModelClientFactory_DoesntRegisterDefaultServiceModelClientFactory()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);
            var expected = Mock.Of<IServiceModelClientFactory>();

            serviceCollection.AddSingleton(expected);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<IServiceModelClientFactory>();

            // Assert

            actual.ShouldBeSameAs(expected);
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithoutRegisteredChannelFactory_RegistersChannelFactory()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress);
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<ChannelFactory<ITestContract>>();

            // Assert

            actual.ShouldNotBeNull();
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithRegisteredChannelFactory_DoesntRegisterChannelFactoryWithoutConfiguration()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);

            using var expected = new ChannelFactory<ITestContract>(binding, enpointAddress);
            serviceCollection.AddSingleton(expected);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress);
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<ChannelFactory<ITestContract>>();

            // Assert

            actual.ShouldBeSameAs(expected);
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_WithRegisteredChannelFactory_DoesntRegisterChannelFactoryWithConfiguration()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);

            using var expected = new ChannelFactory<ITestContract>(binding, enpointAddress);
            serviceCollection.AddSingleton(expected);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress, (sp, se) => { });
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<ChannelFactory<ITestContract>>();

            // Assert

            actual.ShouldBeSameAs(expected);
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_RegisteredWithConfiguratio_ConfigurationExecutedOnceAndOnlyOnce()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);
            var configurationExecuted = 0;

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress, (sp, se) => configurationExecuted++);
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ChannelFactory<ITestContract>>();
            serviceProvider.GetService<ChannelFactory<ITestContract>>();

            // Assert

            configurationExecuted.ShouldBe(1);
        }

        [Fact]
        public static void AddServiceModelClientOfTChannel_RetrievingServiceModelClientFromServiceProvider_InvokesServiceModelClientFactoryCreateServiceModelClient()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();
            var binding = Mock.Of<Binding>();
            var enpointAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expected = Mock.Of<IServiceModelClient<ITestContract>>();
            var serviceModelClientFactoryMock = new Mock<IServiceModelClientFactory>();
            serviceModelClientFactoryMock.Setup(m => m.CreateServiceModelClient<ITestContract>()).Returns(expected);

            serviceCollection.AddSingleton(serviceModelClientFactoryMock.Object);

            // Act

            serviceCollection.AddServiceModelClient<ITestContract>(binding, enpointAddress);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var actual = serviceProvider.GetService<IServiceModelClient<ITestContract>>();

            // Assert

            serviceModelClientFactoryMock.Verify(m => m.CreateServiceModelClient<ITestContract>(), Times.Once());
            actual.ShouldBeSameAs(expected);
        }
    }
}
