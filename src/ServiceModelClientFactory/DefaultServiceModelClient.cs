using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Morgados.ServiceModelClientFactory
{
    internal sealed class DefaultServiceModelClient<TChannel> : IServiceModelClient<TChannel>
        where TChannel : class
    {
        private readonly ChannelFactory<TChannel> channelFactory;
        private TChannel? channel;
        private bool isDisposed;

        public DefaultServiceModelClient(ChannelFactory<TChannel> channelFactory)
        {
            this.channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
            this.isDisposed = false;
        }

        public TChannel Channel
        {
            get
            {
                var channel = (IChannel)(this.channel ??= this.channelFactory.CreateChannel());

                channel.Open();

                return this.channel;
            }
        }

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
    }
}
