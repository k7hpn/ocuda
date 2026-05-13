using System;
using System.IO;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IReportingService
    {
        CollectionWithCount<ReportDefinition> GetList(BaseFilter filter);

        Task<int> ProcessImportAsync(string reportId,
            DateTime dataDate,
            string filename,
            Stream import);

        Task<ReportingImportHeader> GetResultsAsync(string reportType, int year, int month);
    }
}