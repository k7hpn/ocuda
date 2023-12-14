using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class EventService : BaseService<EventService>, IEventService
    {
        public IAgeGroupRepository _ageGroupRepository;
        public IScheduledEventRepository _scheduledEventRepository;
        public EventService(ILogger<EventService> logger,
            IHttpContextAccessor httpContextAccessor,
            IAgeGroupRepository ageGroupRepository,
            IScheduledEventRepository scheduledEventRepository) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(ageGroupRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRepository);

            _ageGroupRepository = ageGroupRepository;
            _scheduledEventRepository = scheduledEventRepository;
        }

        public async Task<AgeGroup> AddSaveAgeGroupAsync(AgeGroup ageGroup)
        {
            await _ageGroupRepository.AddAsync(ageGroup);
            await _ageGroupRepository.SaveAsync();

            return ageGroup;
        }

        public async Task<ICollection<AgeGroup>> GetAgeGroupLookupAsync()
        {
            return await _ageGroupRepository.GetAllAsync();
        }

        public async Task<ScheduledEvent> GetEventAsync(ScheduledEventType ScheduledEventType, int itemId)
        {
            return await _scheduledEventRepository.GetAsync(ScheduledEventType, itemId);
        }

        public async Task<CollectionWithCount<ScheduledEvent>> PaginateAsync(BaseFilter filter)
        {
            return new CollectionWithCount<ScheduledEvent>
            {
                Count = 0,
                Data = new List<ScheduledEvent>()
            };
        }
    }
}