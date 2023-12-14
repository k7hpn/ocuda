using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class ScheduledEventRepository : GenericRepository<PromenadeContext, ScheduledEvent>,
        IScheduledEventRepository
    {
        public ScheduledEventRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ScheduledEvent> GetAsync(ScheduledEventType ScheduledEventType,
            int itemId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.ScheduledEventType == ScheduledEventType
                    && _.ItemId == itemId);
        }
    }
}