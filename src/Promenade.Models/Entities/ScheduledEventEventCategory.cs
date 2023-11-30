using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduledEventEventCategory
    {
        [Required]
        public bool Primary { get; set; }

        public ScheduledEvent ScheduledEvent { get; set; }

        public ScheduledEventCategory ScheduledEventCategory { get; set; }

        [Key]
        [Required]
        public int ScheduledEventCategoryId { get; set; }

        [Key]
        [Required]
        public int ScheduledEventId { get; set; }
    }
}