﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ImageAltTextRepository : GenericRepository<PromenadeContext, LocationInteriorImageAltText>,
        IImageAltTextRepository
    {
        public ImageAltTextRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ImageAltTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<LocationInteriorImageAltText>> GetAllLanguageImageAltTextsAsync(int imageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationInteriorImageId == imageId)
                .Include(_ => _.Language)
                .ToListAsync();
        }

        public async Task<LocationInteriorImageAltText> GetImageAltTextAsync(int imageId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationInteriorImageId == imageId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }
    }
}