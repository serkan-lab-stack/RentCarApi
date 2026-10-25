using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace RentCar.Customers;

public class CustomerProfileDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? LicenseClass { get; set; }
    public DateTime? LicenseExpirationDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
}

public class UpdateCustomerProfileDto
{
    [Required]
    [StringLength(CustomerProfileConsts.MaxLicenseNumberLength)]
    public string LicenseNumber { get; set; } = default!;

    [Required]
    [StringLength(CustomerProfileConsts.MaxLicenseClassLength)]
    public string LicenseClass { get; set; } = default!;

    [Required]
    public DateTime LicenseExpirationDate { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(CustomerProfileConsts.MaxAddressLength)]
    public string? Address { get; set; }
}
