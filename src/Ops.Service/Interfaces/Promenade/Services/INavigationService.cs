﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface INavigationService
    {
        Task<Navigation> CreateAsync(Navigation navigation, string siteSetting = null);

        Task DeleteAsync(int id);

        Task DeleteNavigationTextAsync(int navigationId, int languageId);

        Task<Navigation> EditAsync(Navigation navigation);

        Task<Navigation> GetByIdAsync(int id);

        Task<ICollection<Navigation>> GetNavigationChildrenAsync(int id);

        Task<ICollection<string>> GetNavigationLanguagesByIdAsync(int id);

        Task<NavigationRoles> GetNavigationRolesAsync();

        Task<IEnumerable<Navigation>> GetNavigationTreeAsync(int navigationId);

        Task<RoleProperties> GetRolePropertiesForNavigationAsync(int id);

        Task<int> GetSubnavigationCountAsync(int id);

        Task<NavigationText> GetTextByNavigationAndLanguageAsync(int navigationId, int languageId);

        Task<ICollection<Navigation>> GetTopLevelNavigationsAsync();

        Task ReplaceAllNavigationAsync(IEnumerable<Navigation> navigations);

        Task SetNavigationTextAsync(NavigationText navigationText);

        Task UpdateSortOrder(int id, bool increase);
    }
}