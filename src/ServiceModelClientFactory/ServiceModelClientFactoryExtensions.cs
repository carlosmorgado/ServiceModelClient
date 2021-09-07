using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Morgados.ServiceModelClientFactory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceModelClientFactoryExtensions
    {
        public static IServiceCollection AddServiceModelClient<TChannel>(
            this IServiceCollection services,
            Binding binding,
            EndpointAddress remoteAddress,
            Action<IServiceProvider, ServiceEndpoint>? configure = null)
            where TChannel : class
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (binding is null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            if (remoteAddress is null)
            {
                throw new ArgumentNullException(nameof(remoteAddress));
            }

            // Register channel factory
            if (configure is null)
            {
                services.TryAddSingleton<IChannelFactory<TChannel>>(serviceProvider => new ChannelFactory<TChannel>(binding, null));
            }
            else
            {
                services.TryAddSingleton<IChannelFactory<TChannel>>(serviceProvider =>
                {
                    var channelFactory = new ChannelFactory<TChannel>(binding, null);
                    configure?.Invoke(serviceProvider, channelFactory.Endpoint);
                    return channelFactory;
                });
            }

            // Register service model client
            services.TryAddTransient<IServiceModelClient<TChannel>>(
                serviceProvider => new DefaultServiceModelClient<TChannel>(
                    serviceProvider.GetRequiredService<IChannelFactory<TChannel>>(),
                    remoteAddress));

            return services;
        }
    }
}
