using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace RentCar.Customers;

[Mapper]
public partial class CustomerProfileToCustomerProfileDtoMapper : MapperBase<CustomerProfile, CustomerProfileDto>
{
    public override partial CustomerProfileDto Map(CustomerProfile source);

    public override partial void Map(CustomerProfile source, CustomerProfileDto destination);
}
