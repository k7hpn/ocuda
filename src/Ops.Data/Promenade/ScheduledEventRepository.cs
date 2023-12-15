using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Filters;
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

        public async Task<ScheduledEvent> GetAsync(int eventId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == eventId);
        }

        public async Task<bool> IsSlugInUseAsync(string slug)
        {
            return await DbSet
                .AsNoTracking()
                .AnyAsync(_ => _.Slug == slug);
        }

        public async Task<CollectionWithCount<ScheduledEvent>> PaginateAsync(BaseFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var query = DbSet;

            //var query = filter.CreatedById.HasValue
            //    ? incidents.Where(_ => _.CreatedBy == filter.CreatedById.Value)
            //    : incidents;

            //if (!string.IsNullOrEmpty(filter.SearchText))
            //{
            //    query = query.Where(_ => _.Description.Contains(filter.SearchText)
            //        || _.IncidentType.Description.Contains(filter.SearchText)
            //        || _.InjuriesDamages.Contains(filter.SearchText)
            //        || _.LocationDescription.Contains(filter.SearchText)
            //        || filter.LocationIds.Contains(_.LocationId)
            //        || _.CreatedByUser.Name.Contains(filter.SearchText)
            //        || filter.IncludeIds.Contains(_.Id));
            //}

            return new CollectionWithCount<ScheduledEvent>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.StartDate)
                    .ApplyPagination(filter)
                    .AsNoTracking()
                    .ToListAsync()
            };
        }
    }
}