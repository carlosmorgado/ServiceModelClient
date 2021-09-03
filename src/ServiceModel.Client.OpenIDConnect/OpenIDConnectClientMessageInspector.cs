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
        private readonly Func<IOpenIDConnectClient> openIDConnectClientFactory;

        public OpenIDConnectClientMessageInspector(Func<IOpenIDConnectClient> openIDConnectClientFactory)
            => this.openIDConnectClientFactory = openIDConnectClientFactory;

        public object? BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var synchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                var token = this.openIDConnectClientFactory().GetTokenAsync(string.Empty).GetAwaiter().GetResult();

                if (!(request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out var property)
                    && property is HttpRequestMessageProperty httprequestMessageProperty))
                {
                    httprequestMessageProperty = new HttpRequestMessageProperty();
                    request.Properties[HttpRequestMessageProperty.Name] = httprequestMessageProperty;
                }

                httprequestMessageProperty.Headers["Authentication"] = $"Bearer {token}";

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
