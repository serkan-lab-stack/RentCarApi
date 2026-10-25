using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace RentCar.DamageReports;

[Mapper]
public partial class DamageReportToDamageReportDtoMapper : MapperBase<DamageReport, DamageReportDto>
{
    public override partial DamageReportDto Map(DamageReport source);

    public override partial void Map(DamageReport source, DamageReportDto destination);
}
