using System.Security.Claims;
using Domain.Entities.Users;

namespace Application.Features.Authentication;

public static class ClaimsExtensions
{
    public static UserId GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var value = claimsPrincipal
            .Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
            ?.Value;
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (!UserId.TryParse(value, out var userId))
        {
            throw new ArgumentException($"Invalid user ID: {value}");
        }

        return userId;
    }
}
