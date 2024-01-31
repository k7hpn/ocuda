using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class ScheduledEventService : BaseService<ScheduledEventService>
    {
        private const int CacheAgeGroupSegmentMappingsHours = 4;
        private readonly IAgeGroupRepository _ageGroupRepository;
        private readonly IOcudaCache _cache;
        private readonly IScheduledEventAgeGroupRepository _scheduledEventAgeGroupRepository;
        private readonly IScheduledEventRepository _scheduledEventRepository;
        private readonly SegmentService _segmentService;

        public ScheduledEventService(ILogger<ScheduledEventService> logger,
            IDateTimeProvider dateTimeProvider,
            IAgeGroupRepository ageGroupRepository,
            IOcudaCache cache,
            IScheduledEventAgeGroupRepository scheduledEventAgeGroupRepository,
            IScheduledEventRepository scheduledEventRepository,
            SegmentService segmentService) : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(ageGroupRepository);
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(scheduledEventAgeGroupRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRepository);
            ArgumentNullException.ThrowIfNull(segmentService);

            _ageGroupRepository = ageGroupRepository;
            _cache = cache;
            _scheduledEventAgeGroupRepository = scheduledEventAgeGroupRepository;
            _scheduledEventRepository = scheduledEventRepository;
            _segmentService = segmentService;
        }

        public async Task<ScheduledEvent> GetAsync(bool forceReload, string slug)
        {
            // TODO caching
            var scheduledEvent = await _scheduledEventRepository.GetAsync(slug);

            scheduledEvent.Title = await GetSegmentTestAsync(_segmentService,
                forceReload,
                scheduledEvent.TitleSegmentId);
            scheduledEvent.Description = await GetSegmentTestAsync(_segmentService,
                forceReload,
                scheduledEvent.DescriptionSegmentId);

            if (scheduledEvent.LocationDescriptionId != null)
            {
                scheduledEvent.LocationDescription = await GetSegmentTestAsync(_segmentService,
                    forceReload,
                    scheduledEvent.LocationDescriptionId.Value);
            }

            var ageGroupIds = await _scheduledEventAgeGroupRepository
                .GetByScheduledEventId(scheduledEvent.Id);

            var segmentIds = await GetAgeGroupSegmentIds(ageGroupIds);

            foreach (var segmentId in segmentIds)
            {
                var displayText = await GetSegmentTestAsync(_segmentService,
                    forceReload,
                    segmentId);

                scheduledEvent.AgeGroups.Add(displayText);
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

        private async Task<IEnumerable<int>> GetAgeGroupSegmentIds(IEnumerable<int> ageGroupIds)
        {
            var segmentIds = new List<int>();
            var needToLookupSegmentIds = new List<int>();

            foreach (var ageGroupId in ageGroupIds)
            {
                var cacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromAgeGroupIdToSegmentId,
                    ageGroupId);
                var cachedSegmentId = await _cache.GetIntFromCacheAsync(cacheKey);
                if (cachedSegmentId.HasValue)
                {
                    segmentIds.Add(cachedSegmentId.Value);
                }
                else
                {
                    needToLookupSegmentIds.Add(ageGroupId);
                }
            }

            if (needToLookupSegmentIds?.Any() == true)
            {
                var lookups = await _ageGroupRepository.GetAgeGroupsAsync(needToLookupSegmentIds);

                foreach (var ageGroup in lookups)
                {
                    segmentIds.Add(ageGroup.SegmentId);
                    var cacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromAgeGroupIdToSegmentId,
                        ageGroup.Id);
                    await _cache.SaveToCacheAsync(cacheKey,
                        ageGroup.SegmentId,
                        TimeSpan.FromHours(CacheAgeGroupSegmentMappingsHours));
                }
            }

            return segmentIds;
        }
    }
}