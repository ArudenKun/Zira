using Volo.Abp.Modularity;

namespace Zira;

public abstract class ZiraApplicationTestBase<TStartupModule> : ZiraTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
