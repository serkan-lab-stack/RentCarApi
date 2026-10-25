namespace RentCar;

public static class RentCarDomainErrorCodes
{
    // Vehicles
    public const string VehicleNotAvailable = "RentCar:00001";
    public const string VehicleNotRented = "RentCar:00002";
    public const string VehicleIsRented = "RentCar:00003";
    public const string InvalidMileage = "RentCar:00004";
    public const string VehicleNotInMaintenance = "RentCar:00005";

    // Rentals
    public const string LicenseRequired = "RentCar:00006";
    public const string LicenseExpiresBeforeRentalEnd = "RentCar:00007";
    public const string RentalNotActive = "RentCar:00008";
    public const string RentalAccessDenied = "RentCar:00009";
    public const string RentalOverdue = "RentCar:00010";
    public const string InvalidReturnDate = "RentCar:00011";

    // Damage reports
    public const string DamageReportAlreadyPaid = "RentCar:00012";
    public const string InvalidPaymentAmount = "RentCar:00013";
    public const string DamageReportAccessDenied = "RentCar:00014";
}
