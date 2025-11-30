using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoInterfaceAttributes;
using Duende.IdentityModel.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.IdentityModel;

namespace Zira.Services;

[AutoInterface]
[PublicAPI]
public sealed class AccessTokenService : IAccessTokenService, ISingletonDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AbpRemoteServiceOptions _remoteServiceOptions;
    private readonly AbpIdentityClientOptions _identityClientOptions;

    public AccessTokenService(
        IHttpClientFactory httpClientFactory,
        IOptions<AbpRemoteServiceOptions> remoteServiceOptions,
        IOptions<AbpIdentityClientOptions> identityClientOptions
    )
    {
        _httpClientFactory = httpClientFactory;
        _remoteServiceOptions = remoteServiceOptions.Value;
        _identityClientOptions = identityClientOptions.Value;
    }

    public string AccessToken { get; set; } = string.Empty;

    public async Task<string> ObtainAccessTokenAsync(string userName, string password)
    {
        var discoveryResponse = await GetDiscoveryResponseAsync();
        var tokenResponse = await GetTokenResponseAsync(discoveryResponse, userName, password);
        if (AccessToken.IsNullOrWhiteSpace() || AccessToken.IsNullOrEmpty())
        {
            AccessToken = tokenResponse.AccessToken ?? string.Empty;
        }

        return AccessToken;
    }

    private async Task<DiscoveryDocumentResponse> GetDiscoveryResponseAsync()
    {
        var remoteServiceConfiguration = _remoteServiceOptions.RemoteServices.Default;
        var httpClient = _httpClientFactory.CreateClient();
        return await httpClient.GetDiscoveryDocumentAsync(
            new DiscoveryDocumentRequest
            {
                Address = remoteServiceConfiguration?.BaseUrl,
                Policy = { RequireHttps = true },
            }
        );
    }

    private async Task<TokenResponse> GetTokenResponseAsync(
        DiscoveryDocumentResponse discoveryResponse,
        string userName,
        string password
    )
    {
        var httpClient = _httpClientFactory.CreateClient();
        return await httpClient.RequestPasswordTokenAsync(
            await CreatePasswordTokenRequestAsync(discoveryResponse, userName, password)
        );
    }

    private Task<PasswordTokenRequest> CreatePasswordTokenRequestAsync(
        DiscoveryDocumentResponse discoveryResponse,
        string userName,
        string password
    )
    {
        var config = _identityClientOptions.IdentityClients.Default!;
        var request = new PasswordTokenRequest
        {
            Address = discoveryResponse.TokenEndpoint,
            Scope = config.Scope,
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            UserName = userName,
            Password = password,
        };
        return Task.FromResult(request);
    }
}
