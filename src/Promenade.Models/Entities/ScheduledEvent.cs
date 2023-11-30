using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduledEvent
    {
        [MaxLength(255)]
        public string ContactEmail { get; set; }

        [MaxLength(255)]
        public string ContactName { get; set; }

        [MaxLength(255)]
        public string ContactPhone { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        public Segment EventDescription { get; set; }

        [Required]
        public int EventDescriptionId { get; set; }

        [NotMapped]
        public SegmentText EventDescriptionText { get; set; }

        public Segment EventName { get; set; }

        [Required]
        public int EventNameId { get; set; }

        [NotMapped]
        public SegmentText EventNameText { get; set; }

        [Required]
        public int EventTypeId { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int Id { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        public bool RegistrationRequired { get; set; }

        public IEnumerable<ScheduledEventAgeGroup> ScheduledEventAgeGroups { get; set; }

        public IEnumerable<ScheduledEventEventCategory> ScheduledEventEventCategories { get; set; }
        public ScheduledEventType ScheduledEventType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
    }
}