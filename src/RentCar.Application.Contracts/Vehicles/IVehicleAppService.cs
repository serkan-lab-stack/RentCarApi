using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace RentCar.Vehicles;

public interface IVehicleAppService : IApplicationService
{
    Task<VehicleDto> GetAsync(Guid id);

    Task<PagedResultDto<VehicleDto>> GetListAsync(GetVehicleListInput input);

    Task<VehicleDto> CreateAsync(CreateVehicleDto input);

    Task<VehicleDto> UpdateAsync(Guid id, UpdateVehicleDto input);

    Task DeleteAsync(Guid id);

    Task<VehicleDto> SendToMaintenanceAsync(Guid id);

    Task<VehicleDto> CompleteMaintenanceAsync(Guid id);
}
