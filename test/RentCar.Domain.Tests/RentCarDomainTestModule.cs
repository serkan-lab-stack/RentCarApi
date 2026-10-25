using Volo.Abp.Modularity;

namespace RentCar;

[DependsOn(
    typeof(RentCarDomainModule),
    typeof(RentCarTestBaseModule)
)]
public class RentCarDomainTestModule : AbpModule
{

}
