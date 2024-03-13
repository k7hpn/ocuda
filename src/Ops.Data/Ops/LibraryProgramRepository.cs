using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class LibraryProgramRepository : OpsRepository<OpsContext, LibraryProgram, int>,
        ILibraryProgramRepository
    {
        public LibraryProgramRepository(Repository<OpsContext> repositoryFacade,
            ILogger<LibraryProgramRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int?> GetIdByEventIdAsync(int scheduledEventId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ScheduledEventId == scheduledEventId)
                .Select(_ => _.Id)
                .SingleOrDefaultAsync();
        }

        public async Task<CollectionWithCount<LibraryProgram>> PaginateAsync(BaseFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var query = DbSet.Where(_ => !_.IsDeleted);

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

            return new CollectionWithCount<LibraryProgram>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.StartDate)
                    .ApplyPagination(filter)
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        public async Task UpdateScheduledEventIdAsync(int libraryProgramId, int scheduledEventId)
        {
            var libraryProgram = await FindAsync(libraryProgramId)
                ?? throw new OcudaException($"Unable to find library program id {libraryProgramId}");
            libraryProgram.ScheduledEventId = scheduledEventId;
            DbSet.Update(libraryProgram);
            await _context.SaveChangesAsync();
        }
    }
}