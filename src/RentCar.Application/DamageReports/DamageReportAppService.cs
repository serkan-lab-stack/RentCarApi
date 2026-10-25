using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RentCar.Permissions;
using RentCar.Rentals;
using RentCar.Vehicles;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace RentCar.DamageReports;

// A customer reports and pays damages of his/her own rentals.
// Admins (Rentals.Manage permission) can act on behalf of customers and see all reports.
[Authorize]
public class DamageReportAppService : RentCarAppService, IDamageReportAppService
{
    private readonly IRepository<DamageReport, Guid> _damageReportRepository;
    private readonly IRepository<Rental, Guid> _rentalRepository;
    private readonly IRepository<Vehicle, Guid> _vehicleRepository;

    public DamageReportAppService(
        IRepository<DamageReport, Guid> damageReportRepository,
        IRepository<Rental, Guid> rentalRepository,
        IRepository<Vehicle, Guid> vehicleRepository)
    {
        _damageReportRepository = damageReportRepository;
        _rentalRepository = rentalRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<DamageReportDto> CreateAsync(CreateDamageReportDto input)
    {
        var rental = await _rentalRepository.GetAsync(input.RentalId);
        await EnsureCanAccessAsync(rental.UserId);

        // The damage cost is calculated inside the entity
        var report = new DamageReport(GuidGenerator.Create(), rental, input.Description, input.Severity);
        await _damageReportRepository.InsertAsync(report);

        // Maintenance control: a major damage sends the vehicle to maintenance
        // (if it is currently rented by someone else, nothing changes).
        if (input.Severity == DamageSeverity.Major)
        {
            var vehicle = await _vehicleRepository.GetAsync(rental.VehicleId);
            if (vehicle.Status == VehicleStatus.Available)
            {
                vehicle.SendToMaintenance();
                await _vehicleRepository.UpdateAsync(vehicle);
            }
        }

        return ObjectMapper.Map<DamageReport, DamageReportDto>(report);
    }

    public async Task<DamageReportDto> PayAsync(Guid id, PayDamageReportDto input)
    {
        var report = await _damageReportRepository.GetAsync(id);
        await EnsureCanAccessAsync(report.UserId);

        report.Pay(input.Amount, Clock.Now);
        await _damageReportRepository.UpdateAsync(report);

        return ObjectMapper.Map<DamageReport, DamageReportDto>(report);
    }

    public async Task<DamageReportDto> GetAsync(Guid id)
    {
        var report = await _damageReportRepository.GetAsync(id);
        await EnsureCanAccessAsync(report.UserId);

        return ObjectMapper.Map<DamageReport, DamageReportDto>(report);
    }

    public async Task<PagedResultDto<DamageReportDto>> GetMyReportsAsync(GetDamageReportListInput input)
    {
        var userId = CurrentUser.GetId();
        return await GetPagedAsync(r => r.UserId == userId, input);
    }

    [Authorize(RentCarPermissions.Rentals.Manage)]
    public async Task<PagedResultDto<DamageReportDto>> GetListAsync(GetDamageReportListInput input)
    {
        Expression<Func<DamageReport, bool>> filter = r => true;
        if (input.UserId.HasValue)
        {
            var filterUserId = input.UserId.Value;
            filter = r => r.UserId == filterUserId;
        }

        return await GetPagedAsync(filter, input);
    }

    private async Task<PagedResultDto<DamageReportDto>> GetPagedAsync(
        Expression<Func<DamageReport, bool>> baseFilter,
        GetDamageReportListInput input)
    {
        var queryable = await _damageReportRepository.GetQueryableAsync();

        var query = queryable
            .Where(baseFilter)
            .WhereIf(input.RentalId.HasValue, r => r.RentalId == input.RentalId!.Value)
            .WhereIf(input.IsPaid.HasValue, r => r.IsPaid == input.IsPaid!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var reports = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<DamageReportDto>(
            totalCount,
            ObjectMapper.Map<List<DamageReport>, List<DamageReportDto>>(reports));
    }

    private async Task EnsureCanAccessAsync(Guid ownerUserId)
    {
        if (ownerUserId == CurrentUser.GetId())
        {
            return;
        }

        if (await AuthorizationService.IsGrantedAsync(RentCarPermissions.Rentals.Manage))
        {
            return;
        }

        throw new BusinessException(RentCarDomainErrorCodes.DamageReportAccessDenied);
    }
}
