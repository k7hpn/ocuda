using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduledEventLocation
    {
        public Location Location { get; set; }

        [Key]
        public int LocationId { get; set; }

        public ScheduledEvent ScheduledEvent { get; set; }

        [Key]
        public int ScheduledEventId { get; set; }
    }
}