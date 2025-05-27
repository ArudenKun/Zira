using Mediator;

namespace Application.Abstractions.Caching;

public interface ICacheQuery<out TResponse> : IQuery<TResponse>
{
    string CacheKey => string.Empty;
    public IEnumerable<string>? Tags => null;
}
