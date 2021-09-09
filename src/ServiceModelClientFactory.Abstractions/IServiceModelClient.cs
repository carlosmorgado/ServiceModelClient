using System;

namespace Morgados.ServiceModelClientFactory
{
    public interface IServiceModelClient<TChannel> : IDisposable, IAsyncDisposable
        where TChannel : class
    {
        TChannel Channel { get; }
    }
}
