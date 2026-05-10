using Volo.Abp.Modularity;

namespace Zira;

[DependsOn(
    typeof(ZiraDomainModule),
    typeof(ZiraTestBaseModule)
)]
public class ZiraDomainTestModule : AbpModule
{

}
