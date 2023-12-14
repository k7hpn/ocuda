using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Models.Entities
{
    [Index(nameof(ScheduledEventType), nameof(ItemId), IsUnique = true)]
    public class ScheduledEvent
    {
        [Required]
        public bool AllDay { get; set; }

        public SegmentText Description { get; set; }

        public int DescriptionId { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Key]
        public int Id { get; set; }

        [Required]
        public bool IsPublished { get; set; }

        public Segment LocationDescription { get; set; }
        public int? LocationDescriptionId { get; set; }
        public int? ProgramDetailsId { get; set; }

        // tbd job that switches ispublished after publishon
        // where publishon <= now and !ispublished
        public DateTime? PublishOn { get; set; }

        public IEnumerable<ScheduledEventLocation> ScheduledEventLocations { get; set; }

        [Required]
        public ScheduledEventType ScheduledEventType { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        // announcement
        // tbd - home page automatically?

        // closure
        // closure item connects to one or more locationhoursoverrides

        public SegmentText Title { get; set; }

        // program
        [Required]
        public int TitleId { get; set; }

        // podcast
    }
}