using System.Net.Mime;
using Ocuda.Ops.Models.Definitions.Models;

namespace Ocuda.Ops.Models.Definitions
{
    /// <summary>
    /// Definition of available reports for the software
    /// </summary>
    public static class ReportDefinitions
    {
        public static readonly ReportDefinition[] Definitions =
        [
            // TODO REPORT fix the description of this report
            new() {
                Description = "Lorem ipsum",
                Id = ReportDefinitionId.HooplaCheckouts,
                ImportFileTypes = [MediaTypeNames.Text.Csv],
                Name = "Hoopla Checkouts",
                Period = ReportDefinitionPeriod.Monthly,
                SkipFirstColumn = ["", "Grand Total"],
                ReportType = ReportTypeElectronicResources
            }
        ];

        private const string ReportTypeElectronicResources = "Electronic Resources";
    }
}