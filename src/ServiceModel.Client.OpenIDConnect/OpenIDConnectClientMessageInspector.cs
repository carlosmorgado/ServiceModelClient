using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System;

namespace Morgados.ServiceModel.Client.OpenIDConnect
{
    public sealed class OpenIDConnectClientMessageInspector : IClientMessageInspector
    {
        private readonly Func<HttpClient> httpClientFactory;

        public OpenIDConnectClientMessageInspector(Func<HttpClient> httpClientFactory)
            => this.httpClientFactory = httpClientFactory;

        public object? BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var synchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                this.httpClientFactory().GetAsync("").GetAwaiter().GetResult();

                if (!(request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out var property)
                    && property is HttpRequestMessageProperty httprequestMessageProperty))
                {
                    httprequestMessageProperty = new HttpRequestMessageProperty();
                    request.Properties[HttpRequestMessageProperty.Name] = httprequestMessageProperty;
                }

                httprequestMessageProperty.Headers["Authentication"] = "Bearer";

                return null;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
    }
}
