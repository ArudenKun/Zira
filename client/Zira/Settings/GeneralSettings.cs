using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Zira.Settings;

public sealed partial class GeneralSettings : ObservableValidator
{
    [ObservableProperty]
    [Url]
    public partial string Url { get; set; } = "https://localhost:5001";

    [ObservableProperty]
    public partial bool IsFirstTimeRun { get; set; } = true;
}
