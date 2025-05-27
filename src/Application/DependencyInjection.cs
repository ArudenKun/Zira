using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceScan.SourceGenerator;

namespace Application;

public static partial class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services.AddValidators();

    [GenerateServiceRegistrations(
        FromAssemblyOf = typeof(DependencyInjection),
        AssignableTo = typeof(IValidator<>),
        Lifetime = ServiceLifetime.Scoped
    )]
    public static partial IServiceCollection AddValidators(this IServiceCollection services);
}
