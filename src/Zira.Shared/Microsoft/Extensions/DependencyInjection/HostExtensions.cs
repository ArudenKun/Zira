using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace Microsoft.Extensions.DependencyInjection;

public static class HostExtensions
{
    public static async Task InitializeApplicationAsync(this IHost app)
    {
        Check.NotNull(app, nameof(app));

        app.Services.GetRequiredService<ObjectAccessor<IHost>>().Value = app;
        var application =
            app.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();
        var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        applicationLifetime.ApplicationStopping.Register(() =>
        {
            AsyncHelper.RunSync(() => application.ShutdownAsync());
        });

        applicationLifetime.ApplicationStopped.Register(() =>
        {
            application.Dispose();
        });

        await application.InitializeAsync(app.Services);
    }

    public static void InitializeApplication(this IHost app)
    {
        Check.NotNull(app, nameof(app));

        app.Services.GetRequiredService<ObjectAccessor<IHost>>().Value = app;
        var application =
            app.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();
        var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        applicationLifetime.ApplicationStopping.Register(() =>
        {
            application.Shutdown();
        });

        applicationLifetime.ApplicationStopped.Register(() =>
        {
            application.Dispose();
        });

        application.Initialize(app.Services);
    }
}
