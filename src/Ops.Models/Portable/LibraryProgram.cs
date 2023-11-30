using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Portable
{
    public class LibraryProgram
    {
        internal LibraryProgram()
        {
            AgeGroups = new List<string>();
        }

        public IList<string> AgeGroups { get; }

        [MaxLength(255)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(255)]
        public string ContactName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string ContactPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public string Instructor { get; set; }

        [Required]
        public int LocationId { get; set; }

        public int MaxPeople { get; set; }

        public int MaxWaitList { get; set; }

        [Required]
        public bool RegistrationRequired { get; set; }

        public string Sponsor { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public LibraryProgramText? Text { get; set; }

        public int TotalAttendance { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}