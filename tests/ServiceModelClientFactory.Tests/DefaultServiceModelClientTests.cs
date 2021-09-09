using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;

namespace Morgados.ServiceModelClientFactory.Tests
{
    public static class DefaultServiceModelClientTests
    {
        [Fact]
        public static void Constructor_WithNullChannelFactory_ThrowsArgumentNullException()
        {
            // Arrange

            var channelFactory = default(IChannelFactory<ITestContract>)!;
            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => new DefaultServiceModelClient<ITestContract>(channelFactory, remoteAddress));
        }

        [Fact]
        public static void Constructor_WithNullRemoteAddress_ThrowsArgumentNullException()
        {
            // Arrange

            var channelFactory = Mock.Of<IChannelFactory<ITestContract>>();
            var remoteAddress = default(EndpointAddress)!;

            // Act / Assert

            Should.Throw<ArgumentNullException>(() => new DefaultServiceModelClient<ITestContract>(channelFactory, remoteAddress));
        }

        [Fact]
        public static void Channel_Getter_CreatesChannelFromChannelFactoryOnceAndOnlyOnce()
        {
            // Arrange

            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expectedMock = new Mock<ITestContract>();
            expectedMock.As<IChannel>();

            var channelFactoryMock = new Mock<IChannelFactory<ITestContract>>();
            channelFactoryMock.Setup(cf => cf.CreateChannel(remoteAddress)).Returns(expectedMock.Object);

            using var target = new DefaultServiceModelClient<ITestContract>(channelFactoryMock.Object, remoteAddress);

            // Act

            var actual = target.Channel;

            // Assert

            channelFactoryMock.Verify(cf => cf.CreateChannel(remoteAddress), Times.Once());
            actual.ShouldBeSameAs(expectedMock.Object);
        }

        [Fact]
        public static void Channel_GetterWhenChannelCreated_OpensChannel()
        {
            // Arrange

            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expectedMock = new Mock<ITestContract>();
            var channelMock = expectedMock.As<IChannel>();

            var channelFactoryMock = new Mock<IChannelFactory<ITestContract>>();
            channelFactoryMock.Setup(cf => cf.CreateChannel(remoteAddress)).Returns(expectedMock.Object);

            using var target = new DefaultServiceModelClient<ITestContract>(channelFactoryMock.Object, remoteAddress);

            // Act

            var actual = target.Channel;

            // Assert

            channelMock.Verify(c => c.Open(), Times.Once());
        }

        [Theory]
        [InlineData(CommunicationState.Opening)]
        [InlineData(CommunicationState.Opened)]
        [InlineData(CommunicationState.Closing)]
        [InlineData(CommunicationState.Closed)]
        public static void Dispose_WhenNotDisposedAndNotFaulted_InvokesCloseAndDispose(CommunicationState state)
        {
            // Arrange

            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expectedMock = new Mock<ITestContract>();
            var clientChannelMock = expectedMock.As<IClientChannel>();

            var channelFactoryMock = new Mock<IChannelFactory<ITestContract>>();
            channelFactoryMock.Setup(cf => cf.CreateChannel(remoteAddress)).Returns(expectedMock.Object);

            var target = new DefaultServiceModelClient<ITestContract>(channelFactoryMock.Object, remoteAddress);
            var actual = target.Channel;

            // Act

            target.Dispose();

            // Assert

            clientChannelMock.Verify(cc => cc.Close(), Times.Once());
            clientChannelMock.Verify(cc => cc.Dispose(), Times.Once());
        }

        [Fact]
        public static void Dispose_WhenNotDisposedAndFaulted_InvokesAbortAndDispose()
        {
            // Arrange

            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expectedMock = new Mock<ITestContract>();
            var clientChannelMock = expectedMock.As<IClientChannel>();
            clientChannelMock.Setup(cc => cc.State).Returns(CommunicationState.Faulted);

            var channelFactoryMock = new Mock<IChannelFactory<ITestContract>>();
            channelFactoryMock.Setup(cf => cf.CreateChannel(remoteAddress)).Returns(expectedMock.Object);

            var target = new DefaultServiceModelClient<ITestContract>(channelFactoryMock.Object, remoteAddress);
            var actual = target.Channel;

            // Act

            target.Dispose();

            // Assert

            clientChannelMock.Verify(cc => cc.Abort(), Times.Once());
            clientChannelMock.Verify(cc => cc.Dispose(), Times.Once());
        }

        [Theory]
        [InlineData(CommunicationState.Opening)]
        [InlineData(CommunicationState.Opened)]
        [InlineData(CommunicationState.Closing)]
        [InlineData(CommunicationState.Closed)]
        public static async Task DisposeAsync_WhenNotDisposedAndNotFaulted_InvokesCloseAndDispose(CommunicationState state)
        {
            // Arrange

            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            using var asyncWaitHandle = new ManualResetEvent(true);
            var asyncResult = Mock.Of<IAsyncResult>(ar => ar.AsyncWaitHandle == asyncWaitHandle);
            var expectedMock = new Mock<ITestContract>();
            var clientChannelMock = expectedMock.As<IClientChannel>();
            clientChannelMock.Setup(cc => cc.BeginClose(null, null)).Returns(asyncResult);

            var channelFactoryMock = new Mock<IChannelFactory<ITestContract>>();
            channelFactoryMock.Setup(cf => cf.CreateChannel(remoteAddress)).Returns(expectedMock.Object);

            var target = new DefaultServiceModelClient<ITestContract>(channelFactoryMock.Object, remoteAddress);
            var actual = target.Channel;

            // Act

            await target.DisposeAsync();

            // Assert

            clientChannelMock.Verify(cc => cc.BeginClose(null, null), Times.Once());
            clientChannelMock.Verify(cc => cc.EndClose(asyncResult), Times.Once());
            clientChannelMock.Verify(cc => cc.Dispose(), Times.Once());
        }

        [Fact]
        public static async Task DisposeAsync_WhenNotDisposedAndFaulted_InvokesAbortAndDispose()
        {
            // Arrange

            var remoteAddress = new EndpointAddress(EndpointAddress.NoneUri);

            var expectedMock = new Mock<ITestContract>();
            var clientChannelMock = expectedMock.As<IClientChannel>();
            clientChannelMock.Setup(cc => cc.State).Returns(CommunicationState.Faulted);

            var channelFactoryMock = new Mock<IChannelFactory<ITestContract>>();
            channelFactoryMock.Setup(cf => cf.CreateChannel(remoteAddress)).Returns(expectedMock.Object);

            var target = new DefaultServiceModelClient<ITestContract>(channelFactoryMock.Object, remoteAddress);
            var actual = target.Channel;

            // Act

            await target.DisposeAsync();

            // Assert

            clientChannelMock.Verify(cc => cc.Abort(), Times.Once());
            clientChannelMock.Verify(cc => cc.Dispose(), Times.Once());
        }
    }
}
