using Volo.Abp.Modularity;

namespace RentCar;

[DependsOn(
    typeof(RentCarApplicationModule),
    typeof(RentCarDomainTestModule)
)]
public class RentCarApplicationTestModule : AbpModule
{

}
