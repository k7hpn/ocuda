﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class Post : Abstract.BaseEntity
    {
        [DisplayName("Category")]
        public int PostCategoryId { get; set; }
        public PostCategory PostCategory { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }

        [MaxLength(2000)]
        public string Content { get; set; }

        [DisplayName("Pin this post?")]
        public bool IsPinned { get; set; }

        [DisplayName("Show on homepage?")]
        public bool ShowOnHomepage { get; set; }

        public DateTime? PublishedAt { get; set; }
    }
}