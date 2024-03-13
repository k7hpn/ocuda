using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILibraryProgramService
    {
        public Task<ScheduledEvent> CreateEventAsync(int libraryProgramId);

        public Task<LibraryProgram> GetAsync(int id);

        public Task<int?> GetIdByEventIdAsync(int scheduledEventId);

        public Task<Ocuda.Ops.Models.Portable.ImportResult> ImportAsync(int importUserId,
            string filename,
            bool performImport,
            bool createEvents);

        public Task<CollectionWithCount<LibraryProgram>> PaginateAsync(BaseFilter filter);
    }
}