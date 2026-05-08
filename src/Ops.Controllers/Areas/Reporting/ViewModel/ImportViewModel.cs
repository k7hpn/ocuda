using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Definitions.Models;

namespace Ocuda.Ops.Controllers.Areas.Reporting.ViewModel
{
    public class ImportViewModel : ReportingViewModelBase
    {
        [DisplayName("Select a date in the month the import covers")]
        [Required]
        public DateTime DataDate { get; set; }

        [DisplayName("Select the data file to upload")]
        [Required]
        public IFormFile DataFile { get; set; }

        public ReportDefinition Report { get; set; }
    }
}