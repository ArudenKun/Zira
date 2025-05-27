using Application.Abstractions.Caching;
using FluentValidation;
using Mediator;

namespace Application.Pipelines;

public sealed class ValidationBehavior<TQuery, TResponse> : IPipelineBehavior<TQuery, TResponse>
    where TQuery : ICacheQuery<TResponse>
{
    private readonly IEnumerable<IValidator<TQuery>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TQuery>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TQuery query,
        MessageHandlerDelegate<TQuery, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var context = new ValidationContext<TQuery>(query);
        var failures = _validators
            .Select(x => x.Validate(context))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .ToList();

        if (failures.Count is not 0)
        {
            throw new ValidationException(failures);
        }

        return await next(query, cancellationToken).ConfigureAwait(false);
    }
}
