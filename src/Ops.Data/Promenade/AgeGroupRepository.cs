using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class AgeGroupRepository : GenericRepository<PromenadeContext, AgeGroup>,
        IAgeGroupRepository
    {
        public AgeGroupRepository(Repository<PromenadeContext> repositoryFacade, 
            ILogger<AgeGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<AgeGroup>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().OrderBy(_ => _.Name).ToListAsync();
        }
    }
}
