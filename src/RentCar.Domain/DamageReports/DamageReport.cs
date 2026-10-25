using System;
using RentCar.Rentals;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace RentCar.DamageReports;

public class DamageReport : FullAuditedAggregateRoot<Guid>
{
    public Guid RentalId { get; private set; }
    public Guid VehicleId { get; private set; }

    /// <summary>The customer of the rental, who is charged for the damage.</summary>
    public Guid UserId { get; private set; }

    public string Description { get; private set; } = default!;

    public DamageSeverity Severity { get; private set; }

    public decimal DamageCost { get; private set; }

    public bool IsPaid { get; private set; }

    public DateTime? PaidAt { get; private set; }

    private DamageReport()
    {
        // Required by EF Core
    }

    public DamageReport(Guid id, Rental rental, string description, DamageSeverity severity) : base(id)
    {
        RentalId = rental.Id;
        VehicleId = rental.VehicleId;
        UserId = rental.UserId;

        Description = Check.NotNullOrWhiteSpace(description, nameof(description), DamageReportConsts.MaxDescriptionLength);
        Severity = severity;

        // The price snapshot of the rental is used, so later price changes do not matter
        DamageCost = CalculateCost(severity, rental.DailyPrice);
    }

    public static decimal CalculateCost(DamageSeverity severity, decimal dailyPrice)
    {
        var multiplier = severity switch
        {
            DamageSeverity.Minor => DamageReportConsts.MinorMultiplier,
            DamageSeverity.Moderate => DamageReportConsts.ModerateMultiplier,
            DamageSeverity.Major => DamageReportConsts.MajorMultiplier,
            _ => throw new ArgumentOutOfRangeException(nameof(severity))
        };

        return Math.Round(dailyPrice * multiplier, 2);
    }

    /// <summary>Full payment only: the amount must be exactly the damage cost.</summary>
    public void Pay(decimal amount, DateTime paidAt)
    {
        if (IsPaid)
        {
            throw new BusinessException(RentCarDomainErrorCodes.DamageReportAlreadyPaid)
                .WithData("DamageReportId", Id);
        }

        if (amount != DamageCost)
        {
            throw new BusinessException(RentCarDomainErrorCodes.InvalidPaymentAmount)
                .WithData("Amount", amount)
                .WithData("DamageCost", DamageCost);
        }

        IsPaid = true;
        PaidAt = paidAt;
    }
}
