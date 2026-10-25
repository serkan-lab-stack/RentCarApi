using RentCar.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace RentCar.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(RentCarEntityFrameworkCoreModule),
    typeof(RentCarApplicationContractsModule)
)]
public class RentCarDbMigratorModule : AbpModule
{
}
