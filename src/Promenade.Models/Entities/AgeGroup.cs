using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocuda.Promenade.Models.Entities
{
    public class AgeGroup
    {
        public Segment DisplayName { get; set; }

        [Required]
        public int DisplayNameId { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }
    }
}