using Application.Abstractions.Caching;

namespace Application.Features.Users.Commands;

public sealed record DeleteUserCommand : ICacheInvalidatorCommand;
