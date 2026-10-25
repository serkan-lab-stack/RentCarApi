using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace RentCar.Rentals;

public class RentalDto : EntityDto<Guid>
{
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartDate { get; set; }
    public int RentalDays { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int StartMileage { get; set; }
    public int? EndMileage { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal ExtraKmPrice { get; set; }
    public int DailyKmLimit { get; set; }
    public decimal BasePrice { get; set; }
    public int ExtraKm { get; set; }
    public decimal ExtraKmFee { get; set; }
    public int LateDays { get; set; }
    public decimal LateFee { get; set; }
    public decimal TotalPrice { get; set; }
    public RentalStatus Status { get; set; }
}

public class RentVehicleDto
{
    [Required]
    public Guid VehicleId { get; set; }

    [Range(1, RentalConsts.MaxRentalDays)]
    public int RentalDays { get; set; }
}

public class ExtendRentalDto
{
    [Range(1, 30)]
    public int AdditionalDays { get; set; }
}

public class ReturnRentalDto
{
    /// <summary>Odometer value (km) when the vehicle is returned.</summary>
    [Range(0, int.MaxValue)]
    public int EndMileage { get; set; }

    /// <summary>
    /// Optional. Only users with the "Rentals.Manage" permission (admin) may set it;
    /// normally the current time is used.
    /// </summary>
    public DateTime? ReturnDate { get; set; }
}

public class GetRentalListInput : PagedResultRequestDto
{
    public RentalStatus? Status { get; set; }
    public Guid? VehicleId { get; set; }

    /// <summary>Only used by the admin list.</summary>
    public Guid? UserId { get; set; }
}
