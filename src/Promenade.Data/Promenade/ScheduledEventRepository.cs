using System;
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
    public class ScheduledEventRepository
        : GenericRepository<PromenadeContext, ScheduledEvent>, IScheduledEventRepository
    {
        public ScheduledEventRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduledEventRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ScheduledEvent> GetAsync(string slug)
        {
            return await DbSet
                .Where(_ => _.IsPublished && _.Slug == slug)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<ScheduledEvent>> GetUpcomingSummaryAsync(int locationId,
            DateTime asOf,
            int take)
        {
            return await _context.ScheduledEventLocations
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId
                    && _.ScheduledEvent.IsPublished
                    && _.ScheduledEvent.StartDate >= asOf)
                .OrderBy(_ => _.ScheduledEvent.StartDate)
                .Take(take)
                .Select(_ => new ScheduledEvent
                {
                    DescriptionSegmentId = _.ScheduledEvent.DescriptionSegmentId,
                    DurationMinutes = _.ScheduledEvent.DurationMinutes,
                    Id = _.ScheduledEvent.Id,
                    IsAllDay = _.ScheduledEvent.IsAllDay,
                    MaxPeople = _.ScheduledEvent.MaxPeople,
                    Slug = _.ScheduledEvent.Slug,
                    StartDate = _.ScheduledEvent.StartDate,
                    TitleSegmentId = _.ScheduledEvent.TitleSegmentId
                })
                .ToListAsync();
        }
    }
}