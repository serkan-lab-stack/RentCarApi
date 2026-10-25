using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace RentCar.Rentals;

public interface IRentalAppService : IApplicationService
{
    Task<RentalDto> RentAsync(RentVehicleDto input);

    Task<RentalDto> ExtendAsync(Guid id, ExtendRentalDto input);

    Task<RentalDto> ReturnAsync(Guid id, ReturnRentalDto input);

    Task<RentalDto> GetAsync(Guid id);

    /// <summary>Rental history of the logged-in user.</summary>
    Task<PagedResultDto<RentalDto>> GetMyRentalsAsync(GetRentalListInput input);

    /// <summary>All rentals (admin).</summary>
    Task<PagedResultDto<RentalDto>> GetListAsync(GetRentalListInput input);
}
