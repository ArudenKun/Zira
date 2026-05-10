using Volo.Abp.Modularity;

namespace Zira;

/* Inherit from this class for your domain layer tests. */
public abstract class ZiraDomainTestBase<TStartupModule> : ZiraTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
