using System;
using System.Text.Json.Serialization;
using System.Threading;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Zira.Settings;

[ObservableObject]
[ObservableRecipient]
public sealed partial class SettingsManager : SettingsBase
{
    private static readonly Lazy<SettingsManager> LazyInstance = new(
        () => new SettingsManager(),
        LazyThreadSafetyMode.ExecutionAndPublication
    );

    public static SettingsManager Instance => LazyInstance.Value;

    private SettingsManager()
        : base(ZiraConsts.Name, SettingsManagerJsonSerializerContext.Default.Options) =>
        Messenger = WeakReferenceMessenger.Default;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial GeneralSettings General { get; set; } = new();

    [JsonSerializable(typeof(SettingsManager))]
    [JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
    private sealed partial class SettingsManagerJsonSerializerContext : JsonSerializerContext;
}
