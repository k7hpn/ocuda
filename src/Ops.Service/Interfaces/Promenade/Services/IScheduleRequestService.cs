﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IScheduleRequestService
    {
        public Task<IEnumerable<ScheduleRequest>> GetRequestsAsync(DateTime requestedDate);
        public Task<ScheduleRequest> GetRequestAsync(int requestId);
        public Task<IEnumerable<ScheduleRequest>> GetUnclaimedRequestsAsync();
    }
}
