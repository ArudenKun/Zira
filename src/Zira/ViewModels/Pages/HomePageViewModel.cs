using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lucide.Avalonia;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Zira.Models.EventData;

namespace Zira.ViewModels.Pages;

public sealed partial class HomePageViewModel : PageViewModel, ISingletonDependency
{
    public override int Index => 1;
    public override string DisplayName => "Home";
    public override LucideIconKind IconKind => LucideIconKind.House;

    [ObservableProperty]
    public partial string AccessToken { get; set; } = string.Empty;

    public override void OnLoaded()
    {
        var profile = AsyncHelper.RunSync(async () =>
            await AuthenticationManager.GetUserProfileAsync()
        );
        AccessToken = profile.UserName;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await AuthenticationManager.LogoutAsync();
        await LocalEventBus.PublishAsync(new LogoutEventData());
    }
}
