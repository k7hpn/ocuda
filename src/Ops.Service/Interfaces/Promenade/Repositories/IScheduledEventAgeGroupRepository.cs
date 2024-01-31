using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduledEventAgeGroupRepository : IGenericRepository<ScheduledEventAgeGroup>
    {
        public Task AddAsync(int scheduledEventId, int ageGroupId);
    }
}