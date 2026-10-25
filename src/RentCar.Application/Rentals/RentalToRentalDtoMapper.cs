using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace RentCar.Rentals;

[Mapper]
public partial class RentalToRentalDtoMapper : MapperBase<Rental, RentalDto>
{
    public override partial RentalDto Map(Rental source);

    public override partial void Map(Rental source, RentalDto destination);
}
