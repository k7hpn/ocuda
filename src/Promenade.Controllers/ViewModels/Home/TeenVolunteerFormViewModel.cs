﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Home
{
    public class TeenVolunteerFormViewModel : VolunteerFormViewModel
    {
        public bool AdultFormAvailable { get; set; }

        [DisplayName(i18n.Keys.Promenade.PromptGuardianEmail)]
        [EmailAddress]
        [MaxLength(255)]
        [Required]
        public string GuardianEmail { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptGuardianName)]
        [MaxLength(255)]
        public string GuardianName { get; set; }

        [DisplayName(i18n.Keys.Promenade.PromptGuardianPhone)]
        [MaxLength(255)]
        [Phone]
        [Required]
        public string GuardianPhone { get; set; }

        public override VolunteerFormSubmission ToFormSubmission()
        {
            var submission = base.ToFormSubmission();
            submission.GuardianEmail = GuardianEmail;
            submission.GuardianName = GuardianName;
            submission.GuardianPhone = GuardianPhone;
            return submission;
        }
    }
}