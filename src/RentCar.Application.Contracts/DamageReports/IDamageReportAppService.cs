using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace RentCar.DamageReports;

public interface IDamageReportAppService : IApplicationService
{
    Task<DamageReportDto> CreateAsync(CreateDamageReportDto input);

    Task<DamageReportDto> PayAsync(Guid id, PayDamageReportDto input);

    Task<DamageReportDto> GetAsync(Guid id);

    /// <summary>Damage reports of the logged-in user.</summary>
    Task<PagedResultDto<DamageReportDto>> GetMyReportsAsync(GetDamageReportListInput input);

    /// <summary>All damage reports (admin).</summary>
    Task<PagedResultDto<DamageReportDto>> GetListAsync(GetDamageReportListInput input);
}
