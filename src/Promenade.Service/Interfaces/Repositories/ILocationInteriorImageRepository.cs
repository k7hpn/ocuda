﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationInteriorImageRepository : IGenericRepository<LocationInteriorImage>
    {
        Task<ICollection<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId);
    }
}