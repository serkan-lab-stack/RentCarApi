using Volo.Abp.Settings;

namespace RentCar.Settings;

public class RentCarSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(RentCarSettings.MySetting1));
    }
}
