using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace RentCar.Customers;

/// <summary>
/// Rental-specific information of a user (driving license, address).
/// Name, surname, phone and e-mail stay on the built-in ABP identity user.
/// </summary>
public class CustomerProfile : FullAuditedAggregateRoot<Guid>
{
    public Guid UserId { get; private set; }

    public string LicenseNumber { get; private set; } = default!;

    /// <summary>For example "B".</summary>
    public string LicenseClass { get; private set; } = default!;

    public DateTime LicenseExpirationDate { get; private set; }

    public DateTime? DateOfBirth { get; private set; }

    public string? Address { get; private set; }

    private CustomerProfile()
    {
        // Required by EF Core
    }

    public CustomerProfile(
        Guid id,
        Guid userId,
        string licenseNumber,
        string licenseClass,
        DateTime licenseExpirationDate,
        DateTime? dateOfBirth,
        string? address) : base(id)
    {
        UserId = userId;
        Update(licenseNumber, licenseClass, licenseExpirationDate, dateOfBirth, address);
    }

    public void Update(
        string licenseNumber,
        string licenseClass,
        DateTime licenseExpirationDate,
        DateTime? dateOfBirth,
        string? address)
    {
        LicenseNumber = Check.NotNullOrWhiteSpace(licenseNumber, nameof(licenseNumber), CustomerProfileConsts.MaxLicenseNumberLength);
        LicenseClass = Check.NotNullOrWhiteSpace(licenseClass, nameof(licenseClass), CustomerProfileConsts.MaxLicenseClassLength);
        LicenseExpirationDate = licenseExpirationDate;
        DateOfBirth = dateOfBirth;

        var trimmedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        if (trimmedAddress != null && trimmedAddress.Length > CustomerProfileConsts.MaxAddressLength)
        {
            throw new ArgumentException("Address is too long.", nameof(address));
        }

        Address = trimmedAddress;
    }

    /// <summary>
    /// Used by the rental flow: the license must still be valid on the given date
    /// (for example on the planned return date of the rental).
    /// </summary>
    public bool IsLicenseValidOn(DateTime date)
    {
        return LicenseExpirationDate.Date >= date.Date;
    }
}
