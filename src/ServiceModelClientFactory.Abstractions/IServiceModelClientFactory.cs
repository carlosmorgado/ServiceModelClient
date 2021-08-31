namespace Morgados.ServiceModelClientFactory
{
    public interface IServiceModelClientFactory
    {
        IServiceModelClient<TChannel> CreateServiceModelClient<TChannel>()
            where TChannel : class;
    }
}

