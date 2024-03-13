using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ScheduledEventRegistrationRepository
        : GenericRepository<PromenadeContext, ScheduledEventRegistration>,
        IScheduledEventRegistrationRepository
    {
        public ScheduledEventRegistrationRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventRegistrationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int> GetCountAsync(int scheduledEventId)
        {
            return await DbSet
                .AsNoTracking()
                .CountAsync(_ => _.ScheduledEventId == scheduledEventId);
        }
    }
}