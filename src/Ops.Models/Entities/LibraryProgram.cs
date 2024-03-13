﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Utility.Helpers;

namespace Ocuda.Ops.Models.Entities
{
    public class LibraryProgram : Abstract.BaseEntity
    {
        public LibraryProgram()
        {
            AgeGroups = new List<int>();
        }

        public IList<int> AgeGroups { get; }

        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [MaxLength(255)]
        public string? ContactName { get; set; }

        [MaxLength(255)]
        public string? ContactPhone { get; set; }

        [NotMapped]
        public string Description { get; set; }

        [Required]
        public int DescriptionSegmentId { get; set; }

        [Required]
        [DisplayName("Duration (in minutes)")]
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

        public int? HistoricId { get; set; }
        public int? ImportedBy { get; set; }
        public string? Instructor { get; set; }
        public bool IsAllDay { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsGuardianInfoRequired { get; set; }
        public bool IsOnline { get; set; }

        [Required]
        public bool IsRegistrationRequired { get; set; }

        [Required]
        public bool IsStaffRegistrationRequired { get; set; }

        public int? LocationDescriptionSegmentId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [NotMapped]
        public string LocationName { get; set; }

        public int? MaxAgeMonths { get; set; }
        public int MaxPeople { get; set; }
        public int? MaxWaitList { get; set; }

        public int? MinAgeMonths { get; set; }

        public User OwnedByUser { get; set; }

        [ForeignKey(nameof(OwnedByUser))]
        public int OwnedByUserId { get; set; }

        [NotMapped]
        public int? RegistrationCount { get; set; }

        public int? ScheduledEventId { get; set; }
        public DateTime? SignUpEnd { get; set; }

        public DateTime? SignUpStart { get; set; }

        public string? Sponsor { get; set; }

        public string? SponsorLink { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [NotMapped]
        public string Title { get; set; }

        [Required]
        public int TitleSegmentId { get; set; }

        public int? TotalAttendance { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}