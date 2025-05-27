using Application.Abstractions.Caching;
using Mediator;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;
using ZLogger;

namespace Application.Pipelines;

public sealed class CacheInvalidatorBehavior<TCommand, TResponse>
    : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICacheInvalidatorCommand
{
    private readonly ILogger<CacheInvalidatorBehavior<TCommand, TResponse>> _logger;
    private readonly IFusionCache _cache;

    public CacheInvalidatorBehavior(
        ILogger<CacheInvalidatorBehavior<TCommand, TResponse>> logger,
        IFusionCache cache
    )
    {
        _logger = logger;
        _cache = cache;
    }

    public async ValueTask<TResponse> Handle(
        TCommand command,
        MessageHandlerDelegate<TCommand, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        _logger.ZLogDebug($"Handling command of type {nameof(command)} with details {@command}");

        var response = await next(command, cancellationToken);
        if (!string.IsNullOrEmpty(command.CacheKey) && !string.IsNullOrWhiteSpace(command.CacheKey))
        {
            await _cache.RemoveAsync(command.CacheKey, token: cancellationToken);
            _logger.ZLogDebug($"Cache key {command.CacheKey} removed from cache");
        }

        if (command.Tags is null || !command.Tags.Any())
        {
            return response;
        }

        foreach (var tag in command.Tags)
        {
            await _cache.RemoveAsync(tag, token: cancellationToken);
            _logger.ZLogDebug($"Cache tag {tag} removed from cache");
        }

        return response;
    }
}
