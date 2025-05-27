using Application.Abstractions.Caching;
using Mediator;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Pipelines;

public sealed class CacheBehavior<TQuery, TResponse> : IPipelineBehavior<TQuery, TResponse>
    where TQuery : ICacheQuery<TResponse>
{
    private readonly ILogger<CacheBehavior<TQuery, TResponse>> _logger;
    private readonly IFusionCache _cache;

    public CacheBehavior(ILogger<CacheBehavior<TQuery, TResponse>> logger, IFusionCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async ValueTask<TResponse> Handle(
        TQuery query,
        MessageHandlerDelegate<TQuery, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug(
            "Handling query of type {RequestType} with cache key {CacheKey}",
            nameof(query),
            query.CacheKey
        );
        var response = await _cache
            .GetOrSetAsync(
                query.CacheKey,
                async ct => await next(query, ct),
                tags: query.Tags,
                token: cancellationToken
            )
            .ConfigureAwait(false);

        return response;
    }
}
