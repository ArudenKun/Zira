using System.Security.Claims;
using Clerk.Net.Client;
using Clerk.Net.Client.Sessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages;

public class SignOut : PageModel
{
    private readonly ILogger<SignOut> _logger;
    private readonly ClerkApiClient _clerkApiClient;

    public SignOut(
        ILogger<SignOut> logger,
        ClerkApiClient clerkApiClient,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _clerkApiClient = clerkApiClient;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity is { IsAuthenticated: false })
        {
            return Redirect(Url.Content("~/"));
        }

        var userId =
            User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("User is not authenticated");
        _logger.LogInformation("User Id: {UserId}", userId);
        var sessions = await _clerkApiClient.Sessions.GetAsync(configuration =>
        {
            configuration.QueryParameters.UserId = userId;
            configuration.QueryParameters.Status = GetStatusQueryParameterType.Active;
        });

        if (sessions is null || sessions.Count is 0)
        {
            _logger.LogInformation("No active sessions found");
            return Redirect(Url.Content("~/"));
        }

        // Revoke each session
        foreach (var session in sessions)
        {
            try
            {
                _logger.LogInformation("Session Id: {SessionId}", session.Id);
                await _clerkApiClient.Sessions[session.Id].Revoke.PostAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke session");
            }
        }

        await HttpContext.SignOutAsync();
        return Redirect(Url.Content("~/"));
    }
}
