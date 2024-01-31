using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class AgeGroupRepository
        : GenericRepository<PromenadeContext, AgeGroup>, IAgeGroupRepository
    {
        public AgeGroupRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<AgeGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<AgeGroup>> GetAgeGroupsAsync(IEnumerable<int> ageGroupIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => ageGroupIds.Contains(_.Id))
                .ToListAsync();
        }
    }
}