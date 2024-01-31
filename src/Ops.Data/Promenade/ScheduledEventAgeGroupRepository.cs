using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Data.Promenade
{
    public class ScheduledEventAgeGroupRepository : GenericRepository<PromenadeContext, ScheduledEventAgeGroup>,
        IScheduledEventAgeGroupRepository
    {
        public ScheduledEventAgeGroupRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventAgeGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddAsync(int scheduledEventId, int ageGroupId)
        {
            try
            {
                await DbSet.AddAsync(new ScheduledEventAgeGroup
                {
                    ScheduledEventId = scheduledEventId,
                    AgeGroupId = ageGroupId,
                });
                await SaveAsync();
            }
            catch (InvalidOperationException ioex)
            {
                throw new OcudaException($"An error occurred inserting a ScheduledEventAgeGroup for Age Group ID {ageGroupId} and Scheduled Event ID {scheduledEventId}: {ioex.Message}", ioex);
            }
        }
    }
}