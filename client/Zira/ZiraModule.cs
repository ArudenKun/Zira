using Volo.Abp.Modularity;

namespace Zira;

[DependsOn(typeof(ZiraHttpApiClientModule))]
public class ZiraModule : AbpModule { }
