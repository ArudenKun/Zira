namespace Zira.Models;

/// <summary>
/// Represents the result of an authentication attempt.
/// </summary>
public record AuthenticationResult(
    bool IsSuccess,
    string? Error = null,
    string? ErrorDescription = null
);
