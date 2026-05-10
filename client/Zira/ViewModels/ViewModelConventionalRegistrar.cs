using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using ZLinq;

namespace Zira.ViewModels;

public sealed class ViewModelConventionalRegistrar : DefaultConventionalRegistrar
{
    protected override bool IsConventionalRegistrationDisabled(Type type) =>
        !type.IsAssignableTo<ViewModel>() || base.IsConventionalRegistrationDisabled(type);

    protected override ServiceLifetime? GetLifeTimeOrNull(
        Type type,
        DependencyAttribute? dependencyAttribute
    )
    {
        if (type.IsAssignableTo<NavigationViewModel>())
            return ServiceLifetime.Transient;

        return base.GetLifeTimeOrNull(type, dependencyAttribute);
    }

    protected override List<Type> GetExposedServiceTypes(Type type)
    {
        var exposedServiceTypes = base.GetExposedServiceTypes(type).AsValueEnumerable();
        var viewModelBaseClasses = type.GetBaseClasses(typeof(ViewModel), false);
        return exposedServiceTypes.Union(viewModelBaseClasses).Distinct().ToList();
    }
}
