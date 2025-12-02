using System.Diagnostics;
using AutoInterfaceAttributes;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Zira.Services;

namespace Zira.ViewModels;

[AutoInterface(Inheritance = [typeof(IDisposable)])]
public abstract partial class ViewModel : ObservableValidator, IViewModel
{
    public required IAbpLazyServiceProvider LazyServiceProvider { protected get; init; }

    protected ILoggerFactory LoggerFactory =>
        LazyServiceProvider.LazyGetRequiredService<ILoggerFactory>();

    protected ILogger Logger =>
        LazyServiceProvider.LazyGetService<ILogger>(_ =>
            LoggerFactory.CreateLogger(GetType().FullName!)
        );

    public ILocalEventBus LocalEventBus =>
        LazyServiceProvider.LazyGetRequiredService<ILocalEventBus>();

    public IAuthenticationManager AuthenticationManager =>
        LazyServiceProvider.LazyGetRequiredService<IAuthenticationManager>();

    public IToastService ToastService =>
        LazyServiceProvider.LazyGetRequiredService<IToastService>();

    public IDialogService DialogService =>
        LazyServiceProvider.LazyGetRequiredService<IDialogService>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string IsBusyText { get; set; } = string.Empty;

    public bool IsNotBusy => !IsBusy;

    public virtual void OnLoaded() { }

    public virtual void OnUnloaded() { }

    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    public async Task SetBusyAsync(Func<Task> func, string busyText = "", bool showException = true)
    {
        IsBusy = true;
        IsBusyText = busyText;
        try
        {
            await func();
        }
        catch (Exception ex) when (LogException(ex, true, showException)) { }
        finally
        {
            IsBusy = false;
            IsBusyText = string.Empty;
        }
    }

    public bool LogException(Exception? ex, bool shouldCatch = false, bool shouldDisplay = false)
    {
        if (ex is null)
        {
            return shouldCatch;
        }

        Logger.LogException(ex);
        if (shouldDisplay)
        {
            ToastService.ShowExceptionToast(ex, "Error", ex.ToStringDemystified());
        }

        return shouldCatch;
    }

    #region Disposal

    ~ViewModel() => Dispose(false);

    private List<Action?>? _onDisposeActions;

    public void OnDispose(Action? callback = null)
    {
        _onDisposeActions ??= [];
        _onDisposeActions.Add(callback);
    }

    private bool _isDisposed;

    /// <inheritdoc cref="Dispose"/>>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (!disposing)
        {
            return;
        }

        if (_onDisposeActions is { Count: > 0 })
        {
            foreach (var disposeAction in _onDisposeActions.Distinct())
            {
                disposeAction?.Invoke();
            }
        }

        _isDisposed = true;
    }

    /// <inheritdoc />>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
