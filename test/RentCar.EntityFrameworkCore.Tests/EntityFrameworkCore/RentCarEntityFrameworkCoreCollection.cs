using Xunit;

namespace RentCar.EntityFrameworkCore;

[CollectionDefinition(RentCarTestConsts.CollectionDefinitionName)]
public class RentCarEntityFrameworkCoreCollection : ICollectionFixture<RentCarEntityFrameworkCoreFixture>
{

}
