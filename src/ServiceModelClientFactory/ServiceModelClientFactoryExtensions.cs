using System;
using System.Collections.Generic;
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
            //
            // Core abstractions
            //
            services.TryAddSingleton<IServiceModelClientFactory, DefaultServiceModelClientFactory>();

            // Register channel factory
            if (configure is not null)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(configure));
            }

            services.TryAddSingleton(serviceProvider =>
            {
                var factory = new ChannelFactory<TChannel>(binding, remoteAddress);

                var configurations = serviceProvider
                    .GetRequiredService<IEnumerable<Action<IServiceProvider, ServiceEndpoint>>>();

                foreach (var action in configurations)
                {
                    action(serviceProvider, factory.Endpoint);
                }

                return factory;
            });

            // Register service model client
            services.TryAddTransient(
                serviceProvider => serviceProvider
                    .GetRequiredService<IServiceModelClientFactory>()
                    .CreateServiceModelClient<TChannel>());

            return services;
        }
    }
}
