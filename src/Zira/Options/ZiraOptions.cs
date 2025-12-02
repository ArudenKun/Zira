using CommunityToolkit.Mvvm.ComponentModel;
using Zira.Utilities;

namespace Zira.Options;

[INotifyPropertyChanged]
public sealed partial class ZiraOptions : JsonFileBase
{
    public ZiraOptions()
        : base(AppHelper.SettingsPath) { }

    public LoggingOptions Logging { get; init; } = new();
}
