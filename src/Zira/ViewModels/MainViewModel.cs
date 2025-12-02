using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Zira.Models.EventData;
using Zira.ViewModels.Pages;
using ZLinq;

namespace Zira.ViewModels;

public sealed partial class MainViewModel
    : ViewModel,
        ILocalEventHandler<ShowPageEventData>,
        ISingletonDependency
{
    public MainViewModel(IEnumerable<IPageViewModel> pageViewModels)
    {
        Pages = new AvaloniaList<PageViewModel>(
            pageViewModels.AsValueEnumerable().OrderBy(x => x.Index).Cast<PageViewModel>().ToArray()
        );

        Page = Pages.AsValueEnumerable().First(x => x is SettingPageViewModel);
    }

    public IAvaloniaReadOnlyList<PageViewModel> Pages { get; }

    [ObservableProperty]
    public partial PageViewModel Page { get; set; }

    public Task HandleEventAsync(ShowPageEventData eventData)
    {
        Page = Pages.AsValueEnumerable().First(x => x.GetType() == eventData.ViewModelType);
        return Task.CompletedTask;
    }
}
