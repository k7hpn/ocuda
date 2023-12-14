using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Portable
{
    public class ImportLibraryProgram
    {
        public ImportLibraryProgram()
        {
            AgeGroups = new List<string>();
        }

        public IList<string> AgeGroups { get; set; }

        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [MaxLength(255)]
        public string? ContactName { get; set; }

        [MaxLength(255)]
        public string? ContactPhone { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public string? Instructor { get; set; }
        public string? InternalEmail { get; set; }
        public bool IsAllDay { get; set; }
        public bool IsGuardianInfoRequired { get; set; }
        public bool IsOnline { get; set; }

        [Required]
        public bool IsRegistrationRequired { get; set; }

        [Required]
        public bool IsStaffRegistrationRequired { get; set; }

        [Required]
        public int LocationId { get; set; }

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

        [Required]
        public ImportLibraryProgramText? Text { get; set; }

        public int? TotalAttendance { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}