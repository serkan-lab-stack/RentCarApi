using RentCar.Samples;
using Xunit;

namespace RentCar.EntityFrameworkCore.Domains;

[Collection(RentCarTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<RentCarEntityFrameworkCoreTestModule>
{

}
