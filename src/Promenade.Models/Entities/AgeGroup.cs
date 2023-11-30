using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class AgeGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int Id { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public IEnumerable<ScheduledEventAgeGroup> ScheduledEvents { get; set; }
    }
}