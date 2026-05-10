using System;
using System.Threading.Tasks;
using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Zira.Settings;

namespace Zira.ViewModels;

[PublicAPI]
public abstract partial class ViewModel
    : ObservableValidator,
        IHasExtraProperties,
        IDisposable,
        IAsyncDisposable,
        ITransientDependency
{
    protected ViewModel()
    {
        ExtraProperties = new ExtraPropertyDictionary();
        SettingsManager = SettingsManager.Instance;

        this.SetDefaultsForExtraProperties();
    }

    public required IServiceProvider ServiceProvider { get; init; }
    public required ITransientCachedServiceProvider CachedServiceProvider { get; init; }

    public ExtraPropertyDictionary ExtraProperties { get; }

    protected SettingsManager SettingsManager { get; }

    protected ILoggerFactory LoggerFactory =>
        CachedServiceProvider.GetRequiredService<ILoggerFactory>();

    protected ILogger Logger =>
        CachedServiceProvider.GetService(LoggerFactory.CreateLogger(GetType().FullName!));

    protected IMessenger Messenger => CachedServiceProvider.GetRequiredService<IMessenger>();

    protected IRegionManager RegionManager =>
        field ??= CachedServiceProvider.GetRequiredService<IRegionManager>();

    // protected ISnackbarService SnackbarService =>
    //     ServiceProvider.GetRequiredService<ISnackbarService>();

    // protected IDialogService DialogService => ServiceProvider.GetRequiredService<IDialogService>();

    public GeneralSettings GeneralSettings => SettingsManager.General;

    // public AppearanceSettings AppearanceSettings => SettingsService.Get<AppearanceSettings>();

    // public LoggingSettings LoggingSettings => SettingsService.Get<LoggingSettings>();

    public TopLevel TopLevel => CachedServiceProvider.GetRequiredService<TopLevel>();

    public IStorageProvider StorageProvider => TopLevel.StorageProvider;

    public IClipboard Clipboard => TopLevel.Clipboard!;
    public ILauncher Launcher => TopLevel.Launcher;

    [ObservableProperty]
    public virtual partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string BusyText { get; set; } = string.Empty;

    public virtual void OnLoaded() { }

    public virtual void OnUnloaded() { }

    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    protected virtual async Task SetBusyAsync(
        Func<Task> func,
        string busyText = "",
        bool showException = true
    )
    {
        IsBusy = true;
        BusyText = busyText;
        try
        {
            await func();
        }
        catch (Exception ex) when (LogException(ex, true, showException))
        {
            // Not Used
        }
        finally
        {
            IsBusy = false;
            BusyText = string.Empty;
        }
    }

    protected bool LogException(Exception? ex, bool shouldCatch = false, bool shouldDisplay = false)
    {
        if (ex is null)
        {
            return shouldCatch;
        }

        Logger.LogException(ex);
        if (shouldDisplay)
        {
            // SnackbarService.ShowException(ex);
        }

        return shouldCatch;
    }

    #region Disposal

    private bool _disposed;

    public CompositeDisposable Disposables { get; } = new();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        // 1. Perform any asynchronous cleanup.
        await DisposeAsyncCore().ConfigureAwait(false);

        // 2. Perform synchronous cleanup.
        // Note: We use true here so derived classes that only override Dispose(bool)
        // still get their managed synchronous resources cleaned up properly.
        Dispose(true);

        // 3. Suppress finalization.
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose"/>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed state (managed objects).
            Disposables.Dispose();
        }

        // Free unmanaged resources here (if any derived class introduces them)
        // Set large fields to null if necessary

        _disposed = true;
    }

    /// <summary>
    /// Override this method to asynchronously dispose of managed resources.
    /// </summary>
    protected virtual ValueTask DisposeAsyncCore()
    {
        // For the base ViewModel, there are no strictly async resources to dispose,
        // but we provide the hook for derived classes.
        return default;
    }

    #endregion
}
