using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.Authentication;
using Volo.Abp.IdentityModel;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Zira.Settings;

namespace Zira.Services;

[PublicAPI]
[Dependency(ReplaceServices = true)]
public sealed class AuthenticationService
    : IRemoteServiceHttpClientAuthenticator,
        ISingletonDependency
{
    private string _username = string.Empty;
    private string _password = string.Empty;

    public AuthenticationService(
        IOptions<AbpIdentityClientOptions> options,
        ICancellationTokenProvider cancellationTokenProvider,
        IHttpClientFactory httpClientFactory,
        ICurrentTenant currentTenant,
        IOptions<IdentityModelHttpRequestMessageOptions> identityModelHttpRequestMessageOptions,
        IDistributedCache<IdentityModelTokenCacheItem> tokenCache,
        IDistributedCache<IdentityModelDiscoveryDocumentCacheItem> discoveryDocumentCache,
        IAbpHostEnvironment abpHostEnvironment
    )
    {
        ClientOptions = options.Value;
        CancellationTokenProvider = cancellationTokenProvider;
        HttpClientFactory = httpClientFactory;
        CurrentTenant = currentTenant;
        TokenCache = tokenCache;
        DiscoveryDocumentCache = discoveryDocumentCache;
        AbpHostEnvironment = abpHostEnvironment;
        IdentityModelHttpRequestMessageOptions = identityModelHttpRequestMessageOptions.Value;
        Logger = NullLogger<AuthenticationService>.Instance;
        SettingsManager = SettingsManager.Instance;
    }

    public ILogger<AuthenticationService> Logger { get; set; }
    public AbpIdentityClientOptions ClientOptions { get; }
    public ICancellationTokenProvider CancellationTokenProvider { get; }
    public IHttpClientFactory HttpClientFactory { get; }
    public ICurrentTenant CurrentTenant { get; }
    public IdentityModelHttpRequestMessageOptions IdentityModelHttpRequestMessageOptions { get; }
    public IDistributedCache<IdentityModelTokenCacheItem> TokenCache { get; }
    public IDistributedCache<IdentityModelDiscoveryDocumentCacheItem> DiscoveryDocumentCache { get; }
    public IAbpHostEnvironment AbpHostEnvironment { get; }
    public SettingsManager SettingsManager { get; }
    public string Url => SettingsManager.General.Url;

    async Task IRemoteServiceHttpClientAuthenticator.Authenticate(
        RemoteServiceHttpClientAuthenticateContext context
    )
    {
        var accessToken = await GetAccessTokenAsync();
        if (accessToken.IsNullOrEmpty())
            return;

        SetAccessToken(context.Client, accessToken);
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        _username = username;
        _password = password;

        var accessToken = await GetAccessTokenAsync();
        return !accessToken.IsNullOrEmpty();
    }

    public async Task LogoutAsync()
    {
        await DiscoveryDocumentCache.RemoveAsync(CalculateDiscoveryDocumentCacheKey());
        await TokenCache.RemoveAsync(CalculateTokenCacheKey());

        _username = string.Empty;
        _password = string.Empty;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (_username.IsNullOrEmpty() || _password.IsNullOrEmpty())
            return string.Empty;

        var cacheKey = CalculateTokenCacheKey();
        var tokenCacheItem = await TokenCache.GetAsync(cacheKey);
        if (tokenCacheItem is not null)
            return tokenCacheItem.AccessToken;

        var tokenResponse = await GetTokenResponse();

        if (tokenResponse.IsError)
        {
            if (tokenResponse.ErrorDescription != null)
            {
                throw new AbpException(
                    $"Could not get token from the OpenId Connect server! ErrorType: {tokenResponse.ErrorType}. "
                        + $"Error: {tokenResponse.Error}. ErrorDescription: {tokenResponse.ErrorDescription}. HttpStatusCode: {tokenResponse.HttpStatusCode}"
                );
            }

            var rawError = tokenResponse.Raw!;
            var withoutInnerException = rawError.Split(
                ["<eof/>"],
                StringSplitOptions.RemoveEmptyEntries
            );
            throw new AbpException(withoutInnerException[0]);
        }

        tokenCacheItem = new IdentityModelTokenCacheItem(tokenResponse.AccessToken!);
        await TokenCache.SetAsync(
            cacheKey,
            tokenCacheItem,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AbpHostEnvironment.IsDevelopment()
                    ? TimeSpan.FromSeconds(5)
                    : TimeSpan.FromSeconds(10),
            }
        );

        return tokenCacheItem.AccessToken;
    }

    private static void SetAccessToken(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
    }

    private async Task<TokenResponse> GetTokenResponse()
    {
        using var httpClient = HttpClientFactory.CreateClient();
        AddHeaders(httpClient);

        var response = await httpClient.RequestPasswordTokenAsync(
            await CreatePasswordTokenRequestAsync(),
            CancellationTokenProvider.Token
        );

        return response;
    }

    private void AddHeaders(HttpClient client)
    {
        //tenantId
        if (CurrentTenant.Id.HasValue)
        {
            //TODO: Use AbpAspNetCoreMultiTenancyOptions to get the key
            client.DefaultRequestHeaders.Add(
                TenantResolverConsts.DefaultTenantKey,
                CurrentTenant.Id.Value.ToString()
            );
        }
    }

    private async Task<PasswordTokenRequest> CreatePasswordTokenRequestAsync()
    {
        var discoveryResponse = await GetDiscoveryResponse();
        var request = new PasswordTokenRequest
        {
            Address = discoveryResponse.TokenEndpoint,
            Scope = "Zira",
            ClientId = "ClientId",
            UserName = _username,
            Password = _password,
        };

        IdentityModelHttpRequestMessageOptions.ConfigureHttpRequestMessage?.Invoke(request);

        return request;
    }

    private async Task<IdentityModelDiscoveryDocumentCacheItem> GetDiscoveryResponse()
    {
        var tokenEndpointUrlCacheKey = CalculateDiscoveryDocumentCacheKey();
        var discoveryDocumentCacheItem = await DiscoveryDocumentCache.GetAsync(
            tokenEndpointUrlCacheKey
        );
        if (discoveryDocumentCacheItem == null)
        {
            DiscoveryDocumentResponse discoveryResponse;
            using (var httpClient = HttpClientFactory.CreateClient())
            {
                var request = new DiscoveryDocumentRequest { Address = Url };
                IdentityModelHttpRequestMessageOptions.ConfigureHttpRequestMessage?.Invoke(request);
                discoveryResponse = await httpClient.GetDiscoveryDocumentAsync(request);
            }

            if (discoveryResponse.IsError)
            {
                throw new AbpException(
                    $"Could not retrieve the OpenId Connect discovery document! "
                        + $"ErrorType: {discoveryResponse.ErrorType}. Error: {discoveryResponse.Error}"
                );
            }

            discoveryDocumentCacheItem = new IdentityModelDiscoveryDocumentCacheItem(
                discoveryResponse.TokenEndpoint!,
                discoveryResponse.DeviceAuthorizationEndpoint!
            );
            await DiscoveryDocumentCache.SetAsync(
                tokenEndpointUrlCacheKey,
                discoveryDocumentCacheItem,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = AbpHostEnvironment.IsDevelopment()
                        ? TimeSpan.FromSeconds(5)
                        : TimeSpan.FromSeconds(10),
                }
            );
        }

        return discoveryDocumentCacheItem;
    }

    private string CalculateDiscoveryDocumentCacheKey()
    {
        return Url.IsNullOrWhiteSpace()
            ? throw new AbpException("Url is not valid")
            : Url.ToSha256();
    }

    private string CalculateTokenCacheKey()
    {
        return $"{_username}-{_password}".ToSha256();
    }
}
