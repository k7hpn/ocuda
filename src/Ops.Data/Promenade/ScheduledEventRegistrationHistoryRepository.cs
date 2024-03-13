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
    public class ScheduledEventRegistrationHistoryRepository
        : GenericRepository<PromenadeContext, ScheduledEventRegistrationHistory>,
        IScheduledEventRegistrationHistoryRepository
    {
        public ScheduledEventRegistrationHistoryRepository(
            Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventRegistrationHistoryRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task AddImportAsync(Guid scheduledEventRegistrationId,
            DateTime registeredAt,
            DateTime importedAt,
            int? staffId,
            int importUserId)
        {
            await DbSet.AddRangeAsync(
                new ScheduledEventRegistrationHistory
                {
                    CreatedAt = registeredAt,
                    Notes = "Event registration received",
                    ScheduledEventRegistrationId = scheduledEventRegistrationId,
                    StaffId = staffId
                },
                new ScheduledEventRegistrationHistory
                {
                    CreatedAt = importedAt,
                    Notes = "Record imported into system",
                    ScheduledEventRegistrationId = scheduledEventRegistrationId,
                    StaffId = importUserId
                });

            await SaveAsync();
        }

        public async Task<IEnumerable<ScheduledEventRegistrationHistory>>
            GetByRegistrationIdAsync(Guid scheduledEventRegistrationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ScheduledEventRegistrationId == scheduledEventRegistrationId)
                .OrderBy(_ => _.CreatedAt)
                .ToListAsync();
        }
    }
}