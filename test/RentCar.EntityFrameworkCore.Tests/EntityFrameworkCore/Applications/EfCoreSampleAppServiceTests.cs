using RentCar.Samples;
using Xunit;

namespace RentCar.EntityFrameworkCore.Applications;

[Collection(RentCarTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<RentCarEntityFrameworkCoreTestModule>
{

}
