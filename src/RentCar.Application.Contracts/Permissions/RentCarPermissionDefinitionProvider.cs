using RentCar.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace RentCar.Permissions;

public class RentCarPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(RentCarPermissions.GroupName, L("Permission:RentCar"));

        var vehicles = group.AddPermission(RentCarPermissions.Vehicles.Default, L("Permission:Vehicles"));
        vehicles.AddChild(RentCarPermissions.Vehicles.Create, L("Permission:Vehicles.Create"));
        vehicles.AddChild(RentCarPermissions.Vehicles.Edit, L("Permission:Vehicles.Edit"));
        vehicles.AddChild(RentCarPermissions.Vehicles.Delete, L("Permission:Vehicles.Delete"));

        var rentals = group.AddPermission(RentCarPermissions.Rentals.Default, L("Permission:Rentals"));
        rentals.AddChild(RentCarPermissions.Rentals.Manage, L("Permission:Rentals.Manage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<RentCarResource>(name);
    }
}
