using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ocuda.Ops.Models.Entities
{
    public class LibraryProgram : Abstract.BaseEntity
    {
        // DONE list of ints to agegroups
        // DONE program id related to other programid table
        // int link to scheduledeventtype -> move out to global namespace?

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

        [Required]
        public int LocationId { get; set; }

        [NotMapped]
        public string LocationName { get; set; }

        public int? MaxAgeMonths { get; set; }
        public int? MaxPeople { get; set; }
        public int? MaxWaitList { get; set; }
        public int? MinAgeMonths { get; set; }
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

        public string DurationText
        {
            get
            {
                if (IsAllDay)
                {
                    return "All day";
                }
                if (DurationMinutes == 1)
                {
                    return $"{DurationMinutes} minute";
                }
                if (DurationMinutes < 60)
                {
                    return $"{DurationMinutes} minutes";
                }
                var ts = TimeSpan.FromMinutes(DurationMinutes);
                var sb = new StringBuilder();
                if(ts.Days > 0)
                {
                    sb.Append(ts.Days);
                    if(ts.Days > 1)
                    {
                        sb.Append(" days ");
                    }
                    else
                    {
                        sb.Append(" day ");
                    }
                }
                if (ts.Hours > 0)
                {
                    sb.Append(ts.Hours);
                    if (ts.Hours > 1)
                    {
                        sb.Append(" hours ");
                    }
                    else
                    {
                        sb.Append(" hour ");
                    }
                }
                if(ts.Minutes > 0)
                {
                    sb.Append(ts.Minutes);
                    if(ts.Minutes > 1)
                    {
                        sb.Append(" minutes ");
                    }
                    else
                    {
                        sb.Append(" minute ");
                    }
                }
                return sb.ToString();
            }
        }
    }
}