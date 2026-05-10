using Zira.Localization;
using Volo.Abp.Application.Services;

namespace Zira;

/* Inherit your application services from this class.
 */
public abstract class ZiraAppService : ApplicationService
{
    protected ZiraAppService()
    {
        LocalizationResource = typeof(ZiraResource);
    }
}
