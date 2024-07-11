﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class PagesController : BaseController<PagesController>
    {
        private readonly ICarouselService _carouselService;
        private readonly IDeckService _deckService;
        private readonly IImageFeatureService _imageFeatureService;
        private readonly ILanguageService _languageService;
        private readonly INavBannerService _navBannerService;
        private readonly IPageService _pageService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISegmentService _segmentService;
        private readonly ISocialCardService _socialCardService;

        public PagesController(ServiceFacades.Controller<PagesController> context,
            ICarouselService carouselService,
            IDeckService deckService,
            ILanguageService languageService,
            INavBannerService navBannerService,
            IPageService pageService,
            IPermissionGroupService permissionGroupService,
            ISegmentService segmentService,
            ISocialCardService socialCardService,
            IImageFeatureService imageFeatureService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(carouselService);
            ArgumentNullException.ThrowIfNull(deckService);
            ArgumentNullException.ThrowIfNull(imageFeatureService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navBannerService);
            ArgumentNullException.ThrowIfNull(pageService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(socialCardService);

            _carouselService = carouselService;
            _deckService = deckService;
            _imageFeatureService = imageFeatureService;
            _languageService = languageService;
            _navBannerService = navBannerService;
            _pageService = pageService;
            _permissionGroupService = permissionGroupService;
            _segmentService = segmentService;
            _socialCardService = socialCardService;
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Pages"; } }

        [HttpPost]
        [Route("[action]/{headerId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddPermissionGroup(int headerId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .AddToPermissionGroupAsync<PermissionGroupPageContent>(headerId,
                    permissionGroupId);
                AlertInfo = "Content permission added.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem adding permission: {ex.Message}";
            }

            return RedirectToAction(nameof(ContentPermissions), new { id = headerId });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ChangeSort(int id, bool increase)
        {
            JsonResponse response;

            var layout = await _pageService.GetLayoutForItemAsync(id);

            if (await HasPagePermissionsAsync(layout.PageHeaderId))
            {
                try
                {
                    await _pageService.UpdateItemSortOrder(id, increase);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CloneLayout(int clonePageHeaderId,
            int cloneLayoutId,
            string clonedName)
        {
            if (!await HasPagePermissionsAsync(clonePageHeaderId))
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(clonedName))
            {
                ShowAlertWarning("You must provide a name for a cloned layout.");
                return RedirectToAction(nameof(Layouts), new { id = clonePageHeaderId });
            }

            try
            {
                var priorLayout = await _pageService.GetLayoutByIdAsync(cloneLayoutId);
                var layout = await _pageService
                    .CloneLayoutAsync(clonePageHeaderId, cloneLayoutId, clonedName);

                var pageHeader = await _pageService.GetHeaderByIdAsync(clonePageHeaderId);

                if (layout != null)
                {
                    _logger.LogInformation("Page {Page} layout {Layout} cloned into {Name}",
                        pageHeader.PageName,
                        priorLayout.Name,
                        layout.Name);

                    ShowAlertSuccess("Layout successfully cloned!");
                    return RedirectToAction(nameof(LayoutDetail), new { id = layout.Id });
                }
                else
                {
                    _logger.LogError("Error cloning layout, null layout object returned.");
                    ShowAlertDanger("Error cloning layout.");
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Error cloning layout {LayoutId}: {ErrorMessage}",
                    cloneLayoutId,
                    oex.Message);
                ShowAlertDanger($"Issue cloning layout: {oex.Message}");
            }

            return RedirectToAction(nameof(Layouts), new { id = clonePageHeaderId });
        }

        [Route("[action]/{id}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> ContentPermissions(int id)
        {
            var header = await _pageService.GetHeaderByIdAsync(id);

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var pagePermissions = await _permissionGroupService
                .GetPermissionsAsync<PermissionGroupPageContent>(id);

            var availableGroups = new Dictionary<int, string>();
            var assignedGroups = new Dictionary<int, string>();

            foreach (var permissionGroup in permissionGroups)
            {
                var permission = pagePermissions
                    .SingleOrDefault(_ => _.PermissionGroupId == permissionGroup.Id);
                if (permission == null)
                {
                    availableGroups.Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
                else
                {
                    assignedGroups.Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
            }

            return View(new ContentPermissionsViewModel
            {
                HeaderName = header.PageName,
                HeaderStub = header.Stub,
                HeaderType = header.Type,
                HeaderId = header.Id,
                AvailableGroups = availableGroups,
                AssignedGroups = assignedGroups
            });
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            JsonResponse response;

            if (!model.PageHeader.Stub.All(_ => char.IsLetterOrDigit(_) || _ == '-' || _ == '_'))
            {
                ModelState.AddModelError("PageHeader.Stub", "Invalid stub; only letters, numbers, hyphens and underscores are allowed.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!model.PageHeader.IsLayoutPage)
                    {
                        model.PageHeader.LayoutBannerTemplate = null;
                        model.PageHeader.LayoutCarouselTemplateId = null;
                        model.PageHeader.LayoutFeatureTemplateId = null;
                        model.PageHeader.LayoutWebslideTemplateId = null;
                    }

                    var header = await _pageService.CreateHeaderAsync(model.PageHeader);
                    response = new JsonResponse
                    {
                        Success = true
                    };

                    if (header.IsLayoutPage)
                    {
                        response.Url = Url.Action(nameof(Layouts), new { id = header.Id });
                    }
                    else
                    {
                        response.Url = Url.Action(nameof(Detail), new { id = header.Id });
                    }

                    _logger.LogInformation("Page {Name} (slug {Slug}) of type {Type} created",
                        header.PageName,
                        header.Stub,
                        header.Type);

                    ShowAlertSuccess($"Created page: {header.PageName}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateLayout(LayoutsViewModel model)
        {
            JsonResponse response;

            if (model != null && await HasPagePermissionsAsync(model.PageLayout.PageHeaderId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        model.PageLayout.SocialCardId = null;

                        if (model.StartDate.HasValue && model.StartTime.HasValue)
                        {
                            model.PageLayout.StartDate =
                                model.StartDate.Value.CombineWithTime(model.StartTime.Value);
                        }

                        var layout = await _pageService.CreateLayoutAsync(model.PageLayout);
                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(LayoutDetail), new { id = layout.Id })
                        };

                        var pageHeader = await _pageService.GetHeaderByIdAsync(layout.PageHeaderId);

                        _logger.LogInformation("Page layout {Name} for page {PageHeader} with start time {StartTime} created",
                            layout.Name,
                            pageHeader.PageName,
                            layout.StartDate?.ToString(CultureInfo.CurrentCulture)
                                ?? "unspecified");

                        ShowAlertSuccess($"Created layout: {layout.Name}");
                    }
                    catch (OcudaException ex)
                    {
                        response = new JsonResponse
                        {
                            Success = false,
                            Message = ex.Message
                        };
                    }
                }
                else
                {
                    var errors = ModelState.Values
                        .SelectMany(_ => _.Errors)
                        .Select(_ => _.ErrorMessage);

                    response = new JsonResponse
                    {
                        Success = false,
                        Message = string.Join(Environment.NewLine, errors)
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreatePageItem(LayoutDetailViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            JsonResponse response;

            var layout = await _pageService.GetLayoutByIdAsync(model.PageItem.PageLayoutId);

            if (await HasPagePermissionsAsync(layout.PageHeaderId))
            {
                if (model.BannerFeature == null
                    && model.Carousel == null
                    && model.Deck == null
                    && model.PageFeature == null
                    && model.NavBanner == null
                    && model.Segment == null
                    && model.Webslide == null)
                {
                    ModelState.AddModelError("PageItem", "No content to add.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(model.BannerFeature?.Name))
                        {
                            model.PageItem.BannerFeature = model.BannerFeature;
                        }
                        else if (!string.IsNullOrEmpty(model.Carousel?.Name))
                        {
                            model.PageItem.Carousel = model.Carousel;
                        }
                        else if (!string.IsNullOrEmpty(model.Deck?.Name))
                        {
                            model.PageItem.Deck = model.Deck;
                        }
                        else if (!string.IsNullOrEmpty(model.NavBanner?.Name))
                        {
                            model.PageItem.NavBanner = model.NavBanner;
                        }
                        else if (!string.IsNullOrEmpty(model.PageFeature?.Name))
                        {
                            model.PageItem.PageFeature = model.PageFeature;
                        }
                        else if (!string.IsNullOrEmpty(model.Segment?.Name))
                        {
                            model.Segment.IsActive = true;
                            model.Segment.EndDate = null;
                            model.Segment.StartDate = null;
                            model.PageItem.Segment = model.Segment;
                        }
                        else if (!string.IsNullOrEmpty(model.Webslide?.Name))
                        {
                            model.PageItem.Webslide = model.Webslide;
                        }
                        else
                        {
                            return Json(new JsonResponse
                            {
                                Success = false,
                                Message = "You must supply an item name."
                            });
                        }

                        var pageItem = await _pageService.CreateItemAsync(model.PageItem);

                        string url = null;
                        if (pageItem.BannerFeature != null)
                        {
                            var header = await _pageService.GetHeaderByIdAsync(layout.PageHeaderId);
                            if (!header.LayoutBannerTemplateId.HasValue
                                && pageItem.BannerFeatureId.HasValue)
                            {
                                await _imageFeatureService
                                    .AddTemplateAsync(pageItem.BannerFeatureId.Value,
                                        pageItem.PageLayoutId,
                                        new ImageFeatureTemplate
                                        {
                                            ItemsToDisplay = 1,
                                            MaximumFileSizeBytes = 200 * 1024,
                                            Name = "Banner template",
                                            Width = 2128,
                                        });
                            }

                            url = Url.Action(nameof(ImageFeaturesController.Detail),
                                ImageFeaturesController.Name,
                                new { id = pageItem.BannerFeature.Id });
                        }
                        else if (pageItem.Carousel != null)
                        {
                            url = Url.Action(nameof(CarouselsController.Detail),
                                CarouselsController.Name,
                                new { id = pageItem.Carousel.Id });
                        }
                        else if (pageItem.Deck != null)
                        {
                            url = Url.Action(nameof(DecksController.Detail),
                                DecksController.Name,
                                new { deckId = pageItem.Deck.Id });
                        }
                        else if (pageItem.NavBanner != null)
                        {
                            url = Url.Action(nameof(NavBannerController.Detail),
                                NavBannerController.Name,
                                new { id = pageItem.NavBanner.Id });
                        }
                        else if (pageItem.PageFeature != null)
                        {
                            var header = await _pageService.GetHeaderByIdAsync(layout.PageHeaderId);
                            if (!header.LayoutFeatureTemplateId.HasValue
                                && pageItem.PageFeatureId.HasValue)
                            {
                                await _imageFeatureService
                                   .AddTemplateAsync(pageItem.PageFeatureId.Value,
                                       pageItem.PageLayoutId,
                                       new ImageFeatureTemplate
                                       {
                                           Height = 400,
                                           ItemsToDisplay = 4,
                                           MaximumFileSizeBytes = 100 * 1024,
                                           Name = "Image feature template",
                                           Width = 364,
                                       });
                            }

                            url = Url.Action(nameof(ImageFeaturesController.Detail),
                                ImageFeaturesController.Name,
                                new { id = pageItem.PageFeature.Id });
                        }
                        else if (pageItem.Segment != null)
                        {
                            url = Url.Action(nameof(SegmentsController.Detail),
                                SegmentsController.Name,
                                new { id = pageItem.Segment.Id });
                        }
                        else if (pageItem.Webslide != null)
                        {
                            var header = await _pageService.GetHeaderByIdAsync(layout.PageHeaderId);
                            if (!header.LayoutWebslideTemplateId.HasValue
                                && pageItem.WebslideId.HasValue)
                            {
                                await _imageFeatureService
                                     .AddTemplateAsync(pageItem.WebslideId.Value,
                                         pageItem.PageLayoutId,
                                         new ImageFeatureTemplate
                                         {
                                             Height = 500,
                                             ItemsToDisplay = 4,
                                             MaximumFileSizeBytes = 250 * 1024,
                                             Name = "Image feature template",
                                             Width = 1480,
                                         });
                            }

                            url = Url.Action(nameof(ImageFeaturesController.Detail),
                                ImageFeaturesController.Name,
                                new { id = pageItem.Webslide.Id });
                        }

                        response = new JsonResponse
                        {
                            Success = true,
                            Url = url
                        };

                        _logger.LogInformation("Page item {Item} for layout {Name} created",
                            pageItem.Id,
                            layout.Name);

                        ShowAlertSuccess("Created item for layout");
                    }
                    catch (OcudaException ex)
                    {
                        response = new JsonResponse
                        {
                            Success = false,
                            Message = ex.Message
                        };
                    }
                }
                else
                {
                    var errors = ModelState.Values
                        .SelectMany(_ => _.Errors)
                        .Select(_ => _.ErrorMessage);

                    response = new JsonResponse
                    {
                        Success = false,
                        Message = string.Join(Environment.NewLine, errors)
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            try
            {
                var pageHeader = await _pageService.GetHeaderByIdAsync(model.PageHeader.Id);

                await _pageService.DeleteHeaderAsync(model.PageHeader.Id);
                ShowAlertSuccess($"Deleted page: {pageHeader.PageName}");
                _logger.LogInformation("Page {Page} deleted", pageHeader.PageName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page header: {Message}", ex.Message);
                ShowAlertDanger("Unable to delete page: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new
            {
                page = model.PaginateModel.CurrentPage,
                Type = model.PageType
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteLayout(LayoutsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var pageLayout = await _pageService.GetLayoutByIdAsync(model.PageLayout.Id);

            if (!await HasPagePermissionsAsync(pageLayout.PageHeaderId))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var pageHeader = await _pageService
                    .GetHeaderByIdAsync(pageLayout.PageHeaderId);

                await _pageService.DeleteLayoutAsync(model.PageLayout.Id);
                ShowAlertSuccess($"Deleted layout: {pageLayout.Name}");
                _logger.LogInformation("Page {Page} layout {Layout} deleted",
                    pageHeader.PageName,
                    pageLayout.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page layout: {Message}", ex.Message);
                ShowAlertDanger("Unable to delete layout: ", ex.Message);
            }

            return RedirectToAction(nameof(Layouts), new
            {
                id = pageLayout.PageHeaderId,
                page = model.PaginateModel.CurrentPage
            });
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> DeletePage(DetailViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var page = await _pageService.GetByHeaderAndLanguageAsync(model.HeaderId,
                model.LanguageId);

            await _pageService.DeleteAsync(page);

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            var pageHeader = await _pageService.GetHeaderByIdAsync(page.PageHeaderId);

            ShowAlertSuccess($"Deleted page {language.Description} content!");
            _logger.LogInformation("Page {Page} {Description} content deleted",
                pageHeader.PageName,
                language.Description);

            return RedirectToAction(nameof(Detail),
                new
                {
                    id = model.HeaderId,
                    language = language.Name
                });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeletePageItem(LayoutDetailViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var layout = await _pageService.GetLayoutForItemAsync(model.PageItem.Id);

            if (!await HasPagePermissionsAsync(layout.PageHeaderId))
            {
                return RedirectToUnauthorized();
            }

            var pageHeader = await _pageService.GetHeaderByIdAsync(layout.PageHeaderId);

            try
            {
                await _pageService.DeleteItemAsync(model.PageItem.Id);
                ShowAlertSuccess("Deleted layout item");
                _logger.LogInformation("Page {Page} layout {Layout} item {Item} deleted",
                    pageHeader.PageName,
                    layout.Name,
                    model.PageItem.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page item: {Message}", ex.Message);
                ShowAlertDanger("Error deleting layout item");
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            return RedirectToAction(nameof(LayoutDetail), new
            {
                id = layout.Id,
                language = language.IsDefault ? null : language.Name
            });
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Detail(int id, string language)
        {
            if (!await HasPagePermissionsAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var header = await _pageService.GetHeaderByIdAsync(id);

            if (header.IsLayoutPage)
            {
                return RedirectToAction(nameof(Layouts), new { id = header.Id });
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var page = await _pageService.GetByHeaderAndLanguageAsync(id, selectedLanguage.Id);

            var viewModel = new DetailViewModel
            {
                Page = page,
                HeaderId = header.Id,
                HeaderName = header.PageName,
                HeaderStub = header.Stub,
                HeaderType = header.Type,
                NewPage = page == null,
                LanguageId = selectedLanguage.Id,
                LanguageDescription = selectedLanguage.Description,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                SocialCardList = new SelectList(await _socialCardService.GetListAsync(),
                    nameof(SocialCard.Id), nameof(SocialCard.Title), page?.SocialCardId),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
            };

            if (page?.IsPublished == true)
            {
                var baseUrl = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    viewModel.PageUrl = $"{baseUrl}{selectedLanguage.Name}/{page.PageHeader.Type}/{page.PageHeader.Stub}";
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id?}")]
        [SaveModelState]
        public async Task<IActionResult> Detail(DetailViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!await HasPagePermissionsAsync(model.HeaderId))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            if (ModelState.IsValid)
            {
                var page = model.Page;
                page.LanguageId = language.Id;
                page.PageHeaderId = model.HeaderId;

                var currentPage = await _pageService.GetByHeaderAndLanguageAsync(
                    model.HeaderId, language.Id);

                var pageHeader = await _pageService.GetHeaderByIdAsync(page.PageHeaderId);

                if (currentPage == null)
                {
                    await _pageService.CreateAsync(page);

                    ShowAlertSuccess("Added page content!");
                    _logger.LogInformation("Page {Page} language {Language} content added",
                        pageHeader.PageName,
                        language.Id);
                }
                else
                {
                    await _pageService.EditAsync(page);

                    ShowAlertSuccess("Updated page content!");
                    _logger.LogInformation("Page {Page} language {Language} content updated",
                        pageHeader.PageName,
                        language.Id);
                }
            }

            return RedirectToAction(nameof(Detail), new
            {
                id = model.HeaderId,
                language = language.Name
            });
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Edit(IndexViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var header = await _pageService.EditHeaderAsync(model.PageHeader);
                    response = new JsonResponse { Success = true };
                    ShowAlertSuccess($"Updated page: {header.PageName}");
                    _logger.LogInformation("Page {Page} header updated", header.PageName);
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditLayout(LayoutsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            JsonResponse response;

            var pageLayout = await _pageService.GetLayoutByIdAsync(model.PageLayout.Id);

            if (model != null
                && await HasPagePermissionsAsync(pageLayout.PageHeaderId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        model.PageLayout.SocialCardId = null;

                        if (model.StartDate.HasValue && model.StartTime.HasValue)
                        {
                            model.PageLayout.StartDate =
                                model.StartDate.Value.CombineWithTime(model.StartTime.Value);
                        }

                        var layout = await _pageService.EditLayoutAsync(model.PageLayout);
                        response = new JsonResponse
                        {
                            Success = true
                        };

                        var pageHeader = await _pageService.GetHeaderByIdAsync(layout.PageHeaderId);

                        ShowAlertSuccess($"Updated layout: {layout.Name}");
                        _logger.LogInformation("Page {Page} layout {Name} updated",
                            pageHeader.PageName,
                            layout.Name);
                    }
                    catch (OcudaException ex)
                    {
                        response = new JsonResponse
                        {
                            Success = false,
                            Message = ex.Message
                        };
                    }
                }
                else
                {
                    var errors = ModelState.Values
                        .SelectMany(_ => _.Errors)
                        .Select(_ => _.ErrorMessage);

                    response = new JsonResponse
                    {
                        Success = false,
                        Message = string.Join(Environment.NewLine, errors)
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditPageItem(LayoutDetailViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            JsonResponse response;

            var layout = await _pageService.GetLayoutForItemAsync(model.PageItem.Id);

            if (await HasPagePermissionsAsync(layout.PageHeaderId))
            {
                var pageItem = await _pageService.GetItemByIdAsync(model.PageItem.Id);

                if (pageItem.CarouselId.HasValue)
                {
                    ModelState.AddModelError("PageItem", "Carousels can only be edited from the carousel page");
                }
                else if (pageItem.BannerFeatureId.HasValue && model.BannerFeature == null)
                {
                    ModelState.AddModelError("PageItem", "No banner submitted");
                }
                else if (pageItem.DeckId.HasValue && model.Deck == null)
                {
                    ModelState.AddModelError("PageItem", "No deck submitted");
                }
                else if (pageItem.NavBannerId.HasValue && model.NavBanner == null)
                {
                    ModelState.AddModelError("PageItem", "No navbanner submitted");
                }
                else if (pageItem.PageFeatureId.HasValue && model.PageFeature == null)
                {
                    ModelState.AddModelError("PageItem", "No feature submitted");
                }
                else if (pageItem.SegmentId.HasValue && model.Segment == null)
                {
                    ModelState.AddModelError("PageItem", "No segment submitted");
                }
                else if (pageItem.WebslideId.HasValue && model.Webslide == null)
                {
                    ModelState.AddModelError("PageItem", "No webslide submitted");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        if (pageItem.BannerFeatureId.HasValue)
                        {
                            model.BannerFeature.Id = pageItem.BannerFeatureId.Value;
                            await _imageFeatureService.EditAsync(model.BannerFeature);
                        }
                        else if (pageItem.DeckId.HasValue)
                        {
                            model.Deck.Id = pageItem.DeckId.Value;
                            await _deckService.EditAsync(model.Deck);
                        }
                        else if (pageItem.NavBannerId.HasValue)
                        {
                            model.NavBanner.Id = pageItem.NavBannerId.Value;
                            await _navBannerService.EditAsync(model.NavBanner);
                        }
                        else if (pageItem.PageFeatureId.HasValue)
                        {
                            model.PageFeature.Id = pageItem.PageFeatureId.Value;
                            await _imageFeatureService.EditAsync(model.PageFeature);
                        }
                        else if (pageItem.SegmentId.HasValue)
                        {
                            model.Segment.IsActive = true;
                            model.Segment.EndDate = null;
                            model.Segment.StartDate = null;
                            model.Segment.Id = pageItem.SegmentId.Value;
                            await _segmentService.EditAsync(model.Segment);
                        }
                        else if (pageItem.WebslideId.HasValue)
                        {
                            model.Webslide.Id = pageItem.WebslideId.Value;
                            await _imageFeatureService.EditAsync(model.Webslide);
                        }

                        var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(LayoutDetail), new
                            {
                                id = layout.Id,
                                language = language.IsDefault ? null : language.Name
                            })
                        };

                        var pageHeader = await _pageService.GetHeaderByIdAsync(layout.PageHeaderId);

                        ShowAlertSuccess("Updated layout item");
                        _logger.LogInformation("Page {Page} layout {Name} item {Item} updated",
                            pageHeader.PageName,
                            layout.Name,
                            pageItem.Id);
                    }
                    catch (OcudaException ex)
                    {
                        response = new JsonResponse
                        {
                            Success = false,
                            Message = ex.Message
                        };
                    }
                }
                else
                {
                    var errors = ModelState.Values
                        .SelectMany(_ => _.Errors)
                        .Select(_ => _.ErrorMessage);

                    response = new JsonResponse
                    {
                        Success = false,
                        Message = string.Join(Environment.NewLine, errors)
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1, PageType? Type = null)
        {
            if (Type == null)
            {
                return RedirectToAction(nameof(Index), new { page, Type = PageType.Home });
            }

            var filter = new PageFilter(page)
            {
                PageType = Type ?? PageType.Home
            };

            var headerList = await _pageService.GetPaginatedHeaderListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = headerList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1,
                        Type
                    });
            }

            var typeNotes = new StringBuilder();

            if (Type == PageType.Sorry)
            {
                typeNotes.Append("The following are special page stubs that can be used to represent errors accessing a resource based on HTTP result codes:<ul>")
                    .Append("<li><strong>").Append(Utility.ErrorPageSlug.Error).Append("</strong> - ")
                    .Append("An error of some sort has prevented the requested page from displaying.")
                    .AppendLine("</li>")
                    .Append("<li><strong>").Append(Utility.ErrorPageSlug.NotFound).Append("</strong> - ")
                    .Append("A Not Found error (HTTP 404) occured when attempting to access the requested resource.")
                    .AppendLine("</li>").Append("</ul>");
            }

            var viewModel = new IndexViewModel
            {
                BaseLink = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl),
                CarouselTemplates = new SelectList(await _carouselService.GetAllTemplatesAsync(),
                    nameof(CarouselTemplate.Id),
                    nameof(CarouselTemplate.Name)),
                ImageFeatureTemplates = new SelectList(await _imageFeatureService
                    .GetAllTemplatesAsync(),
                        nameof(ImageFeatureTemplate.Id),
                        nameof(ImageFeatureTemplate.Name)),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                IsWebContentManager = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement),
                PageHeaders = headerList.Data,
                PageType = filter.PageType.Value,
                PaginateModel = paginateModel,
                PermissionIds = UserClaims(ClaimType.PermissionId),
                TypeNotes = typeNotes.ToString()
            };

            return View(viewModel);
        }

        [Route("[action]/{id}")]
        [HttpGet]
        [RestoreModelState]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Stubs are part of a URI and are normalized to lowercase")]
        public async Task<IActionResult> LayoutDetail(int id, string language)
        {
            var pageLayout = await _pageService.GetLayoutDetailsAsync(id);

            if (!await HasPagePermissionsAsync(pageLayout.PageHeaderId))
            {
                return RedirectToUnauthorized();
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            pageLayout.PageLayoutText = await _pageService.GetTextByLayoutAndLanguageAsync(
                pageLayout.Id, selectedLanguage.Id);

            foreach (var item in pageLayout.Items)
            {
                if (item.CarouselId.HasValue)
                {
                    item.Carousel.Name = await _carouselService.GetDefaultNameForCarouselAsync(
                        item.CarouselId.Value);
                }
            }

            var pageHeader = await _pageService.GetHeaderByIdAsync(pageLayout.PageHeaderId);

            var viewModel = new LayoutDetailViewModel
            {
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                PageLayout = pageLayout,
                PageLayoutId = pageLayout.Id,
                PreviewLink = await GetPreviewLink(pageHeader)
            };

            return View(viewModel);
        }

        [Route("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> LayoutDetail(LayoutDetailViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var pageLayout = await _pageService.GetLayoutByIdAsync(
                model.PageLayoutText.PageLayoutId);

            if (!await HasPagePermissionsAsync(pageLayout.PageHeaderId))
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                await _pageService.SetLayoutTextAsync(model.PageLayoutText);
                ShowAlertSuccess("Updated layout text.");
                var pageHeader = await _pageService.GetHeaderByIdAsync(pageLayout.PageHeaderId);
                _logger.LogInformation("Page {Page} layout {Name} text updated",
                    pageHeader.PageName,
                    pageLayout.Name);
            }

            var language = await _languageService.GetActiveByIdAsync(
                model.PageLayoutText.LanguageId);

            return RedirectToAction(nameof(LayoutDetail), new
            {
                id = pageLayout.Id,
                language = language.IsDefault ? null : language.Name
            });
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Layouts(int id, int page)
        {
            if (!await HasPagePermissionsAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var header = await _pageService.GetHeaderByIdAsync(id);

            if (!header.IsLayoutPage)
            {
                return RedirectToAction(nameof(Detail), new { id = header.Id });
            }

            page = page == default ? 1 : page;

            var filter = new BaseFilter(page);

            var layoutList = await _pageService.GetPaginatedLayoutListForHeaderAsync(id, filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = layoutList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new LayoutsViewModel
            {
                HeaderId = header.Id,
                HeaderName = header.PageName,
                HeaderStub = header.Stub,
                HeaderType = header.Type,
                PageLayouts = layoutList.Data,
                PaginateModel = paginateModel,
                PreviewLink = await GetPreviewLink(header)
            };

            return View(viewModel);
        }

        [Route("[action]/{headerId}")]
        public async Task<IActionResult> Preview(int headerId, int languageId)
        {
            try
            {
                var page = await _pageService.GetByHeaderAndLanguageAsync(headerId, languageId);
                var language = await _languageService.GetActiveByIdAsync(languageId);

                var viewModel = new PreviewViewModel
                {
                    HeaderId = headerId,
                    Language = language.Name,
                    Content = CommonMark.CommonMarkConverter.Convert(page.Content),
                    Stub = page.PageHeader.Stub
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error previewing header {Header} in language {Language}: {Message}",
                    headerId,
                    languageId,
                    ex.Message);
                ShowAlertWarning("Unable to preview page");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Route("[action]/{headerId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> RemovePermissionGroup(int headerId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupPageContent>(headerId,
                    permissionGroupId);
                AlertInfo = "Content permission removed.";
                var pageHeader = await _pageService.GetHeaderByIdAsync(headerId);
                _logger.LogInformation("Page {Page} had permission group {PermissionGroup} removed",
                    pageHeader.PageName,
                    permissionGroupId);
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem removing permission: {ex.Message}";
            }

            return RedirectToAction(nameof(ContentPermissions), new { id = headerId });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> StubInUse(PageHeader pageHeader)
        {
            var response = new JsonResponse
            {
                Success = !(await _pageService.StubInUseAsync(pageHeader))
            };

            if (!response.Success)
            {
                response.Message = "The chosen stub is already in use for that page type. Please choose a different stub.";
            }

            return Json(response);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Normalize to lowercase as part of a link")]
        private async Task<string> GetPreviewLink(PageHeader pageHeader)
        {
            var previewLink = new StringBuilder();

            var baseUrl = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

            if (pageHeader != null && !string.IsNullOrEmpty(baseUrl))
            {
                previewLink.Append(baseUrl);
                if (pageHeader.Type != PageType.Home)
                {
                    previewLink.Append(pageHeader.Type.ToString().ToLowerInvariant())
                        .Append('/')
                        .Append(pageHeader.Stub)
                        .Append('/');
                }
            }

            return previewLink.ToString();
        }

        private async Task<bool> HasPagePermissionsAsync(int pageHeaderId)
        {
            return await HasPermissionAsync<PermissionGroupPageContent>(_permissionGroupService,
                    pageHeaderId)
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement)
                || IsSiteManager();
        }
    }
}