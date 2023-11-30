using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class ScheduledEvent : Abstract.BaseEntity
    {
        [Required]
        public bool AllDay { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        public IEnumerable<ScheduledEventLocations> ScheduledEventLocations { get; set; }

        [Required]
        public ScheduledEventType ScheduledEventType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public bool IsPublished { get; set; }

        // tbd job that switches ispublished after publishon
        // where publishon <= now and !ispublished
        public DateTime? PublishOn { get; set; }

        // announcement
        // tbd - home page automatically?

        // closure
        // closure item connects to one or more locationhoursoverrides

        // program

        public int? ProgramDetailsId { get; set; }

        // podcast

    }
}