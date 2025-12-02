using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.IdentityModel;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace Zira.Services;

[PublicAPI]
[Dependency(ReplaceServices = true)]
public sealed class ZiraIdentityModelAuthenticationService : IdentityModelAuthenticationService
{
    private readonly IAuthenticationManager _authenticationManager;

    public ZiraIdentityModelAuthenticationService(
        IOptions<AbpIdentityClientOptions> options,
        ICancellationTokenProvider cancellationTokenProvider,
        IHttpClientFactory httpClientFactory,
        ICurrentTenant currentTenant,
        IOptions<IdentityModelHttpRequestMessageOptions> identityModelHttpRequestMessageOptions,
        IDistributedCache<IdentityModelTokenCacheItem> tokenCache,
        IDistributedCache<IdentityModelDiscoveryDocumentCacheItem> discoveryDocumentCache,
        IAbpHostEnvironment abpHostEnvironment,
        IAuthenticationManager authenticationManager
    )
        : base(
            options,
            cancellationTokenProvider,
            httpClientFactory,
            currentTenant,
            identityModelHttpRequestMessageOptions,
            tokenCache,
            discoveryDocumentCache,
            abpHostEnvironment
        )
    {
        _authenticationManager = authenticationManager;
    }

    protected override async Task<string?> GetAccessTokenOrNullAsync(string? identityClientName) =>
        await _authenticationManager.GetAccessTokenAsync();
}
