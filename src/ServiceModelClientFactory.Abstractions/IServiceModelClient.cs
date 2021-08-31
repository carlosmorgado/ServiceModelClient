using System;

namespace Morgados.ServiceModelClientFactory
{
    public interface IServiceModelClient<TChannel> : IDisposable
        where TChannel : class
    {
        TChannel Channel { get; }
    }
}
