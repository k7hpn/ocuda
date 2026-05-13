using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class ReportingImportHeaderRepository(Repository<OpsContext> repositoryFacade,
        ILogger<ReportingImportHeaderRepository> logger)
            : OpsRepository<OpsContext, ReportingImportHeader, int>(repositoryFacade, logger),
            IReportingImportHeaderRepository
    {
        public async Task<ReportingImportHeader> GetReportAsync(string reportType,
            int year,
            int month)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.ReportingImportData)
                .Include(_ => _.ReportingLocationSet.ReportingLocations)
                .AsSplitQuery()
                .SingleOrDefaultAsync(_ => _.ReportType == reportType
                    && _.Year == year
                    && _.Month == month);
        }
    }
}