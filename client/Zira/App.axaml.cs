using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PleasantUI.ToolKit.Controls;
using R3;
using R3.ObservableEvents;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Zira.ViewModels;

namespace Zira;

public sealed class App : Application
{
    private IAbpApplicationWithInternalServiceProvider? _abpApplication;

    public IAbpApplicationWithInternalServiceProvider AbpApplication =>
        _abpApplication
        ?? throw new InvalidOperationException("AbpApplication has not been initialized");

    public static new App Current =>
        (App?)Application.Current
        ?? throw new NullReferenceException("Application has not been initialized");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            _abpApplication = AbpApplicationFactory.Create<ZiraModule>(options =>
            {
                options.UseAutofac();

                options.Services.AddObjectAccessor<TopLevel>();
                options.Services.AddSingleton(sp =>
                    sp.GetRequiredService<IObjectAccessor<TopLevel>>().Value
                    ?? throw new NullReferenceException("TopLevel has not been set")
                );
            });
            _abpApplication.Initialize();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow =
                    AbpApplication
                        .ServiceProvider.GetRequiredService<ViewLocator>()
                        .Build(
                            AbpApplication.ServiceProvider.GetRequiredService<MainWindowViewModel>()
                        ) as Window;

                AbpApplication
                    .ServiceProvider.GetRequiredService<ObjectAccessor<TopLevel>>()
                    .Value = desktop.MainWindow;

                var errorHandlingSubscription = SetupErrorHandling();

                desktop.Exit += (_, _) =>
                {
                    _abpApplication.Shutdown();
                    _abpApplication.Dispose();
                    _abpApplication = null;
                    errorHandlingSubscription.Dispose();
                };
            }

            DataTemplates.AddIfNotContains(
                AbpApplication.ServiceProvider.GetRequiredService<ViewLocator>()
            );

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception e)
        {
            if (_abpApplication is not null)
                AbpApplication
                    .ServiceProvider.GetRequiredService<ILogger<App>>()
                    .LogCritical(e, "Failed to start the application");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IDisposable SetupErrorHandling()
    {
        var builder = Disposable.CreateBuilder();
        Avalonia
            .Threading.Dispatcher.UIThread.Events()
            .UnhandledException.SubscribeAwait(
                async (args, _) => await ShowCrashDialogAsync(args.Exception, "UI")
            )
            .AddTo(ref builder);

        AppDomain
            .CurrentDomain.Events()
            .UnhandledException.SubscribeAwait(
                async (args, _) =>
                    await ShowCrashDialogAsync((Exception)args.ExceptionObject, "App")
            )
            .AddTo(ref builder);

        return builder.Build();
    }

    private async Task ShowCrashDialogAsync(Exception exception, string category)
    {
        var logger = AbpApplication
            .ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(category);
        var dialog = CrashReportDialog.FromException(exception, "Zira", "0.1.0");
        dialog
            .Events()
            .SaveReportRequested.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => dialog.CloseAsync());
        dialog
            .Events()
            .SendReportRequested.Subscribe(eventArgs =>
                eventArgs.ReportFailure?.Invoke("Report Sending is not yet implemented")
            );

        dialog.IncludeScreenshot = false;
        dialog.IsEmailRequired = false;
        dialog.ShowScreenshotTab = false;
        logger.LogError(exception, "Unhandled exception");
        await dialog.ShowAsync();
    }
}
