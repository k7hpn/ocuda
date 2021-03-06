﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageHeaderRepository : IGenericRepository<PageHeader>
    {
        Task<PageHeader> FindAsync(int id);
        Task<DataWithCount<ICollection<PageHeader>>> GetPaginatedListAsync(PageFilter filter);
        Task<ICollection<string>> GetLanguagesByIdAsync(int id);
        Task<bool> StubInUseAsync(PageHeader header);
    }
}
