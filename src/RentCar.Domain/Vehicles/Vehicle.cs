using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace RentCar.Vehicles;

public class Vehicle : FullAuditedAggregateRoot<Guid>
{
    public string Brand { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public int Year { get; private set; }

    /// <summary>Current odometer value in km.</summary>
    public int Mileage { get; private set; }

    /// <summary>Odometer value at the last maintenance.</summary>
    public int LastMaintenanceMileage { get; private set; }

    /// <summary>The vehicle goes to maintenance when (Mileage - LastMaintenanceMileage) reaches this value.</summary>
    public int MaintenanceIntervalKm { get; private set; }

    public decimal DailyPrice { get; private set; }

    /// <summary>Price per km that exceeds the daily km limit.</summary>
    public decimal ExtraKmPrice { get; private set; }

    /// <summary>Free km per rental day.</summary>
    public int DailyKmLimit { get; private set; }

    public VehicleStatus Status { get; private set; }

    private Vehicle()
    {
        // Required by EF Core
    }

    public Vehicle(
        Guid id,
        string brand,
        string model,
        int year,
        int mileage,
        decimal dailyPrice,
        decimal extraKmPrice,
        int dailyKmLimit,
        int maintenanceIntervalKm) : base(id)
    {
        if (mileage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mileage));
        }

        SetDetails(brand, model, year, dailyPrice, extraKmPrice, dailyKmLimit, maintenanceIntervalKm);

        Mileage = mileage;
        LastMaintenanceMileage = mileage; // a new vehicle is assumed to be freshly serviced
        Status = VehicleStatus.Available;
    }

    public void Update(
        string brand,
        string model,
        int year,
        decimal dailyPrice,
        decimal extraKmPrice,
        int dailyKmLimit,
        int maintenanceIntervalKm)
    {
        SetDetails(brand, model, year, dailyPrice, extraKmPrice, dailyKmLimit, maintenanceIntervalKm);

        // A lowered maintenance interval may make an available vehicle due for maintenance.
        if (Status == VehicleStatus.Available && IsMaintenanceDue())
        {
            Status = VehicleStatus.Maintenance;
        }
    }

    public bool IsMaintenanceDue()
    {
        return Mileage - LastMaintenanceMileage >= MaintenanceIntervalKm;
    }

    /// <summary>Called by the rental flow when a vehicle is handed over to a customer.</summary>
    public void MarkAsRented()
    {
        if (Status != VehicleStatus.Available)
        {
            throw new BusinessException(RentCarDomainErrorCodes.VehicleNotAvailable)
                .WithData("Brand", Brand)
                .WithData("Model", Model);
        }

        Status = VehicleStatus.Rented;
    }

    /// <summary>
    /// Called by the rental flow when a vehicle is returned.
    /// Updates the odometer and automatically sends the vehicle to maintenance if the interval is reached.
    /// </summary>
    public void ReturnFromRental(int endMileage)
    {
        if (Status != VehicleStatus.Rented)
        {
            throw new BusinessException(RentCarDomainErrorCodes.VehicleNotRented)
                .WithData("Brand", Brand)
                .WithData("Model", Model);
        }

        if (endMileage < Mileage)
        {
            throw new BusinessException(RentCarDomainErrorCodes.InvalidMileage)
                .WithData("CurrentMileage", Mileage)
                .WithData("EndMileage", endMileage);
        }

        Mileage = endMileage;
        Status = IsMaintenanceDue() ? VehicleStatus.Maintenance : VehicleStatus.Available;
    }

    public void SendToMaintenance()
    {
        EnsureNotRented();
        Status = VehicleStatus.Maintenance;
    }

    public void CompleteMaintenance()
    {
        if (Status != VehicleStatus.Maintenance)
        {
            throw new BusinessException(RentCarDomainErrorCodes.VehicleNotInMaintenance)
                .WithData("Brand", Brand)
                .WithData("Model", Model);
        }

        LastMaintenanceMileage = Mileage;
        Status = VehicleStatus.Available;
    }

    public void EnsureNotRented()
    {
        if (Status == VehicleStatus.Rented)
        {
            throw new BusinessException(RentCarDomainErrorCodes.VehicleIsRented)
                .WithData("Brand", Brand)
                .WithData("Model", Model);
        }
    }

    private void SetDetails(
        string brand,
        string model,
        int year,
        decimal dailyPrice,
        decimal extraKmPrice,
        int dailyKmLimit,
        int maintenanceIntervalKm)
    {
        Brand = Check.NotNullOrWhiteSpace(brand, nameof(brand), VehicleConsts.MaxBrandLength);
        Model = Check.NotNullOrWhiteSpace(model, nameof(model), VehicleConsts.MaxModelLength);

        Check.Range(year, nameof(year), VehicleConsts.MinYear, DateTime.UtcNow.Year + 1);
        Year = year;

        if (dailyPrice <= 0) throw new ArgumentOutOfRangeException(nameof(dailyPrice));
        if (extraKmPrice < 0) throw new ArgumentOutOfRangeException(nameof(extraKmPrice));
        if (dailyKmLimit <= 0) throw new ArgumentOutOfRangeException(nameof(dailyKmLimit));
        if (maintenanceIntervalKm <= 0) throw new ArgumentOutOfRangeException(nameof(maintenanceIntervalKm));

        DailyPrice = dailyPrice;
        ExtraKmPrice = extraKmPrice;
        DailyKmLimit = dailyKmLimit;
        MaintenanceIntervalKm = maintenanceIntervalKm;
    }
}
