using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Events.ViewModels
{
    public class RegistrationViewModel
    {
        public RegistrationViewModel()
        {
            RelevantUsers = new Dictionary<int, User>();
        }

        public IDictionary<int, User> RelevantUsers { get; }
        public ScheduledEventRegistration ScheduledEventRegistration { get; set; }
        public IEnumerable<ScheduledEventRegistrationHistory> ScheduledEventRegistrationHistories { get; set; }

        public User GetUser(int? userId)
        {
            if (!userId.HasValue)
            {
                return null;
            }

            return RelevantUsers.TryGetValue(userId.Value, out var user)
                ? user
                : null;
        }
    }
}