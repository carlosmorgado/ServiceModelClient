using System.Threading.Tasks;

namespace Morgados.ServiceModel.Client.OpenIDConnect
{
    public interface IOpenIDConnectClient
    {
        Task<string> GetTokenAsync(string scopes);
    }
}
