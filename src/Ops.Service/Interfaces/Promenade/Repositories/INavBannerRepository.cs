﻿using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavBannerRepository : IGenericRepository<NavBanner>
    {
        Task<NavBanner> GetByIdAsync(int navBannerId);
        Task<int?> GetPageLayoutIdAsync(int id);
        Task<int?> GetPageHeaderIdAsync(int id);
    }
}
