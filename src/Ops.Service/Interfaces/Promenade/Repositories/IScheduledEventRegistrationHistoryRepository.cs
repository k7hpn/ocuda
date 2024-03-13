using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduledEventRegistrationHistoryRepository
        : IGenericRepository<ScheduledEventRegistrationHistory>
    {
        public Task AddImportAsync(Guid scheduledEventRegistrationId,
            DateTime registeredAt,
            DateTime importedAt,
            int? staffId,
            int importUserId);

        public Task<IEnumerable<ScheduledEventRegistrationHistory>>
            GetByRegistrationIdAsync(Guid scheduledEventRegistrationId);
    }
}