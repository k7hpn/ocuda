using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduledEventRegistration
    {
        [MaxLength(255)]
        public string Email { get; set; } = null!;

        [MaxLength(255)]
        public string FirstName { get; set; } = null!;

        [Key]
        public Guid Id { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [MaxLength(255)]
        public string LastName { get; set; } = null!;

        [MaxLength(255)]
        public string Phone { get; set; } = null!;

        public DateTime RegisteredAt { get; set; }

        [NotMapped]
        public bool RegisteredByStaff { get; set; }

        public Guid? RelatedScheduledEventRegistrationId { get; set; }
        public int ScheduledEventId { get; set; }
    }
}