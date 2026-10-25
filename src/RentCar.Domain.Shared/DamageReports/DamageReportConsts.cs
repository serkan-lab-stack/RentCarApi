namespace RentCar.DamageReports;

public static class DamageReportConsts
{
    public const int MaxDescriptionLength = 1000;

    // Damage cost = daily price of the rental x the multiplier of the severity
    public const decimal MinorMultiplier = 1m;
    public const decimal ModerateMultiplier = 3m;
    public const decimal MajorMultiplier = 10m;
}
