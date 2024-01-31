using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Models.Entities
{
    // announcement
    // closure (item connects to one or more locationhoursoverrides)
    // program
    // podca1t

    [Index(nameof(Slug), IsUnique = true)]
    public class ScheduledEvent
    {
        public ScheduledEvent()
        {
            AgeGroups = new List<string>();
        }

        [NotMapped]
        public ICollection<string> AgeGroups { get; }

        [NotMapped]
        public string Description { get; set; }

        public int DescriptionSegmentId { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        public string DurationText
        {
            get
            {
                if (IsAllDay)
                {
                    return "All day";
                }
                return TimeDisplayHelper.FormatMinutes(DurationMinutes);
            }
        }

        public int? ExternalId { get; set; }

        [Key]
        public int Id { get; set; }

        [Required]
        public bool IsAllDay { get; set; }

        [Required]
        public bool IsPublished { get; set; }

        [NotMapped]
        public string LocationDescription { get; set; }

        public int? LocationDescriptionId { get; set; }

        // TODO job that switches ispublished after publishon
        // where publishon <= now and !ispublished
        public DateTime? PublishOn { get; set; }

        [Required]
        public ScheduledEventType ScheduledEventType { get; set; }

        [MaxLength(255)]
        [Required]
        public string Slug { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [NotMapped]
        public string Title { get; set; }

        [Required]
        public int TitleSegmentId { get; set; }
    }
}