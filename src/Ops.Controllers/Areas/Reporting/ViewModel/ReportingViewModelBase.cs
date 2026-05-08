using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Reporting.ViewModel
{
    public abstract class ReportingViewModelBase : PaginateModel
    {
        public string Heading { get; set; }
        public bool IsIndex { get; set; }
        public string SecondaryHeading { get; set; }
    }
}