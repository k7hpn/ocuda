using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Events.ViewModels
{
    public class RegistrationViewModel
    {
        public ScheduledEventRegistration ScheduledEventRegistration { get; set; }
        public IEnumerable<ScheduledEventRegistrationHistory> ScheduledEventRegistrationHistories { get; set; }
    }
}