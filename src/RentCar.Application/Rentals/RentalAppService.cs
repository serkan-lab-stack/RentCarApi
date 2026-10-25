using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RentCar.Customers;
using RentCar.Permissions;
using RentCar.Vehicles;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace RentCar.Rentals;

// Every logged-in user can rent, extend and return his/her own rentals.
// Listing all rentals / returning on behalf of others needs the Rentals.Manage permission (admin).
[Authorize]
public class RentalAppService : RentCarAppService, IRentalAppService
{
    private readonly IRepository<Rental, Guid> _rentalRepository;
    private readonly IRepository<Vehicle, Guid> _vehicleRepository;
    private readonly IRepository<CustomerProfile, Guid> _profileRepository;

    public RentalAppService(
        IRepository<Rental, Guid> rentalRepository,
        IRepository<Vehicle, Guid> vehicleRepository,
        IRepository<CustomerProfile, Guid> profileRepository)
    {
        _rentalRepository = rentalRepository;
        _vehicleRepository = vehicleRepository;
        _profileRepository = profileRepository;
    }

    public async Task<RentalDto> RentAsync(RentVehicleDto input)
    {
        var userId = CurrentUser.GetId();
        var now = Clock.Now;

        // The license must exist and be valid until the end of the rental
        await EnsureValidLicenseAsync(userId, now.AddDays(input.RentalDays));

        var vehicle = await _vehicleRepository.GetAsync(input.VehicleId);
        vehicle.MarkAsRented(); // throws if the vehicle is rented or in maintenance

        var rental = new Rental(GuidGenerator.Create(), userId, vehicle, now, input.RentalDays);

        await _vehicleRepository.UpdateAsync(vehicle);
        await _rentalRepository.InsertAsync(rental);

        return ObjectMapper.Map<Rental, RentalDto>(rental);
    }

    public async Task<RentalDto> ExtendAsync(Guid id, ExtendRentalDto input)
    {
        var rental = await _rentalRepository.GetAsync(id);
        await EnsureCanAccessAsync(rental);

        await EnsureValidLicenseAsync(rental.UserId, rental.PlannedEndDate.AddDays(input.AdditionalDays));

        rental.Extend(input.AdditionalDays, Clock.Now);
        await _rentalRepository.UpdateAsync(rental);

        return ObjectMapper.Map<Rental, RentalDto>(rental);
    }

    public async Task<RentalDto> ReturnAsync(Guid id, ReturnRentalDto input)
    {
        var rental = await _rentalRepository.GetAsync(id);
        await EnsureCanAccessAsync(rental);

        var returnDate = Clock.Now;
        if (input.ReturnDate.HasValue)
        {
            // A custom return date is a back-office feature
            await AuthorizationService.CheckAsync(RentCarPermissions.Rentals.Manage);
            returnDate = input.ReturnDate.Value;
        }

        var vehicle = await _vehicleRepository.GetAsync(rental.VehicleId);

        rental.Complete(returnDate, input.EndMileage);        // calculates the final price
        vehicle.ReturnFromRental(input.EndMileage);           // updates km, sends to maintenance if due

        await _rentalRepository.UpdateAsync(rental);
        await _vehicleRepository.UpdateAsync(vehicle);

        return ObjectMapper.Map<Rental, RentalDto>(rental);
    }

    public async Task<RentalDto> GetAsync(Guid id)
    {
        var rental = await _rentalRepository.GetAsync(id);
        await EnsureCanAccessAsync(rental);

        return ObjectMapper.Map<Rental, RentalDto>(rental);
    }

    public async Task<PagedResultDto<RentalDto>> GetMyRentalsAsync(GetRentalListInput input)
    {
        var userId = CurrentUser.GetId();
        return await GetPagedAsync(r => r.UserId == userId, input);
    }

    [Authorize(RentCarPermissions.Rentals.Manage)]
    public async Task<PagedResultDto<RentalDto>> GetListAsync(GetRentalListInput input)
    {
        Expression<Func<Rental, bool>> filter = r => true;
        if (input.UserId.HasValue)
        {
            var filterUserId = input.UserId.Value;
            filter = r => r.UserId == filterUserId;
        }

        return await GetPagedAsync(filter, input);
    }

    private async Task<PagedResultDto<RentalDto>> GetPagedAsync(
        Expression<Func<Rental, bool>> baseFilter,
        GetRentalListInput input)
    {
        var queryable = await _rentalRepository.GetQueryableAsync();

        var query = queryable
            .Where(baseFilter)
            .WhereIf(input.Status.HasValue, r => r.Status == input.Status!.Value)
            .WhereIf(input.VehicleId.HasValue, r => r.VehicleId == input.VehicleId!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var rentals = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<RentalDto>(
            totalCount,
            ObjectMapper.Map<List<Rental>, List<RentalDto>>(rentals));
    }

    private async Task EnsureValidLicenseAsync(Guid userId, DateTime rentalEndDate)
    {
        var profile = await _profileRepository.FindAsync(p => p.UserId == userId);

        if (profile == null)
        {
            throw new BusinessException(RentCarDomainErrorCodes.LicenseRequired);
        }

        if (!profile.IsLicenseValidOn(rentalEndDate))
        {
            throw new BusinessException(RentCarDomainErrorCodes.LicenseExpiresBeforeRentalEnd)
                .WithData("ExpirationDate", profile.LicenseExpirationDate.ToString("yyyy-MM-dd"));
        }
    }

    private async Task EnsureCanAccessAsync(Rental rental)
    {
        if (rental.UserId == CurrentUser.GetId())
        {
            return;
        }

        if (await AuthorizationService.IsGrantedAsync(RentCarPermissions.Rentals.Manage))
        {
            return;
        }

        throw new BusinessException(RentCarDomainErrorCodes.RentalAccessDenied);
    }
}
