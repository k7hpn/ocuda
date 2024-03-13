using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IEventService
    {
        public Task<AgeGroup> AddSaveAgeGroupAsync(AgeGroup ageGroup);

        public Task<ScheduledEvent> CreateEventAsync(LibraryProgram libraryProgram, string slug);

        public Task<ICollection<AgeGroup>> GetAgeGroupLookupAsync();

        public Task<ScheduledEvent> GetEventAsync(int eventId);

        public Task<ScheduledEventRegistration> GetEventRegistrationAsync(Guid id);

        public Task<IEnumerable<ScheduledEventRegistration>>
            GetEventRegistrationsAsync(int eventId);

        public Task<IDictionary<int, int>>
            GetRegistrationCountAsync(IEnumerable<int> registrationEventIds);

        public Task<IEnumerable<ScheduledEventRegistrationHistory>>
            GetRegistrationHistoryAsync(Guid scheduledEventRegistrationId);

        public Task<bool> IsSlugInUseAsync(string slug);

        public Task<CollectionWithCount<ScheduledEvent>> PaginateAsync(BaseFilter filter);
    }
}