﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Ops
{
    public class GroupRepository : GenericRepository<PromenadeContext,Group, int>, IGroupRepository
    {
        public GroupRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.GroupType)
                .ToListAsync();
        }

        public async Task<int> CountAsync(GroupFilter filter)
        {
            return await ApplyFilters(filter)
                .CountAsync();
        }

        public async Task<ICollection<Group>> PageAsync(GroupFilter filter)
        {
            return await ApplyFilters(filter)
                .OrderBy(_ => _.GroupType)
                .ApplyPagination(filter)
                .ToListAsync();
        }

        private IQueryable<Group> ApplyFilters(GroupFilter filter)
        {
            var items = DbSet.AsNoTracking();

            if (filter.GroupIds?.Count > 0)
            {
                items = items.Where(_ => !filter.GroupIds.Contains(_.Id));
            }

            return items;
        }

        public async Task<List<Group>> GetAllGroupRegions()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsLocationRegion)
                .OrderBy(_ => _.GroupType)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Group>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.GroupType)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> IsDuplicateGroupTypeAsync(Group group)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != group.Id && _.GroupType == group.GroupType)
                .AnyAsync();
        }

        public async Task<bool> IsDuplicateStubAsync(Group group)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != group.Id && _.Stub == group.Stub)
                .AnyAsync();
        }

        public async Task<Group> GetGroupByStubAsync(string groupStub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == groupStub)
                .FirstOrDefaultAsync();
        }
    }
}