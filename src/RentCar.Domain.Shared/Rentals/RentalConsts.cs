namespace RentCar.Rentals;

public static class RentalConsts
{
    /// <summary>Maximum number of days for a single rental request.</summary>
    public const int MaxRentalDays = 90;

    /// <summary>Each late day is charged as: daily price x this multiplier.</summary>
    public const decimal LateFeeMultiplier = 1.5m;
}
