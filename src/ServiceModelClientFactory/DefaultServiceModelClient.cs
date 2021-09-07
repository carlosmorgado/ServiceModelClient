﻿using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Morgados.ServiceModelClientFactory
{
    internal sealed class DefaultServiceModelClient<TChannel> : IServiceModelClient<TChannel>
        where TChannel : class
    {
        private readonly IChannelFactory<TChannel> channelFactory;
        private readonly EndpointAddress remoteAddress;
        private TChannel? channel;
        private bool isDisposed;

        public DefaultServiceModelClient(IChannelFactory<TChannel> channelFactory, EndpointAddress remoteAddress)
        {
            this.channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
            this.remoteAddress = remoteAddress ?? throw new ArgumentNullException(nameof(remoteAddress));
            this.isDisposed = false;
        }

        public TChannel Channel => this.channel ??= this.CreateChannel();

        public void Dispose()
        {
            if (!this.isDisposed && this.channel is IClientChannel clientChannel)
            {
                if (clientChannel.State == CommunicationState.Faulted)
                {
                    clientChannel.Abort();
                }
                else
                {
                    clientChannel.Close();
                }

                clientChannel.Dispose();
            }

            this.channel = null!;
            this.isDisposed = true;
        }

        private TChannel CreateChannel()
        {
            var channel = this.channelFactory.CreateChannel(this.remoteAddress);
            ((IChannel)channel).Open();
            return channel;
        }
    }
}
