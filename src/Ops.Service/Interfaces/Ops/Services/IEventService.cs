using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IEventService
    {
        public Task<IEnumerable<ScheduledEvent>> ScheduledEvents { get; set; }

        public Task<DataImportResult> ImportAsync(int currentUserId, string filePath);
    }
}