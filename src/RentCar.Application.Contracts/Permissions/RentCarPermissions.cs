namespace RentCar.Permissions;

public static class RentCarPermissions
{
    public const string GroupName = "RentCar";

    public static class Vehicles
    {
        public const string Default = GroupName + ".Vehicles";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Rentals
    {
        public const string Default = GroupName + ".Rentals";

        /// <summary>See all rentals and return a rental on behalf of a customer (back office).</summary>
        public const string Manage = Default + ".Manage";
    }
}
