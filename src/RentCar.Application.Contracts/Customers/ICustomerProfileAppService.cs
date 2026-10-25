using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace RentCar.Customers;

public interface ICustomerProfileAppService : IApplicationService
{
    /// <summary>Returns the profile of the logged-in user (empty values if never saved).</summary>
    Task<CustomerProfileDto> GetMyProfileAsync();

    /// <summary>Creates or updates the profile of the logged-in user.</summary>
    Task<CustomerProfileDto> UpdateMyProfileAsync(UpdateCustomerProfileDto input);
}
