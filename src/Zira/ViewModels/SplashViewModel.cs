using CommunityToolkit.Mvvm.ComponentModel;
using Humanizer;
using Volo.Abp.DependencyInjection;
using Zira.Models.EventData;

namespace Zira.ViewModels;

public sealed partial class SplashViewModel : ViewModel, ITransientDependency
{
    [ObservableProperty]
    public partial string StatusText { get; set; } = "Initializing";

    public override async void OnLoaded()
    {
        await Task.Delay(1.Seconds());
        StatusText = "Loading Settings";
        await Task.Delay(200.Milliseconds());
        await LocalEventBus.PublishAsync(new SplashViewFinishedEventData());
    }
}
