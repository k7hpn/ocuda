using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class ScheduledEventLocations
    {
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedBy { get; set; }

        [NotMapped]
        public string CreatedByName { get; set; }

        public User CreatedByUser { get; set; }

        [Key]
        public int LocationId { get; set; }

        public ScheduledEvent ScheduledEvent { get; set; }

        [Key]
        public int ScheduledEventId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UpdatedByUser))]
        public int? UpdatedBy { get; set; }

        [NotMapped]
        public string UpdatedByName { get; set; }

        public User UpdatedByUser { get; set; }
    }
}