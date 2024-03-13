using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ScheduledEventRegistrationRepository
        : GenericRepository<PromenadeContext, ScheduledEventRegistration>,
        IScheduledEventRegistrationRepository
    {
        public ScheduledEventRegistrationRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventRegistrationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ScheduledEventRegistration> AddSaveAsync(ScheduledEventRegistration registrations)
        {
            await DbSet.AddRangeAsync(registrations);
            await SaveAsync();
            return registrations;
        }

        public async Task<IEnumerable<ScheduledEventRegistration>> GetActiveByIdAsync(int eventId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.ScheduledEventId == eventId)
                .OrderBy(_ => _.RegisteredAt)
                .ToListAsync();
        }

        public async Task<ScheduledEventRegistration> GetByIdAsync(Guid id)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<IDictionary<int, int>>
            GetRegistrationCountAsync(IEnumerable<int> regstrationEventIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && regstrationEventIds.Contains(_.ScheduledEventId))
                .GroupBy(_ => _.ScheduledEventId)
                .Select(_ => new { _.Key, Count = _.Count() })
                .ToDictionaryAsync(_ => _.Key, _ => _.Count);
        }
    }
}