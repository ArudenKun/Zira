using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Zira.Models.EventData;

namespace Zira.ViewModels;

public sealed partial class MainWindowViewModel
    : ViewModel,
        ILocalEventHandler<SplashViewFinishedEventData>,
        ILocalEventHandler<LoginSuccessEventData>,
        ISingletonDependency
{
    public MainWindowViewModel(ISukiToastManager toastManager, ISukiDialogManager dialogManager)
    {
        ToastManager = toastManager;
        DialogManager = dialogManager;
    }

    public ISukiToastManager ToastManager { get; }
    public ISukiDialogManager DialogManager { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainView))]
    [NotifyCanExecuteChangedFor(nameof(ShowPageCommand))]
    public partial ViewModel ContentViewModel { get; set; } = null!;

    public bool IsMainView => ContentViewModel is MainViewModel;

    public override void OnLoaded()
    {
        ContentViewModel = LazyServiceProvider.LazyGetRequiredService<SplashViewModel>();
    }

    [RelayCommand(CanExecute = nameof(IsMainView))]
    private async Task ShowPage(Type pageType)
    {
        await LocalEventBus.PublishAsync(new ShowPageEventData(pageType));
    }

    public Task HandleEventAsync(SplashViewFinishedEventData eventData)
    {
        ContentViewModel = LazyServiceProvider.LazyGetRequiredService<LoginViewModel>();
        return Task.CompletedTask;
    }

    public Task HandleEventAsync(LoginSuccessEventData eventData)
    {
        ContentViewModel = LazyServiceProvider.LazyGetRequiredService<MainViewModel>();
        return Task.CompletedTask;
    }
}
