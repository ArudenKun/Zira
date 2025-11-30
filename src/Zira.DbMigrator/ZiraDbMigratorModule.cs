using Zira.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Zira.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ZiraEntityFrameworkCoreModule),
    typeof(ZiraApplicationContractsModule)
)]
public class ZiraDbMigratorModule : AbpModule
{
}
