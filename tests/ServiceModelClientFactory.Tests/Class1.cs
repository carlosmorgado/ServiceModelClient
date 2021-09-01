using System.Net.Http;
using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Morgados.ServiceModel.Client.OpenIDConnect;

namespace Morgados.ServiceModelClientFactory.Tests
{
    internal class Class1
    {
        private void X()
        {
            var sc = new ServiceCollection();

            sc.AddHttpClient("OpenIDConnect");

            sc.AddServiceModelClient<MyChannel>(
                new BasicHttpBinding(),
                new EndpointAddress(string.Empty),
                (services, endpoint) =>
                {
                    endpoint.EndpointBehaviors.Add(
                        new OpenIDConnectClientAuthenticationEndpointBehavior(
                            () => services
                                .GetRequiredService<IHttpClientFactory>()
                                .CreateClient("OpenIDConnect")));
                });
        }
    }
}

internal interface MyChannel { }
