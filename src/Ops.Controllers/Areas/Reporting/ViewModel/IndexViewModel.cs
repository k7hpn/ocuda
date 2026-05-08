using System.Collections.Generic;
using Ocuda.Ops.Models.Definitions.Models;

namespace Ocuda.Ops.Controllers.Areas.Reporting.ViewModel
{
    public class IndexViewModel : ReportingViewModelBase
    {
        public IEnumerable<ReportDefinition> Reports { get; set; }
    }
}