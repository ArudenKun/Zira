using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Zira.Data;

/* This is used if database provider does't define
 * IZiraDbSchemaMigrator implementation.
 */
public class NullZiraDbSchemaMigrator : IZiraDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
