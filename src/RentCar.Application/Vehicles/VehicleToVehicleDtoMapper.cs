using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace RentCar.Vehicles;

// ABP 10+ templates use Mapperly (source generator) instead of AutoMapper.
[Mapper]
public partial class VehicleToVehicleDtoMapper : MapperBase<Vehicle, VehicleDto>
{
    public override partial VehicleDto Map(Vehicle source);

    public override partial void Map(Vehicle source, VehicleDto destination);
}
