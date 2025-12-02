using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using R3;
using R3.ObservableEvents;
using Volo.Abp.DependencyInjection;
using Zira.Services;
using Zira.Utilities;
using Zira.ViewModels;
using ZLinq;

namespace Zira;

public sealed class App : Application, IDisposable, ISingletonDependency
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ViewLocator _viewLocator;
    private readonly IToastService _toastService;
    private readonly ILoggerFactory _loggerFactory;

    private IDisposable? _subscriptions;

    public App(
        MainWindowViewModel mainWindowViewModel,
        ViewLocator viewLocator,
        IToastService toastService,
        ILoggerFactory loggerFactory
    )
    {
        _mainWindowViewModel = mainWindowViewModel;
        _viewLocator = viewLocator;
        _toastService = toastService;
        _loggerFactory = loggerFactory;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        DataTemplates.Add(_viewLocator);

        _subscriptions = Disposable.Combine(
            AppDomain
                .CurrentDomain.Events()
                .UnhandledException.Subscribe(e =>
                    HandleUnhandledException((Exception)e.ExceptionObject, "App")
                ),
            RxEvents.TaskSchedulerUnobservedTaskException.Subscribe(e =>
            {
                var logger = _loggerFactory.CreateLogger("Task");
                try
                {
                    HandleUnhandledException(e.Exception, "Task");
                    e.SetObserved();
                }
                catch (Exception exception)
                {
                    logger.LogException(exception, LogLevel.Error);
                    e.SetObserved();
                }
            }),
            Dispatcher
                .UIThread.Events()
                .UnhandledException.Subscribe(e =>
                {
                    HandleUnhandledException(e.Exception, "UI");
                    e.Handled = true;
                })
        );
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = _viewLocator.CreateView(_mainWindowViewModel) as Window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }

    private void HandleUnhandledException(Exception exception, string category)
    {
        var logger = _loggerFactory.CreateLogger(category);
        logger.LogError(exception, "Unhandled Exception");
        DispatchHelper.Invoke(() =>
            _toastService.ShowExceptionToast(exception, $"{category} Exception")
        );
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.AsValueEnumerable()
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
