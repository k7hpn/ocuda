using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Portable
{
    public class ImportLibraryRegistration
    {
        [MaxLength(255)]
        public string Email { get; set; } = null!;

        [MaxLength(255)]
        public string FirstName { get; set; } = null!;

        [MaxLength(255)]
        public string LastName { get; set; } = null!;

        [MaxLength(255)]
        public string Phone { get; set; } = null!;

        public DateTime RegisteredAt { get; set; }
        public bool StaffRegistered { get; set; }
    }
}