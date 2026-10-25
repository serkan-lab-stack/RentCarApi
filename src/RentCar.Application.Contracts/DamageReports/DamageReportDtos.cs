using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace RentCar.DamageReports;

public class DamageReportDto : EntityDto<Guid>
{
    public Guid RentalId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = default!;
    public DamageSeverity Severity { get; set; }
    public decimal DamageCost { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateDamageReportDto
{
    [Required]
    public Guid RentalId { get; set; }

    [Required]
    [StringLength(DamageReportConsts.MaxDescriptionLength)]
    public string Description { get; set; } = default!;

    /// <summary>1 = Minor, 2 = Moderate, 3 = Major</summary>
    [EnumDataType(typeof(DamageSeverity))]
    public DamageSeverity Severity { get; set; }
}

public class PayDamageReportDto
{
    /// <summary>Must be exactly the damage cost.</summary>
    [Range(0.01, 1_000_000_000)]
    public decimal Amount { get; set; }
}

public class GetDamageReportListInput : PagedResultRequestDto
{
    public Guid? RentalId { get; set; }
    public bool? IsPaid { get; set; }

    /// <summary>Only used by the admin list.</summary>
    public Guid? UserId { get; set; }
}
