using AutoInterfaceAttributes;
using Duende.IdentityModel.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Identity;
using Volo.Abp.IdentityModel;

namespace Zira.Services;

/// <summary>
/// Represents the result of an authentication attempt.
/// </summary>
public record AuthenticationResult(
    bool IsSuccess,
    string? Error = null,
    string? ErrorDescription = null
);

[AutoInterface]
[PublicAPI]
public sealed class AuthenticationManager
    : IAuthenticationManager,
        ISingletonDependency,
        IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly ILogger<AuthenticationManager> _logger;
    private readonly AbpRemoteServiceOptions _remoteServiceOptions;
    private readonly AbpIdentityClientOptions _identityClientOptions;

    private const int ExpirationBufferSeconds = 60;

    private readonly SemaphoreSlim _lock = new(1, 1);

    // State
    private string? _userName;
    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset _accessTokenExpiration;

    // Cache
    private IdentityUserDto? _identityUserDto;

    public AuthenticationManager(
        IHttpClientFactory httpClientFactory,
        IIdentityUserAppService identityUserAppService,
        IOptions<AbpRemoteServiceOptions> remoteServiceOptions,
        IOptions<AbpIdentityClientOptions> identityClientOptions,
        ILogger<AuthenticationManager> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _identityUserAppService = identityUserAppService;
        _logger = logger;
        _remoteServiceOptions = remoteServiceOptions.Value;
        _identityClientOptions = identityClientOptions.Value;
    }

    public bool IsAuthenticated =>
        !string.IsNullOrEmpty(_accessToken) && !IsTokenExpiredOrExpiring();

    public async Task<AuthenticationResult> AuthenticateAsync(string userName, string password)
    {
        await _lock.WaitAsync();
        try
        {
            // Reset state
            ClearState();
            _userName = userName;

            var discoveryResponse = await GetDiscoveryResponseAsync();
            if (discoveryResponse.IsError)
            {
                LogProtocolResponse("Discovery", discoveryResponse);
                return new AuthenticationResult(false, discoveryResponse.Error);
            }

            var tokenResponse = await GetPasswordTokenResponseAsync(
                discoveryResponse,
                userName,
                password
            );
            if (tokenResponse.IsError)
            {
                LogProtocolResponse("Token Request", tokenResponse);
                return new AuthenticationResult(
                    false,
                    tokenResponse.Error,
                    tokenResponse.ErrorDescription
                );
            }

            SetTokenData(tokenResponse);
            return new AuthenticationResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during authentication.");
            return new AuthenticationResult(false, "Unexpected error", ex.Message);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task LogoutAsync()
    {
        await _lock.WaitAsync();
        try
        {
            // 1. If we have no tokens, just clear local state and return
            if (string.IsNullOrEmpty(_refreshToken) && string.IsNullOrEmpty(_accessToken))
            {
                ClearState();
                return;
            }

            // 2. Get the Discovery Document to find the Revocation Endpoint
            var discoveryResponse = await GetDiscoveryResponseAsync();
            if (discoveryResponse.IsError)
            {
                // If we can't reach the server, we log it, but we MUST still clear local state below
                _logger.LogWarning(
                    "Logout revocation skipped: Could not retrieve discovery document. {Error}",
                    discoveryResponse.Error
                );
                return;
            }

            // 3. Prepare Client
            var client = CreateClient();
            var config = _identityClientOptions.IdentityClients.Default!;

            // 4. Revoke Refresh Token (Priority)
            // Revoking the Refresh Token is crucial as it prevents generating new Access Tokens.
            if (!string.IsNullOrEmpty(_refreshToken))
            {
                var revokeRefreshResponse = await client.RevokeTokenAsync(
                    new TokenRevocationRequest
                    {
                        Address = discoveryResponse.RevocationEndpoint,
                        ClientId = config.ClientId,
                        ClientSecret = config.ClientSecret,
                        Token = _refreshToken,
                        TokenTypeHint = "refresh_token",
                    }
                );

                if (revokeRefreshResponse.IsError)
                {
                    _logger.LogWarning(
                        "Failed to revoke Refresh Token: {Error}",
                        revokeRefreshResponse.Error
                    );
                }
            }

            // 5. Revoke Access Token (Optional)
            // Useful if you are using Reference Tokens, but harmless for JWTs.
            if (!string.IsNullOrEmpty(_accessToken))
            {
                var revokeAccessResponse = await client.RevokeTokenAsync(
                    new TokenRevocationRequest
                    {
                        Address = discoveryResponse.RevocationEndpoint,
                        ClientId = config.ClientId,
                        ClientSecret = config.ClientSecret,
                        Token = _accessToken,
                        TokenTypeHint = "access_token",
                    }
                );

                if (revokeAccessResponse.IsError)
                {
                    // We log as debug/warning only, as access tokens might already be expired
                    _logger.LogDebug(
                        "Failed to revoke Access Token: {Error}",
                        revokeAccessResponse.Error
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during server-side token revocation.");
        }
        finally
        {
            // 6. CRITICAL: Always clear local state.
            // Even if the server is down and revocation fails, the user must be logged out locally.
            ClearState();
            _lock.Release();
        }
    }

    public async ValueTask<IdentityUserDto?> GetUserAsync()
    {
        // Check local cache first
        if (_identityUserDto is not null)
        {
            return _identityUserDto;
        }

        if (string.IsNullOrEmpty(_userName))
        {
            return null;
        }

        try
        {
            // Ensure we have a valid token before calling the API
            await GetAccessTokenAsync();

            // Note: Ensure the IIdentityUserAppService is configured to use the token managed here,
            // otherwise this call might fail with 401.
            _identityUserDto = await _identityUserAppService.FindByUsernameAsync(_userName);
            return _identityUserDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user details for {UserName}", _userName);
            return null;
        }
    }

    public async ValueTask<string> GetAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            return string.Empty;
        }

        if (IsTokenExpiredOrExpiring())
        {
            if (!await RefreshTokenAsync())
            {
                return string.Empty;
            }
        }

        return _accessToken ?? string.Empty;
    }

    /// <summary>
    /// Returns true if refresh was successful, false otherwise.
    /// </summary>
    private async Task<bool> RefreshTokenAsync()
    {
        await _lock.WaitAsync();
        try
        {
            // Double-check locking: Token might have been refreshed while waiting for lock
            if (!IsTokenExpiredOrExpiring())
            {
                return true;
            }

            if (string.IsNullOrEmpty(_refreshToken))
            {
                _logger.LogWarning("Cannot refresh token: Refresh token is missing.");
                return false;
            }

            var discoveryResponse = await GetDiscoveryResponseAsync();
            if (discoveryResponse.IsError)
            {
                LogProtocolResponse("Discovery (Refresh)", discoveryResponse);
                return false;
            }

            var client = CreateClient();
            var config = _identityClientOptions.IdentityClients.Default!;

            var refreshResponse = await client.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    Address = discoveryResponse.TokenEndpoint,
                    ClientId = config.ClientId,
                    ClientSecret = config.ClientSecret,
                    RefreshToken = _refreshToken,
                }
            );

            if (!refreshResponse.IsError)
            {
                SetTokenData(refreshResponse);
                return true;
            }

            LogProtocolResponse("Token Refresh", refreshResponse);

            // If refresh fails (e.g., refresh token expired), clear session to force re-login
            ClearState();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during token refresh.");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void SetTokenData(TokenResponse response)
    {
        _accessToken = response.AccessToken;
        _refreshToken = response.RefreshToken;
        _accessTokenExpiration = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn);
    }

    private void ClearState()
    {
        _userName = null;
        _accessToken = null;
        _refreshToken = null;
        _identityUserDto = null;
        _accessTokenExpiration = DateTimeOffset.MinValue;
    }

    private bool IsTokenExpiredOrExpiring()
    {
        return string.IsNullOrEmpty(_accessToken)
            || DateTimeOffset.UtcNow.AddSeconds(ExpirationBufferSeconds) >= _accessTokenExpiration;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient();
    }

    private async Task<DiscoveryDocumentResponse> GetDiscoveryResponseAsync()
    {
        var remoteServiceConfiguration = _remoteServiceOptions.RemoteServices.Default;
        var client = CreateClient();

        return await client.GetDiscoveryDocumentAsync(
            new DiscoveryDocumentRequest
            {
                Address = remoteServiceConfiguration?.BaseUrl,
                Policy =
                {
                    RequireHttps =
                        _remoteServiceOptions.RemoteServices.Default?.BaseUrl.StartsWith("https")
                        ?? true,
                },
            }
        );
    }

    private async Task<TokenResponse> GetPasswordTokenResponseAsync(
        DiscoveryDocumentResponse discoveryResponse,
        string userName,
        string password
    )
    {
        var client = CreateClient();
        var config = _identityClientOptions.IdentityClients.Default!;

        return await client.RequestPasswordTokenAsync(
            new PasswordTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                Scope = config.Scope,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                UserName = userName,
                Password = password,
            }
        );
    }

    private void LogProtocolResponse(string context, ProtocolResponse response)
    {
        if (response.Exception != null)
        {
            _logger.LogError(
                response.Exception,
                "{Context} failed: {Error}",
                context,
                response.Error
            );
        }
        else
        {
            _logger.LogWarning("{Context} failed: {Error}", context, response.Error);
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}
