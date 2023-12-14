using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduledEventRepository : IGenericRepository<ScheduledEvent>
    {
        public Task<ScheduledEvent> GetAsync(ScheduledEventType ScheduledEventType, int itemId);
    }
}