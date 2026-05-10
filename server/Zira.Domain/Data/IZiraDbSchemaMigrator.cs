using System.Threading.Tasks;

namespace Zira.Data;

public interface IZiraDbSchemaMigrator
{
    Task MigrateAsync();
}
