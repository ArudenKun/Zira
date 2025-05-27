using Mediator;

namespace Application.Abstractions.Caching;

public interface ICacheInvalidatorCommand : ICacheInvalidatorCommand<Unit>;

public interface ICacheInvalidatorCommand<out TResponse> : ICommand<TResponse>
{
    string CacheKey => string.Empty;
    public IEnumerable<string>? Tags => null;
}
