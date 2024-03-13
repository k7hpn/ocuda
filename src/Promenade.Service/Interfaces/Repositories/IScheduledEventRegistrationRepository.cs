using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IScheduledEventRegistrationRepository
        : IGenericRepository<ScheduledEventRegistration>
    {
        public Task<int> GetCountAsync(int scheduledEventId);
    }
}