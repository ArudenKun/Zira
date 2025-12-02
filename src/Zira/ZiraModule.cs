using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using Volo.Abp.Autofac;
using Volo.Abp.Http.Client;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace Zira;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ZiraHttpApiClientModule),
    typeof(AbpHttpClientIdentityModelModule)
)]
public sealed class ZiraModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpHttpClientBuilderOptions>(options =>
        {
            options.ProxyClientBuildActions.Add(
                (_, clientBuilder) =>
                {
                    clientBuilder.AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3,
                            i => TimeSpan.FromSeconds(Math.Pow(2, i))
                        )
                    );
                }
            );
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ISukiToastManager, SukiToastManager>();
        context.Services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        context.Services.AddObjectAccessor<IHost>();
    }
}
