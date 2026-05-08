using System;
using System.IO;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IReportingService
    {
        CollectionWithCount<ReportDefinition> GetList(BaseFilter filter);

        Task<object> ProcessImportAsync(string reportId, DateTime dataDate, Stream import);
    }
}