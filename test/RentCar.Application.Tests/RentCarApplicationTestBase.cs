using Volo.Abp.Modularity;

namespace RentCar;

public abstract class RentCarApplicationTestBase<TStartupModule> : RentCarTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
