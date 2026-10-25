using RentCar.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace RentCar.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class RentCarController : AbpControllerBase
{
    protected RentCarController()
    {
        LocalizationResource = typeof(RentCarResource);
    }
}
