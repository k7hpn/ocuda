using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ScheduledEventLocationRepository
        : GenericRepository<PromenadeContext, ScheduledEventLocation>, IScheduledEventLocationRepository
    {
        public ScheduledEventLocationRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventLocationRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}