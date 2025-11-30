using Zira.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Zira.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class ZiraController : AbpControllerBase
{
    protected ZiraController()
    {
        LocalizationResource = typeof(ZiraResource);
    }
}
