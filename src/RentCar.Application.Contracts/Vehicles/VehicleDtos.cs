using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace RentCar.Vehicles;

public class VehicleDto : EntityDto<Guid>
{
    public string Brand { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public int LastMaintenanceMileage { get; set; }
    public int MaintenanceIntervalKm { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal ExtraKmPrice { get; set; }
    public int DailyKmLimit { get; set; }
    public VehicleStatus Status { get; set; }
}

public abstract class VehicleInputBase
{
    [Required]
    [StringLength(VehicleConsts.MaxBrandLength)]
    public string Brand { get; set; } = default!;

    [Required]
    [StringLength(VehicleConsts.MaxModelLength)]
    public string Model { get; set; } = default!;

    [Range(VehicleConsts.MinYear, 2100)]
    public int Year { get; set; }

    [Range(0.01, 1_000_000)]
    public decimal DailyPrice { get; set; }

    [Range(0, 100_000)]
    public decimal ExtraKmPrice { get; set; }

    [Range(1, 5_000)]
    public int DailyKmLimit { get; set; }

    [Range(1, 1_000_000)]
    public int MaintenanceIntervalKm { get; set; } = VehicleConsts.DefaultMaintenanceIntervalKm;
}

public class CreateVehicleDto : VehicleInputBase
{
    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }
}

public class UpdateVehicleDto : VehicleInputBase
{
}

public class GetVehicleListInput : PagedResultRequestDto
{
    /// <summary>Searches in brand and model.</summary>
    public string? Filter { get; set; }

    public VehicleStatus? Status { get; set; }
}
