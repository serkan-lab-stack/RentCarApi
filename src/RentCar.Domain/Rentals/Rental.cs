using System;
using RentCar.Vehicles;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace RentCar.Rentals;

public class Rental : FullAuditedAggregateRoot<Guid>
{
    public Guid VehicleId { get; private set; }
    public Guid UserId { get; private set; }

    public DateTime StartDate { get; private set; }

    /// <summary>Agreed rental length in days (grows when the rental is extended).</summary>
    public int RentalDays { get; private set; }

    public DateTime PlannedEndDate { get; private set; }

    public DateTime? ReturnDate { get; private set; }

    public int StartMileage { get; private set; }
    public int? EndMileage { get; private set; }

    // Prices are copied from the vehicle at rental time, so later price changes do not affect this rental.
    public decimal DailyPrice { get; private set; }
    public decimal ExtraKmPrice { get; private set; }
    public int DailyKmLimit { get; private set; }

    public decimal BasePrice { get; private set; }
    public int ExtraKm { get; private set; }
    public decimal ExtraKmFee { get; private set; }
    public int LateDays { get; private set; }
    public decimal LateFee { get; private set; }

    /// <summary>An estimate (base price) while the rental is active; the final price after return.</summary>
    public decimal TotalPrice { get; private set; }

    public RentalStatus Status { get; private set; }

    private Rental()
    {
        // Required by EF Core
    }

    public Rental(Guid id, Guid userId, Vehicle vehicle, DateTime startDate, int rentalDays) : base(id)
    {
        Check.Range(rentalDays, nameof(rentalDays), 1, RentalConsts.MaxRentalDays);

        UserId = userId;
        VehicleId = vehicle.Id;
        StartDate = startDate;
        RentalDays = rentalDays;
        PlannedEndDate = startDate.AddDays(rentalDays);

        StartMileage = vehicle.Mileage;
        DailyPrice = vehicle.DailyPrice;
        ExtraKmPrice = vehicle.ExtraKmPrice;
        DailyKmLimit = vehicle.DailyKmLimit;

        Status = RentalStatus.Active;
        CalculateEstimate();
    }

    public void Extend(int additionalDays, DateTime now)
    {
        EnsureActive();

        if (additionalDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(additionalDays));
        }

        // Extending after the end date would be a way to dodge the late fee.
        if (now > PlannedEndDate)
        {
            throw new BusinessException(RentCarDomainErrorCodes.RentalOverdue)
                .WithData("RentalId", Id);
        }

        RentalDays += additionalDays;
        PlannedEndDate = StartDate.AddDays(RentalDays);
        CalculateEstimate();
    }

    /// <summary>
    /// Finishes the rental and calculates the final price:
    /// base price + extra km fee + late fee.
    /// </summary>
    public void Complete(DateTime returnDate, int endMileage)
    {
        EnsureActive();

        if (returnDate < StartDate)
        {
            throw new BusinessException(RentCarDomainErrorCodes.InvalidReturnDate)
                .WithData("RentalId", Id);
        }

        if (endMileage < StartMileage)
        {
            throw new BusinessException(RentCarDomainErrorCodes.InvalidMileage)
                .WithData("CurrentMileage", StartMileage)
                .WithData("EndMileage", endMileage);
        }

        BasePrice = Math.Round(RentalDays * DailyPrice, 2);

        // Extra km: kilometers above the free allowance (days x daily km limit)
        var drivenKm = endMileage - StartMileage;
        var includedKm = RentalDays * DailyKmLimit;
        ExtraKm = Math.Max(0, drivenKm - includedKm);
        ExtraKmFee = Math.Round(ExtraKm * ExtraKmPrice, 2);

        // Late fee: every started late day is charged (daily price x multiplier)
        var lateTicks = (returnDate - PlannedEndDate).Ticks;
        LateDays = lateTicks > 0
            ? (int)((lateTicks + TimeSpan.TicksPerDay - 1) / TimeSpan.TicksPerDay)
            : 0;
        LateFee = Math.Round(LateDays * DailyPrice * RentalConsts.LateFeeMultiplier, 2);

        TotalPrice = BasePrice + ExtraKmFee + LateFee;

        ReturnDate = returnDate;
        EndMileage = endMileage;
        Status = RentalStatus.Returned;
    }

    private void CalculateEstimate()
    {
        BasePrice = Math.Round(RentalDays * DailyPrice, 2);
        TotalPrice = BasePrice;
    }

    private void EnsureActive()
    {
        if (Status != RentalStatus.Active)
        {
            throw new BusinessException(RentCarDomainErrorCodes.RentalNotActive)
                .WithData("RentalId", Id);
        }
    }
}
