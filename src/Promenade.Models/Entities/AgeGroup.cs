using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class AgeGroup
    {
        [NotMapped]
        public string DisplayName { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public bool IsDisabled { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public Segment Segment { get; set; }

        [Required]
        public int SegmentId { get; set; }
    }
}