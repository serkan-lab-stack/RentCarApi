using System.Threading.Tasks;

namespace RentCar.Data;

public interface IRentCarDbSchemaMigrator
{
    Task MigrateAsync();
}
