﻿using System;
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

            //
            // Core abstractions
            //
            services.TryAddSingleton<IServiceModelClientFactory, DefaultServiceModelClientFactory>();

            // Register channel factory
            if (configure is null)
            {
                services.TryAddSingleton(serviceProvider => new ChannelFactory<TChannel>(binding, remoteAddress));
            }
            else
            {
                services.TryAddSingleton(serviceProvider =>
                {
                    var channelFactory = new ChannelFactory<TChannel>(binding, remoteAddress);
                    configure?.Invoke(serviceProvider, channelFactory.Endpoint);
                    return channelFactory;
                });
            }

            // Register service model client
            services.TryAddTransient(
                serviceProvider => serviceProvider
                    .GetRequiredService<IServiceModelClientFactory>()
                    .CreateServiceModelClient<TChannel>());

            return services;
        }
    }
}
