using Avalonia;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Velopack;
using Volo.Abp.Threading;
using Zira.Extensions;
using Zira.Options;
using Zira.Utilities;

namespace Zira;

public static class Program
{
    private static readonly Lazy<IHost> LazyVisualDesignerHost = new(() =>
    {
        var builder = Host.CreateApplicationBuilder();
        AsyncHelper.RunSync(() => ConfigureAsync(builder));
        var app = builder.Build();
        AsyncHelper.RunSync(() => app.InitializeAsync());
        return app;
    });

    private static IHost VisualDesignerHost => LazyVisualDesignerHost.Value;

    private static ILogger Logger => Log.ForContext("SourceContext", "Zira");

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.WithDemystifiedStackTraces()
            .WriteTo.Async(c =>
                c.File(
                    AppHelper.LogsDir.CombinePath("log.txt"),
                    outputTemplate: LoggingOptions.Template
                )
            )
            .WriteTo.Async(c => c.Console(outputTemplate: LoggingOptions.Template))
            .CreateBootstrapLogger();

        try
        {
            Logger.Information("Starting Avalonia host.");
            VelopackApp.Build().SetArgs(args).SetLogger(VelopackLogger.Instance).Run();
            var builder = Host.CreateApplicationBuilder(args);
            await ConfigureAsync(builder);
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task ConfigureAsync(HostApplicationBuilder builder)
    {
        builder.Configuration.AddConfiguration(
            ConfigurationHelper.BuildConfiguration(
                new AbpConfigurationBuilderOptions { BasePath = AppHelper.DataDir }
            )
        );
        builder.Configuration.AddAppSettingsSecretsJson();
        builder.AddAvaloniaHosting<App>(appBuilder =>
            appBuilder
                .UsePlatformDetect()
                .UseR3(exception => Logger.Fatal(exception, "R3 Unhandled Exception"))
                .LogToTrace()
        );
        builder.AddAutofac();
        await builder.AddApplicationAsync<ZiraModule>();
        builder.Services.AddSingleton(sp => new LoggingLevelSwitch(
            sp.GetRequiredService<IOptions<LoggingOptions>>().Value.LogEventLevel
        ));
        builder.Services.AddSerilog(
            (sp, loggingConfiguration) =>
                loggingConfiguration
                    .MinimumLevel.ControlledBy(sp.GetRequiredService<LoggingLevelSwitch>())
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithDemystifiedStackTraces()
                    .WriteTo.Async(c =>
                        c.File(
                            AppHelper.LogsDir.CombinePath("log.txt"),
                            outputTemplate: LoggingOptions.Template,
                            fileSizeLimitBytes: sp.GetRequiredService<
                                IOptions<LoggingOptions>
                            >().Value.Size == 0
                                ? null
                                : (long)
                                    sp.GetRequiredService<IOptions<LoggingOptions>>()
                                        .Value.Size.Megabytes()
                                        .Bytes,
                            retainedFileTimeLimit: 30.Days(),
                            rollingInterval: RollingInterval.Day,
                            rollOnFileSizeLimit: true,
                            shared: true
                        )
                    )
                    .WriteTo.Async(c => c.Console(outputTemplate: LoggingOptions.Template))
        );
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    // ReSharper disable once UnusedMember.Global
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure(() => VisualDesignerHost.Services.GetRequiredService<App>())
            .UsePlatformDetect()
            .UseR3()
            .LogToTrace();
}
