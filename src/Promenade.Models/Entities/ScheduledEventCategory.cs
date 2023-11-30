using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduledEventCategory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int Id { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
    }
}