using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using JetBrains.Annotations;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.Authentication;

namespace Zira.Services;

[PublicAPI]
public sealed class RemoteServiceHttpClientAuthenticator
    : IRemoteServiceHttpClientAuthenticator,
        ITransientDependency
{
    private readonly IAccessTokenService _accessTokenService;

    public RemoteServiceHttpClientAuthenticator(IAccessTokenService accessTokenService)
    {
        _accessTokenService = accessTokenService;
    }

    public Task Authenticate(RemoteServiceHttpClientAuthenticateContext context)
    {
        context.Client.SetBearerToken(_accessTokenService.AccessToken);
        return Task.CompletedTask;
    }
}
