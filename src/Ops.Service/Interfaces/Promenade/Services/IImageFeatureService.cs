﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IImageFeatureService
    {
        Task<ImageFeatureTemplate> AddTemplateAsync(int imageFeatureId,
            int pageLayoutId,
            ImageFeatureTemplate imageFeatureTemplate);

        Task ClearTemplateForImageFeatureAsync(int imageFeatureTemplateId);

        Task<ImageFeatureItem> CreateItemAsync(ImageFeatureItem imageFeatureItem);

        Task<ImageFeature> CreateNoSaveAsync(ImageFeature imageFeature);

        Task DeleteItemAsync(int imageFeatureItemId);

        Task DeleteNoSaveAsync(int id);

        Task<ImageFeature> EditAsync(ImageFeature imageFeature);

        Task<ImageFeatureItem> EditItemAsync(ImageFeatureItem imageFeatureItem);

        Task<ICollection<ImageFeatureTemplate>> GetAllTemplatesAsync();

        Task<ImageFeature> GetImageFeatureDetailsAsync(int id, int languageId);

        Task<string> GetImageFeaturePathAsync(string languageName);

        Task<ImageFeatureItem> GetItemByIdAsync(int id);

        Task<ImageFeatureItemText> GetItemTextByIdsAsync(int imageFeatureItemId, int languageId);

        Task<int?> GetPageHeaderIdForImageFeatureAsync(int id);

        Task<int> GetPageLayoutIdForImageFeatureAsync(int id);

        Task<ImageFeatureTemplate> GetTemplateForImageFeatureAsync(int id);

        Task<ImageFeatureItemText> SetItemTextAsync(ImageFeatureItemText itemText);

        Task UpdateItemSortOrder(int id, bool increase);

        Task UpdateTemplateAsync(ImageFeatureTemplate imageFeatureTemplate);
    }
}