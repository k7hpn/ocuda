using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class EventService : BaseService<EventService>, IEventService
    {
        private readonly IAgeGroupRepository _ageGroupRepository;
        private readonly ILanguageService _languageService;
        private readonly IScheduledEventLocationRepository _scheduledEventLocationRepository;
        private readonly IScheduledEventRepository _scheduledEventRepository;
        private readonly ISegmentService _segmentService;

        public EventService(ILogger<EventService> logger,
            IHttpContextAccessor httpContextAccessor,
            IAgeGroupRepository ageGroupRepository,
            ILanguageService languageService,
            IScheduledEventLocationRepository scheduledEventLocationRepository,
            IScheduledEventRepository scheduledEventRepository,
            ISegmentService segmentService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(ageGroupRepository);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(scheduledEventLocationRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRepository);
            ArgumentNullException.ThrowIfNull(segmentService);

            _ageGroupRepository = ageGroupRepository;
            _languageService = languageService;
            _scheduledEventLocationRepository = scheduledEventLocationRepository;
            _scheduledEventRepository = scheduledEventRepository;
            _segmentService = segmentService;
        }

        public async Task<AgeGroup> AddSaveAgeGroupAsync(AgeGroup ageGroup)
        {
            await _ageGroupRepository.AddAsync(ageGroup);
            await _ageGroupRepository.SaveAsync();

            return ageGroup;
        }

        public async Task<ScheduledEvent> CreateEventAsync(LibraryProgram libraryProgram,
            string slug)
        {
            ArgumentNullException.ThrowIfNull(libraryProgram);

            var scheduledEvent = new ScheduledEvent
            {
                DescriptionSegmentId = libraryProgram.DescriptionSegmentId,
                DurationMinutes = libraryProgram.DurationMinutes,
                ExternalId = libraryProgram.HistoricId,
                IsAllDay = libraryProgram.IsAllDay,
                IsPublished = true,
                LocationDescriptionId = libraryProgram.LocationDescriptionSegmentId,
                ScheduledEventType = ScheduledEventType.Program,
                Slug = slug,
                StartDate = libraryProgram.StartDate,
                TitleSegmentId = libraryProgram.TitleSegmentId
            };
            await _scheduledEventRepository.AddAsync(scheduledEvent);

            try
            {
                await _scheduledEventRepository.SaveAsync();
            }
            catch (DbUpdateException duex)
            {
                _logger.LogError(duex,
                    "Could not create event for library program {ProgramId}: {ErrorMessage}",
                    libraryProgram.Id,
                    duex.Message);
                throw new OcudaException($"Could not create event for library program id: {libraryProgram.Id}.", duex);
            }

            if (!libraryProgram.IsOnline)
            {
                await _scheduledEventLocationRepository.AddAsync(new ScheduledEventLocation
                {
                    LocationId = libraryProgram.LocationId,
                    ScheduledEventId = scheduledEvent.Id
                });

                try
                {
                    await _scheduledEventLocationRepository.SaveAsync();
                }
                catch (DbUpdateException duex)
                {
                    _logger.LogError(duex,
                        "Could not associate event for library program {ProgramId} with location {LocationId}: {ErrorMessage}",
                        libraryProgram.Id,
                        libraryProgram.LocationId,
                        duex.Message);
                    throw new OcudaException($"Could not associate event for library program {libraryProgram.Id} with location {libraryProgram.LocationId}.",
                        duex);
                }
            }

            return scheduledEvent;
        }

        public async Task<ICollection<AgeGroup>> GetAgeGroupLookupAsync()
        {
            return await _ageGroupRepository.GetAllAsync();
        }

        public async Task<ScheduledEvent> GetEventAsync(int eventId)
        {
            return await _scheduledEventRepository.GetAsync(eventId);
        }

        public async Task<bool> IsSlugInUseAsync(string slug)
        {
            return await _scheduledEventRepository.IsSlugInUseAsync(slug);
        }

        public async Task<CollectionWithCount<ScheduledEvent>> PaginateAsync(BaseFilter filter)
        {
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();

            // TODO this could use some caching

            var scheduledEvents = await _scheduledEventRepository.PaginateAsync(filter);

            foreach (var scheduledEvent in scheduledEvents.Data)
            {
                var segment = await _segmentService
                    .GetBySegmentAndLanguageAsync(scheduledEvent.TitleSegmentId, defaultLanguageId);
                if (segment != null)
                {
                    scheduledEvent.Title = segment.Text;
                }
            }
            return scheduledEvents;
        }
    }
}