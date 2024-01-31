using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ScheduledEventAgeGroupRepository
        : GenericRepository<PromenadeContext, ScheduledEventAgeGroup>,
        IScheduledEventAgeGroupRepository
    {
        public ScheduledEventAgeGroupRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventAgeGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<int>> GetByScheduledEventId(int scheduledEventId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ScheduledEventId == scheduledEventId)
                .Select(_ => _.AgeGroupId)
                .ToListAsync();
        }
    }
}