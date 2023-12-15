using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduledEventRepository : IGenericRepository<ScheduledEvent>
    {
        public Task<ScheduledEvent> GetAsync(int eventId);

        public Task<bool> IsSlugInUseAsync(string slug);

        public Task<CollectionWithCount<ScheduledEvent>> PaginateAsync(BaseFilter filter);
    }
}