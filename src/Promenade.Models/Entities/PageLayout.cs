﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageLayout
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int PageHeaderId { get; set; }
        public PageHeader PageHeader { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [DisplayName("Social Card")]
        public int? SocialCardId { get; set; }
        public SocialCard SocialCard { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        public ICollection<PageItem> Items { get; set; }

        [NotMapped]
        public PageLayoutText PageLayoutText { get; set; }

        public Guid PreviewId { get; set; }

        [NotMapped]
        public bool IsPreview { get; set; }
    }
}
