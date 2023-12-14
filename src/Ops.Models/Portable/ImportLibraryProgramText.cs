using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Portable
{
    public class ImportLibraryProgramText
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string LanguageCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
    }
}