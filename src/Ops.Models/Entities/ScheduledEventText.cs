using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class ScheduledEventText
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public string LanguageCode { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}