﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class ExternalResourcePromRepository
        : GenericRepository<PromenadeContext, ExternalResource>,
        IExternalResourcePromRepository
    {
        public ExternalResourcePromRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ExternalResourcePromRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ExternalResource> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type)
        {
            var externalResources = DbSet
                .AsNoTracking();

            if (type.HasValue)
            {
                externalResources = externalResources
                    .Where(_ => _.Type == type.Value);
            }

            return await externalResources
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<ExternalResource>>> GetPaginatedListAsync(
            ExternalResourceFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.ExternalResourceType.HasValue)
            {
                query = query
                    .Where(_ => _.Type == filter.ExternalResourceType.Value);
            }

            return new DataWithCount<ICollection<ExternalResource>>
            {
                Count = await query.CountAsync(),
                Data = await query
                .OrderBy(_ => _.SortOrder)
                .ApplyPagination(filter)
                .ToListAsync()
            };
        }

        public async Task<int?> GetMaxSortOrderAsync(ExternalResourceType type)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Type == type)
                .MaxAsync(_ => (int?)_.SortOrder);
        }

        public async Task<ExternalResource> GetBySortOrderAsync(ExternalResourceType type,
            int sortOrder)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Type == type && _.SortOrder == sortOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ExternalResource>> GetSubsequentAsync(
            ExternalResourceType type, int sortOrder)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Type == type && _.SortOrder > sortOrder)
                .ToListAsync();
        }
    }
}
