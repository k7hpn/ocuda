using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduledEventAgeGroup
    {
        public AgeGroup AgeGroup { get; set; }

        [Key]
        [Required]
        public int AgeGroupId { get; set; }

        public ScheduledEvent ScheduledEvent { get; set; }

        [Key]
        [Required]
        public int ScheduledEventId { get; set; }
    }
}