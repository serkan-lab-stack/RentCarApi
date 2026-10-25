namespace RentCar.DamageReports;

public enum DamageSeverity
{
    /// <summary>Scratches, small dents.</summary>
    Minor = 1,

    /// <summary>Broken parts, larger dents.</summary>
    Moderate = 2,

    /// <summary>Serious damage; the vehicle must go to maintenance.</summary>
    Major = 3
}
