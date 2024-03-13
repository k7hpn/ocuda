using System;
using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Events.ViewModels
{
    public class DetailsViewModel
    {
        public DateTime Now { get; set; }
        public ScheduledEvent ScheduledEvent { get; set; }
        public IEnumerable<ScheduledEventRegistration> ScheduledEventRegistrations { get; set; }
    }
}