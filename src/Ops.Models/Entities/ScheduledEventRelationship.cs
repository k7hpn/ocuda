using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class ScheduledEventRelationship
    {
        public ScheduledEvent RelatedScheduledEvent { get; set; }

        [Key]
        public int RelatedScheduledEventId { get; set; }

        public ScheduledEvent ScheduledEvent { get; set; }

        [Key]
        public int ScheduledEventId { get; set; }
    }
}