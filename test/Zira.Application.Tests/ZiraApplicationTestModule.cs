using Volo.Abp.Modularity;

namespace Zira;

[DependsOn(
    typeof(ZiraApplicationModule),
    typeof(ZiraDomainTestModule)
)]
public class ZiraApplicationTestModule : AbpModule
{

}
