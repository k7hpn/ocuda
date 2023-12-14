using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IEventService
    {
        public Task<AgeGroup> AddSaveAgeGroupAsync(AgeGroup ageGroup);

        public Task<ICollection<AgeGroup>> GetAgeGroupLookupAsync();

        public Task<ScheduledEvent> GetEventAsync(ScheduledEventType ScheduledEventType,
            int itemId);

        public Task<CollectionWithCount<ScheduledEvent>> PaginateAsync(BaseFilter filter);
    }
}