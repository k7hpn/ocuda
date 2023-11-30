using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Events
{
    public class ImportViewModel
    {
        [DisplayName("Program File")]
        [FileExtensions(Extensions = "json")]
        public string FileName
        {
            get
            {
                return Import?.FileName;
            }
        }

        [Required]
        public IFormFile Import { get; set; }
    }
}