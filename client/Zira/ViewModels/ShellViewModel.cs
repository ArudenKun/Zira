using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Zira.Extensions;
using Zira.Views;

namespace Zira.ViewModels;

[Dependency(ServiceLifetime.Singleton)]
public sealed class ShellViewModel : NavigationViewModel
{
    public override void OnLoaded()
    {
        RegionManager.RequestNavigate<ShellView>(
            Regions.Main,
            onComplete: result =>
            {
                Logger.LogWarning(
                    "Result {0}: {1}",
                    result.Status.ToString(),
                    result.Exception?.Message
                );
            }
        );
    }
}
