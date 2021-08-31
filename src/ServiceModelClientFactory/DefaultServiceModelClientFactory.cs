using System;
using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;

namespace Morgados.ServiceModelClientFactory
{
    internal sealed class DefaultServiceModelClientFactory : IServiceModelClientFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultServiceModelClientFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IServiceModelClient<TChannel> CreateServiceModelClient<TChannel>()
            where TChannel : class
        {
            var channelFactory = this.serviceProvider.GetRequiredService<ChannelFactory<TChannel>>();
            return new DefaultServiceModelClient<TChannel>(channelFactory);
        }
    }
}
