using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocuda.Ops.Models.Entities
{
    public class LibraryProgramRelationship
    {
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedBy { get; set; }

        public User CreatedByUser { get; set; }
        public LibraryProgram LibraryProgram { get; set; }

        [Required]
        [Key]
        public int LibraryProgramId { get; set; }

        public LibraryProgram RelatedLibraryProgram { get; set; }

        [Required]
        [Key]
        public int RelatedLibraryProgramId { get; set; }
    }
}