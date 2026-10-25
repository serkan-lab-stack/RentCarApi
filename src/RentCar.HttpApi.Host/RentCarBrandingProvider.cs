using Microsoft.Extensions.Localization;
using RentCar.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace RentCar;

[Dependency(ReplaceServices = true)]
public class RentCarBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<RentCarResource> _localizer;

    public RentCarBrandingProvider(IStringLocalizer<RentCarResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];

    public override string? LogoUrl => "~/images/logo/leptonxlite/logo-light-thumbnail.png";

    public override string? LogoIconUrl => "~/images/logo/leptonxlite/logo-dark-thumbnail.png";
}
