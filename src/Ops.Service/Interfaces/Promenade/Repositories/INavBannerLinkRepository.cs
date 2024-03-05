﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavBannerLinkRepository : IGenericRepository<NavBannerLink>
    {
        Task<List<NavBannerLink>> GetLinksByNavBannerIdAsync(int navBannerId);
    }
}
