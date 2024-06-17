﻿using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class NavBannerLinkTextRepository
        : GenericRepository<PromenadeContext, NavBannerLinkText>,
        INavBannerLinkTextRepository
    {
        public NavBannerLinkTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<NavBannerLinkText> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavBannerLinkText> GetByLinkIdAsync(int id, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.NavBannerLinkId == id && _.LanguageId == languageId);
        }
    }
}