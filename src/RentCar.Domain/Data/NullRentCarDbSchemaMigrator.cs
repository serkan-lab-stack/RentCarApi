using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace RentCar.Data;

/* This is used if database provider does't define
 * IRentCarDbSchemaMigrator implementation.
 */
public class NullRentCarDbSchemaMigrator : IRentCarDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
