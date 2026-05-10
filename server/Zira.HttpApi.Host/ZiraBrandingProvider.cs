using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;
using Zira.Localization;

namespace Zira;

[Dependency(ReplaceServices = true)]
public class ZiraBrandingProvider : DefaultBrandingProvider
{
    private readonly IStringLocalizer<ZiraResource> _localizer;

    public ZiraBrandingProvider(IStringLocalizer<ZiraResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
