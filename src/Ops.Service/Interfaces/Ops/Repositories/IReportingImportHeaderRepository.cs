using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IReportingImportHeaderRepository : IOpsRepository<ReportingImportHeader, int>
    {
        public Task<ReportingImportHeader> GetReportAsync(string reportType, int year, int month);
    }
}