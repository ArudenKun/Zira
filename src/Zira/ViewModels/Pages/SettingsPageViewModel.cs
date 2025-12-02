using Lucide.Avalonia;
using Volo.Abp.DependencyInjection;

namespace Zira.ViewModels.Pages;

public sealed class SettingsPageViewModel : PageViewModel, ISingletonDependency
{
    public override int Index => int.MaxValue;
    public override string DisplayName => "Settings";
    public override LucideIconKind IconKind => LucideIconKind.Settings;
}
