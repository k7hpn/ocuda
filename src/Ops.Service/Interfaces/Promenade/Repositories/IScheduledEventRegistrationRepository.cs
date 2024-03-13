using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduledEventRegistrationRepository
        : IGenericRepository<ScheduledEventRegistration>
    {
        public Task<ScheduledEventRegistration> AddSaveAsync(ScheduledEventRegistration registrations);

        public Task<IEnumerable<ScheduledEventRegistration>> GetActiveByIdAsync(int eventId);

        public Task<ScheduledEventRegistration> GetByIdAsync(Guid id);

        public Task<IDictionary<int, int>> GetRegistrationCountAsync(IEnumerable<int> regstrationEventIds);
    }
}