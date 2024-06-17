using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduledEventRegistrationHistory
    {
        [Required]
        public DateTime CreatedAt { get; set; }

        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Notes { get; set; }

        [Required]
        public Guid ScheduledEventRegistrationId { get; set; }

        public int? StaffId { get; set; }
    }
}