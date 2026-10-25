using Volo.Abp.Modularity;

namespace RentCar;

/* Inherit from this class for your domain layer tests. */
public abstract class RentCarDomainTestBase<TStartupModule> : RentCarTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
