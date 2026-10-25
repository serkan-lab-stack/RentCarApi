using RentCar.Localization;
using Volo.Abp.Application.Services;

namespace RentCar;

/* Inherit your application services from this class.
 */
public abstract class RentCarAppService : ApplicationService
{
    protected RentCarAppService()
    {
        LocalizationResource = typeof(RentCarResource);
    }
}
