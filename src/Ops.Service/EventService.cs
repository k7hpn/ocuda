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
using Ocuda.Ops.Service.Interfaces.Ops.Services;
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
        private readonly IScheduledEventRegistrationHistoryRepository _registrationHistoryRepository;
        private readonly IScheduledEventRegistrationRepository _registrationRepository;
        private readonly IScheduledEventLocationRepository _scheduledEventLocationRepository;
        private readonly IScheduledEventRepository _scheduledEventRepository;
        private readonly ISegmentService _segmentService;
        private readonly IUserService _userService;

        public EventService(ILogger<EventService> logger,
            IHttpContextAccessor httpContextAccessor,
            IAgeGroupRepository ageGroupRepository,
            ILanguageService languageService,
            IScheduledEventLocationRepository scheduledEventLocationRepository,
            IScheduledEventRegistrationHistoryRepository scheduledEventRegistrationHistoryRepository,
            IScheduledEventRegistrationRepository scheduledEventRegistrationRepository,
            IScheduledEventRepository scheduledEventRepository,
            ISegmentService segmentService,
            IUserService userService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(ageGroupRepository);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(scheduledEventLocationRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRegistrationHistoryRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRegistrationRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRepository);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(userService);

            _ageGroupRepository = ageGroupRepository;
            _languageService = languageService;
            _registrationHistoryRepository = scheduledEventRegistrationHistoryRepository;
            _registrationRepository = scheduledEventRegistrationRepository;
            _scheduledEventLocationRepository = scheduledEventLocationRepository;
            _scheduledEventRepository = scheduledEventRepository;
            _segmentService = segmentService;
            _userService = userService;
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
                MaxPeople = libraryProgram.MaxPeople,
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
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
            var scheduledEvent = await _scheduledEventRepository.GetAsync(eventId);
            var segment = await _segmentService.
                GetBySegmentAndLanguageAsync(scheduledEvent.TitleSegmentId, defaultLanguageId);
            if (segment != null)
            {
                scheduledEvent.Title = segment.Text;
            }

            return scheduledEvent;
        }

        public async Task<ScheduledEventRegistration> GetEventRegistrationAsync(Guid id)
        {
            return await _registrationRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ScheduledEventRegistration>>
                    GetEventRegistrationsAsync(int eventId)

        {
            return await _registrationRepository.GetActiveByIdAsync(eventId);
        }

        public async Task<IDictionary<int, int>>
            GetRegistrationCountAsync(IEnumerable<int> registrationEventIds)
        {
            IDictionary<int, int> registrationCount = new Dictionary<int, int>();

            if (registrationEventIds.Any())
            {
                registrationCount = await _registrationRepository
                    .GetRegistrationCountAsync(registrationEventIds);
            }

            return registrationCount;
        }

        public async Task<IEnumerable<ScheduledEventRegistrationHistory>>
                    GetRegistrationHistoryAsync(Guid scheduledEventRegistrationId)
        {
            var histories = await _registrationHistoryRepository
                .GetByRegistrationIdAsync(scheduledEventRegistrationId);

            foreach (var history in histories.Where(_ => _.StaffId.HasValue))
            {
                history.StaffUser = await _userService.GetByIdAsync(history.StaffId.Value);
            }

            return histories;
        }

        public async Task<bool> IsSlugInUseAsync(string slug)
        {
            return await _scheduledEventRepository.IsSlugInUseAsync(slug);
        }

        public async Task<CollectionWithCount<ScheduledEvent>> PaginateAsync(BaseFilter filter)
        {
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
            var scheduledEvents = await _scheduledEventRepository.PaginateAsync(filter);

            // get registration counts
            var registrationCount = await GetRegistrationCountAsync(scheduledEvents.Data
                .Where(_ => _.MaxPeople > 0)
                .Select(_ => _.Id));

            foreach (var scheduledEvent in scheduledEvents.Data)
            {
                var segment = await _segmentService
                    .GetBySegmentAndLanguageAsync(scheduledEvent.TitleSegmentId, defaultLanguageId);
                if (segment != null)
                {
                    scheduledEvent.Title = segment.Text;
                }

                if (registrationCount.TryGetValue(scheduledEvent.Id, out int value))
                {
                    scheduledEvent.RegistrationCount = value;
                }
            }

            return scheduledEvents;
        }
    }
}