using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace RentCar.Customers;

// Every authenticated user manages only his/her own profile; the user id always comes from the token.
[Authorize]
public class CustomerProfileAppService : RentCarAppService, ICustomerProfileAppService
{
    private readonly IRepository<CustomerProfile, Guid> _profileRepository;

    public CustomerProfileAppService(IRepository<CustomerProfile, Guid> profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<CustomerProfileDto> GetMyProfileAsync()
    {
        var userId = CurrentUser.GetId();
        var profile = await _profileRepository.FindAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return new CustomerProfileDto { UserId = userId };
        }

        return ObjectMapper.Map<CustomerProfile, CustomerProfileDto>(profile);
    }

    public async Task<CustomerProfileDto> UpdateMyProfileAsync(UpdateCustomerProfileDto input)
    {
        var userId = CurrentUser.GetId();
        var profile = await _profileRepository.FindAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new CustomerProfile(
                GuidGenerator.Create(),
                userId,
                input.LicenseNumber,
                input.LicenseClass,
                input.LicenseExpirationDate,
                input.DateOfBirth,
                input.Address);

            await _profileRepository.InsertAsync(profile);
        }
        else
        {
            profile.Update(
                input.LicenseNumber,
                input.LicenseClass,
                input.LicenseExpirationDate,
                input.DateOfBirth,
                input.Address);

            await _profileRepository.UpdateAsync(profile);
        }

        return ObjectMapper.Map<CustomerProfile, CustomerProfileDto>(profile);
    }
}
