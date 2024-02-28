﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Feature;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.NavBannerViewModels;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class NavBannerController : BaseController<NavBannerController>
    {
        private readonly ILanguageService _languageService;
        private readonly INavBannerService _navBannerService;

        public static string Name { get { return "NavBanner"; } }
        public static string Area { get { return "SiteManagement"; } }

        public NavBannerController(ServiceFacades.Controller<NavBannerController> context,
            ILanguageService languageService,
            INavBannerService navBannerService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navBannerService);

            _languageService = languageService;
            _navBannerService = navBannerService;
        }

        [HttpGet]
        [Route("{action}/{id}")]
        public async Task<IActionResult> Detail(int id, string language)
        {
            try
            {
                var navBanner = await _navBannerService.GetByIdAsync(id);

                var languages = await _languageService.GetActiveAsync();

                var selectedLanguage = languages
                    .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                    ?? languages.Single(_ => _.IsDefault);

                var viewModel = new DetailViewModel
                {
                    Name = navBanner.Name,
                    LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                    PageLayoutId = await _navBannerService.GetPageLayoutIdForNavBannerAsync(navBanner.Id)
                };


                return View(viewModel);
            }
            catch (OcudaException oex)
            {
                Console.WriteLine(oex.Message);
                return NotFound();
            }
        }
    }
}