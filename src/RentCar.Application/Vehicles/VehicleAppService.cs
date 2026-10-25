using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RentCar.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace RentCar.Vehicles;

// Any authenticated user can list/view vehicles (customers need to see what is available).
// Write operations require the permissions defined in RentCarPermissions (granted to the admin role).
[Authorize]
public class VehicleAppService : RentCarAppService, IVehicleAppService
{
    private readonly IRepository<Vehicle, Guid> _vehicleRepository;

    public VehicleAppService(IRepository<Vehicle, Guid> vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<VehicleDto> GetAsync(Guid id)
    {
        var vehicle = await _vehicleRepository.GetAsync(id);
        return ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
    }

    public async Task<PagedResultDto<VehicleDto>> GetListAsync(GetVehicleListInput input)
    {
        var queryable = await _vehicleRepository.GetQueryableAsync();

        var query = queryable
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                v => v.Brand.Contains(input.Filter!) || v.Model.Contains(input.Filter!))
            .WhereIf(
                input.Status.HasValue,
                v => v.Status == input.Status!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var vehicles = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<VehicleDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<Vehicle>, System.Collections.Generic.List<VehicleDto>>(vehicles));
    }

    [Authorize(RentCarPermissions.Vehicles.Create)]
    public async Task<VehicleDto> CreateAsync(CreateVehicleDto input)
    {
        var vehicle = new Vehicle(
            GuidGenerator.Create(),
            input.Brand,
            input.Model,
            input.Year,
            input.Mileage,
            input.DailyPrice,
            input.ExtraKmPrice,
            input.DailyKmLimit,
            input.MaintenanceIntervalKm);

        await _vehicleRepository.InsertAsync(vehicle);

        return ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
    }

    [Authorize(RentCarPermissions.Vehicles.Edit)]
    public async Task<VehicleDto> UpdateAsync(Guid id, UpdateVehicleDto input)
    {
        var vehicle = await _vehicleRepository.GetAsync(id);

        vehicle.Update(
            input.Brand,
            input.Model,
            input.Year,
            input.DailyPrice,
            input.ExtraKmPrice,
            input.DailyKmLimit,
            input.MaintenanceIntervalKm);

        await _vehicleRepository.UpdateAsync(vehicle);

        return ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
    }

    [Authorize(RentCarPermissions.Vehicles.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var vehicle = await _vehicleRepository.GetAsync(id);
        vehicle.EnsureNotRented();

        await _vehicleRepository.DeleteAsync(vehicle); // soft delete (FullAuditedAggregateRoot)
    }

    [Authorize(RentCarPermissions.Vehicles.Edit)]
    public async Task<VehicleDto> SendToMaintenanceAsync(Guid id)
    {
        var vehicle = await _vehicleRepository.GetAsync(id);
        vehicle.SendToMaintenance();
        await _vehicleRepository.UpdateAsync(vehicle);

        return ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
    }

    [Authorize(RentCarPermissions.Vehicles.Edit)]
    public async Task<VehicleDto> CompleteMaintenanceAsync(Guid id)
    {
        var vehicle = await _vehicleRepository.GetAsync(id);
        vehicle.CompleteMaintenance();
        await _vehicleRepository.UpdateAsync(vehicle);

        return ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
    }
}
