using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class ScheduledEventService : BaseService<ScheduledEventService>
    {
        private readonly IScheduledEventRepository _scheduledEventRepository;
        private readonly SegmentService _segmentService;
        private readonly LocationService _locationService;

        public ScheduledEventService(ILogger<ScheduledEventService> logger,
            IDateTimeProvider dateTimeProvider,
            SegmentService segmentService,
            IScheduledEventRepository scheduledEventRepository) : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(scheduledEventRepository);
            ArgumentNullException.ThrowIfNull(segmentService);

            _scheduledEventRepository = scheduledEventRepository;
            _segmentService = segmentService;
        }

        public async Task<ScheduledEvent> GetAsync(bool forceReload, string slug)
        {
            // caching
            var scheduledEvent = await _scheduledEventRepository.GetAsync(slug);

            scheduledEvent.Title = await GetSegmentTestAsync(_segmentService,
                forceReload,
                scheduledEvent.TitleSegmentId);
            scheduledEvent.Description = await GetSegmentTestAsync(_segmentService,
                forceReload,
                scheduledEvent.DescriptionSegmentId);

            if(scheduledEvent.LocationDescriptionId != null)
            {
                scheduledEvent.LocationDescription = await GetSegmentTestAsync(_segmentService,
                    forceReload,
                    scheduledEvent.LocationDescriptionId.Value);
            }

            return scheduledEvent;
        }

        public async Task<IEnumerable<ScheduledEvent>> GetUpcomingAsync(bool forceReload,
            int locationId,
            int take)
        {
            var upcomingEvents = await _scheduledEventRepository
                .GetUpcomingSummaryAsync(locationId, _dateTimeProvider.Now, take);

            foreach (var upcomingEvent in upcomingEvents)
            {
                var description = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(upcomingEvent.DescriptionSegmentId,
                        forceReload);
                upcomingEvent.Description = description.Text;
                var title = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(upcomingEvent.TitleSegmentId, forceReload);
                upcomingEvent.Title = title.Text;
            }

            return upcomingEvents;
        }
    }
}