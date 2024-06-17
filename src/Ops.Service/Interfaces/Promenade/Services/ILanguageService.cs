﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ILanguageService
    {
        Task<ICollection<Language>> GetActiveAsync();

        Task<Language> GetActiveByIdAsync(int id);

        Task<IDictionary<int, string>> GetActiveNamesAsync();

        Task<int> GetDefaultLanguageId();

        Task<int> GetDefaultLanguageIdAsync();
    }
}