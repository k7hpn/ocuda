﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Controllers.ViewModels.Locations;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private readonly IConfiguration _configuration;
        private readonly IDigitalDisplayService _digitalDisplayService;
        private readonly IFeatureService _featureService;
        private readonly IImageService _imageService;
        private readonly ILanguageService _languageService;
        private readonly ILocationFeatureService _locationFeatureService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISegmentService _segmentService;
        private readonly IVolunteerFormService _volunteerFormService;

        public LocationsController(Controller<LocationsController> context,
            IConfiguration configuration,
            IDigitalDisplayService digitalDisplayService,
            IFeatureService featureService,
            IImageService imageService,
            ILanguageService languageService,
            ILocationFeatureService locationFeatureService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            ISegmentService segmentService,
            IVolunteerFormService volunteerFormService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(digitalDisplayService);
            ArgumentNullException.ThrowIfNull(featureService);
            ArgumentNullException.ThrowIfNull(imageService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(locationFeatureService);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(volunteerFormService);

            _configuration = configuration;
            _digitalDisplayService = digitalDisplayService;
            _featureService = featureService;
            _imageService = imageService;
            _languageService = languageService;
            _locationFeatureService = locationFeatureService;
            _locationService = locationService;
            _permissionGroupService = permissionGroupService;
            _segmentService = segmentService;
            _volunteerFormService = volunteerFormService;
        }

        public static string Name
        { get { return "Locations"; } }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> AddDescription(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement)
                && await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(featureId, slug);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null) { return NotFound(); }

            if (locationFeature.SegmentId != null)
            {
                return RedirectToAction(nameof(Areas.SiteManagement.SegmentsController.Detail),
                    Areas.SiteManagement.SegmentsController.Name,
                    new
                    {
                        area = Areas.SiteManagement.SegmentsController.Area,
                        id = locationFeature.SegmentId
                    });
            }

            var segment = await _segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = $"Location {location.Name} feature {feature.Name} custom text"
            });

            if (segment == null)
            {
                _logger.LogError(
                    "Unable to create segment for {LocationName} feature {FeatureName}",
                    location.Name,
                    feature.Name);
                ShowAlertDanger("Unable to create segment. Please contact an administrator.");
                return RedirectToAction(nameof(LocationFeature), new { slug, featureId });
            }

            locationFeature.SegmentId = segment.Id;
            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(Areas.SiteManagement.SegmentsController.Detail),
                Areas.SiteManagement.SegmentsController.Name,
                new
                {
                    area = Areas.SiteManagement.SegmentsController.Area,
                    id = segment.Id
                });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddDescription(string stub)
        {
            var location = await _locationService.GetLocationByStubAsync(stub);
            if (location != null)
            {
                if (location.DescriptionSegmentId == default)
                {
                    var segment = new Segment
                    {
                        Name = $"{location.Name} description",
                    };
                    segment = await _segmentService.CreateAsync(segment);
                    location.DescriptionSegmentId = segment.Id;
                    await _locationService.EditAsync(location);
                    return RedirectToAction(nameof(SegmentsController.Detail),
                        SegmentsController.Name,
                        new { area = SegmentsController.Area, id = segment.Id });
                }
                else
                {
                    ShowAlertDanger("There is already a location description segment attached to this location.");
                }
                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> AddFeature(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature == null)
            {
                await _locationFeatureService.AddLocationFeatureAsync(new LocationFeature
                {
                    FeatureId = featureId,
                    LocationId = location.Id
                });
            }
            else
            {
                ShowAlertDanger("Feature is already configured for that location.");
            }

            return RedirectToAction(nameof(LocationFeature), new { slug, featureId });
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> AddFeature(string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var features = await _featureService.GetAllFeaturesAsync();

            var locationFeatures = await _locationFeatureService
                .GetLocationFeaturesByLocationAsync(location.Id);

            var locationHasFeatureIds = locationFeatures.Select(_ => _.FeatureId);

            var viewModel = new AddFeatureViewModel
            {
                Location = location
            };

            viewModel.AvailableFeatures.AddRange(features
                .Where(_ => !locationHasFeatureIds.Contains(_.Id))
                .ToList());

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        [RestoreModelState]
        public async Task<IActionResult> AddInteriorImage(string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var viewModel = new InteriorImageViewModel
            {
                CropHeight = _locationService.InteriorImageHeight,
                CropWidth = _locationService.InteriorImageWidth,
                LocationName = location.Name,
                Slug = location.Stub
            };

            foreach (var languageItem in await _languageService.GetActiveAsync())
            {
                viewModel.Languages.Add(languageItem.Id, languageItem.Description);
                viewModel.AltTexts.Add(languageItem.Id, string.Empty);
            }

            return View("AddInteriorImage", viewModel);
        }

        [HttpPost("[action]/{slug}")]
        [SaveModelState]
        public async Task<IActionResult> AddInteriorImage(
            InteriorImageViewModel interiorImageViewModel,
            string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            if (interiorImageViewModel == null) { return BadRequest(); }
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var languages = await _languageService.GetActiveAsync();

            foreach (var language in languages)
            {
                if (!interiorImageViewModel.AltTexts.TryGetValue(language.Id, out string value)
                    || string.IsNullOrWhiteSpace(value))
                {
                    ShowAlertDanger("You must supply Alt Text in all requested languages.");
                    return RedirectToAction(nameof(AddInteriorImage), new { slug });
                }
            }

            try
            {
                await _locationService.UploadAddInteriorImageAsync(location.Id,
                    interiorImageViewModel.Filename,
                    interiorImageViewModel.Image,
                    interiorImageViewModel.AltTexts);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"An error occurred: {oex.Message}");
                return RedirectToAction(nameof(AddInteriorImage), new { slug });
            }

            return RedirectToAction(nameof(UpdateInteriorImages), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddLocationNotice(string stub)
        {
            var location = await _locationService.GetLocationByStubAsync(stub);
            if (location != null)
            {
                if (!location.PreFeatureSegmentId.HasValue)
                {
                    var segment = new Segment
                    {
                        IsActive = false,
                        Name = $"{location.Name} location notice",
                    };
                    segment = await _segmentService.CreateAsync(segment);
                    location.PreFeatureSegmentId = segment.Id;
                    await _locationService.EditAsync(location);
                    return RedirectToAction(nameof(SegmentsController.Detail),
                        SegmentsController.Name,
                        new { area = SegmentsController.Area, id = segment.Id });
                }
                else
                {
                    ShowAlertDanger("There is already a location notice segment attached to this location.");
                }
                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> ChangeInternalImageSortOrder(int interiorImageId,
            int increment,
            string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            await _locationService.UpdateInteriorImageSortAsync(slug, interiorImageId, increment);

            return RedirectToAction(nameof(UpdateInteriorImages), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ClearLink(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null) { return NotFound(); }

            locationFeature.RedirectUrl = null;

            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ClearSegment(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement)
                && await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature?.SegmentId == null) { return NotFound(); }

            await _segmentService.DeleteAsync(locationFeature.SegmentId.Value);

            locationFeature.SegmentId = null;
            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId
            });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var defaultLanguageId = await _languageService.GetDefaultLanguageId();

            if (location.DescriptionSegmentId != default)
            {
                location.DescriptionSegment = await _segmentService
                    .GetBySegmentAndLanguageAsync(location.DescriptionSegmentId, defaultLanguageId);
            }

            if (location.PreFeatureSegmentId.HasValue)
            {
                location.PreFeatureSegmentText = await _segmentService
                    .GetBySegmentAndLanguageAsync(location.PreFeatureSegmentId.Value,
                        defaultLanguageId);
            }

            var features = await _featureService.GetAllFeaturesAsync();

            var locationFeatures = await _locationFeatureService
                .GetLocationFeaturesByLocationAsync(location.Id);

            var featuresHere = features
                .Where(_ => locationFeatures.Select(_ => _.FeatureId).Contains(_.Id));

            var languages = await _languageService.GetActiveAsync();

            location.InteriorImages = await _locationService
                .GetLocationInteriorImagesAsync(location.Id);

            var viewModel = new DetailsViewModel
            {
                AtThisLocation = featuresHere.Where(_ => _.IsAtThisLocation)
                    .OrderBy(_ => _.SortOrder)
                    .ToList(),
                Displays = await _digitalDisplayService.GetByLocationAsync(location.Id),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                Location = location,
                LocationManager = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.LocationManagement),
                SegmentEditor = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement),
                ServicesAvailable = featuresHere.Where(_ => !_.IsAtThisLocation)
                    .OrderBy(_ => _.SortOrder)
                    .ToList(),
            };

            var volunteerFeature = await _featureService
                .GetFeatureBySlugAsync("volunteer");
            if (volunteerFeature != null)
            {
                var forms = await _volunteerFormService.GetVolunteerFormsAsync();
                if (forms.Count != 0)
                {
                    var formsViewModel = new List<LocationVolunteerFormViewModel>();
                    foreach (var form in forms)
                    {
                        var mappings = await _volunteerFormService
                            .GetFormUserMappingsAsync(form.Id, location.Id);
                        var newForm = new LocationVolunteerFormViewModel
                        {
                            TypeId = (int)form.VolunteerFormType,
                            TypeName = form.VolunteerFormType.ToString(),
                            FormMappings = mappings
                                .ToList(
                                ).ConvertAll(_ => new LocationVolunteerMappingViewModel(_)),
                            IsDisabled = form.IsDisabled
                        };
                        if (form.IsDisabled)
                        {
                            newForm.AlertWarning = $"The {form.VolunteerFormType} volunteer form is not active.";
                        }
                        formsViewModel.Add(newForm);
                    }

                    var locationFeature = await _locationFeatureService
                        .GetByFeatureIdLocationIdAsync(volunteerFeature.Id, location.Id);
                    var hasForms = formsViewModel
                        .Any(_ => _.FormMappings.Count != 0 && !_.IsDisabled);
                    var hasLocationFeature = locationFeature != null;

                    if (hasForms && !hasLocationFeature)
                    {
                        await _volunteerFormService
                            .AddVolunteerLocationFeature(volunteerFeature.Id,
                                location.Id,
                                location.Stub);
                    }
                    else if (!hasForms && hasLocationFeature)
                    {
                        await _locationFeatureService
                            .DeleteAsync(volunteerFeature.Id, location.Id);
                    }
                    viewModel.VolunteerForms.AddRange(formsViewModel);
                }
            }

            foreach (var display in viewModel.Displays)
            {
                var assets = await _digitalDisplayService.GetNonExpiredAssetsAsync(display.Id);
                display.SlideCount = assets.Count();
            }

            if (location.PreFeatureSegmentId.HasValue)
            {
                viewModel.LocationNoticeSegment
                    = await _segmentService.GetByIdAsync(location.PreFeatureSegmentId.Value);
                viewModel.LocationNoticeLanguages.AddRange(await _segmentService
                    .GetSegmentLanguagesByIdAsync(location.PreFeatureSegmentId.Value));
            }

            viewModel.DescriptionLanguages.AddRange(await _segmentService
                .GetSegmentLanguagesByIdAsync(location.DescriptionSegmentId));
            viewModel.AllLanguages.AddRange(await _languageService.GetActiveNamesAsync());

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> ExteriorImage(string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (string.IsNullOrEmpty(location?.ImagePath))
            {
                return NotFound();
            }

            var fullImagePath = await _locationService
                .GetExteriorImageFilePathAsync(location.ImagePath);

            if (!System.IO.File.Exists(fullImagePath))
            {
                return NotFound();
            }

            new FileExtensionContentTypeProvider()
                .TryGetContentType(fullImagePath, out string fileType);

            return PhysicalFile(fullImagePath, fileType
                ?? System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            var filter = new LocationFilter(page == 0 ? 1 : page, 60);

            var locationList = await _locationService.GetPaginatedListAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = filter.Page,
                ItemCount = locationList.Count,
                ItemsPerPage = filter.Take.Value,
                Locations = locationList.Data
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> InteriorImage(int id)
        {
            var interiorImage = await _locationService.GetInteriorImageByIdAsync(id);

            if (string.IsNullOrEmpty(interiorImage?.ImagePath))
            {
                return NotFound();
            }

            var fullImagePath = await _locationService
                .GetInteriorImageFilePathAsync(interiorImage.ImagePath);

            if (!System.IO.File.Exists(fullImagePath))
            {
                return NotFound();
            }

            new FileExtensionContentTypeProvider()
                .TryGetContentType(fullImagePath, out string fileType);

            return PhysicalFile(fullImagePath, fileType
                ?? System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> LocationFeature(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(featureId, slug);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature == null)
            {
                return NotFound();
            }

            var viewModel = new LocationFeatureViewModel
            {
                Feature = feature,
                Location = location,
                LocationFeature = locationFeature,
                CanManageLocations = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.LocationManagement),
                CanEditSegments = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement)
            };

            viewModel.AllLanguages.AddRange(await _languageService.GetActiveNamesAsync());

            var defaultLanguageId = await _languageService.GetDefaultLanguageId();

            var nameSegment = await _segmentService
                .GetBySegmentAndLanguageAsync(feature.NameSegmentId, defaultLanguageId);
            feature.DisplayName = nameSegment.Text;
            viewModel.FeatureNameLanguages.AddRange(await _segmentService
                .GetSegmentLanguagesByIdAsync(feature.NameSegmentId));

            if (feature.TextSegmentId.HasValue)
            {
                var featureText = await _segmentService
                    .GetBySegmentAndLanguageAsync(feature.TextSegmentId.Value, defaultLanguageId);
                feature.BodyText = CommonMark.CommonMarkConverter.Convert(featureText?.Text);
                viewModel.FeatureTextLanguages.AddRange(await _segmentService
                    .GetSegmentLanguagesByIdAsync(feature.TextSegmentId.Value));
            }

            if (locationFeature.SegmentId.HasValue)
            {
                var locationFeatureText = await _segmentService
                    .GetBySegmentAndLanguageAsync(locationFeature.SegmentId.Value,
                        defaultLanguageId);
                locationFeature.Text = CommonMark
                    .CommonMarkConverter
                    .Convert(locationFeatureText?.Text);
                viewModel.LocationFeatureLanguages.AddRange(await _segmentService
                    .GetSegmentLanguagesByIdAsync(locationFeature.SegmentId.Value));
            }

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> MapImage(string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (string.IsNullOrEmpty(location?.MapImagePath))
            {
                return NotFound();
            }

            var fullImagePath = await _locationService
                .GetMapImageFilePathAsync(location.MapImagePath);

            if (!System.IO.File.Exists(fullImagePath))
            {
                return NotFound();
            }

            new FileExtensionContentTypeProvider()
                .TryGetContentType(fullImagePath, out string fileType);

            return PhysicalFile(fullImagePath, fileType
                ?? System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [HttpPost]
        [Route("{slug}/[action]")]
        public async Task<IActionResult> MapVolunteerCoordinator(string slug, int type, int userId)
        {
            var location = await _locationService.GetLocationByStubAsync(slug);
            try
            {
                await _volunteerFormService
                    .AddFormUserMapping(location.Id, (VolunteerFormType)type, userId);
                ShowAlertSuccess($"Added staff member to receive {(VolunteerFormType)type} Volunteer form submissions.");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to add staff member for for {location.Name}: {oex.Message}");
            }
            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveFeature(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null) { return NotFound(); }

            if (locationFeature.SegmentId.HasValue)
            {
                await _segmentService
                    .DeleteWithTextsAlreadyVerifiedAsync(locationFeature.SegmentId.Value);
            }

            await _locationFeatureService.DeleteAsync(featureId, location.Id);

            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost]
        [Route("{slug}/[action]")]
        public async Task<IActionResult> RemoveFormUserMapping(string slug, int userId, int type)
        {
            var location = await _locationService.GetLocationByStubAsync(slug);
            try
            {
                await _volunteerFormService
                    .RemoveFormUserMapping(location.Id, userId, (VolunteerFormType)type);
                ShowAlertSuccess($"Removed staff member from receiving {(VolunteerFormType)type} Volunteer form submissions.");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to remove staff member for {location.Name}: {oex.Message}");
            }
            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> RemoveInteriorImage(int interiorImageId, string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            await _locationService.DeleteInteriorImageAsync(interiorImageId);

            ShowAlertSuccess("Image deleted successfully");
            return RedirectToAction(nameof(UpdateInteriorImages), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveLocationNotice(int id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location != null)
            {
                if (location.PreFeatureSegmentId.HasValue)
                {
                    var segmentId = location.PreFeatureSegmentId.Value;
                    location.PreFeatureSegmentId = null;
                    await _locationService.EditAsync(location);
                    await _segmentService.DeleteAsync(segmentId);
                    ShowAlertSuccess("Location notice removed.");
                }
                else
                {
                    ShowAlertDanger("No location notice segment attached to this location.");
                }
                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> UpdateExteriorImage(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            if (string.IsNullOrEmpty(slug)) { return NotFound(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            return View("ExteriorImage", new ExteriorImageViewModel
            {
                CropHeight = _locationService.ExteriorImageHeight,
                CropWidth = _locationService.ExteriorImageWidth,
                LocationName = location.Name,
                Slug = location.Stub
            });
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> UpdateExteriorImage(ExteriorImageViewModel viewModel,
            string slug)
        {
            if (viewModel == null) { return BadRequest(); }

            if (string.IsNullOrEmpty(slug)) { return NotFound(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            if (viewModel.Image == null)
            {
                ShowAlertDanger("Please provide an exterior image");
                return RedirectToAction(nameof(ExteriorImage), new { slug });
            }

            var location = await _locationService.GetLocationByStubAsync(slug);

            try
            {
                await _locationService
                    .UpdateExteriorImageAsync(viewModel.Image, viewModel.Filename, slug);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to update exterior photo for location {location.Name}: {oex.Message}");

                return RedirectToAction(nameof(UpdateExteriorImage), new { slug });
            }

            ShowAlertSuccess($"Location {location.Name} exterior image updated successfully!");
            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> UpdateInteriorImage(InteriorImageViewModel viewModel,
            string slug)
        {
            if (viewModel == null) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            if (!viewModel.ImageId.HasValue) { return NotFound(); }

            if (viewModel.Image == null)
            {
                ShowAlertDanger("Please provide an interior image");
                return RedirectToAction(nameof(UpdateInteriorImage), new { slug });
            }

            var interiorImage = await _locationService
                .GetInteriorImageByIdAsync(viewModel.ImageId.Value);
            if (interiorImage == null) { return NotFound(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            try
            {
                await _locationService.UpdateInteriorImageAsync(interiorImage,
                    viewModel.Image.FileName,
                    viewModel.Image);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to update interior photo for location {location.Name}: {oex.Message}");

                return RedirectToAction(nameof(UpdateInteriorImage),
                    new { interiorImageId = interiorImage.Id, slug });
            }

            ShowAlertSuccess($"Location {location.Name} interior image updated successfully!");
            return RedirectToAction(nameof(UpdateInteriorImage),
                new { interiorImageId = interiorImage.Id, slug });
        }

        [HttpGet("[action]/{slug}")]
        [RestoreModelState]
        public async Task<IActionResult> UpdateInteriorImage(int interiorImageId, string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var interiorImage = await _locationService.GetInteriorImageByIdAsync(interiorImageId);
            if (interiorImage == null) { return NotFound(); }

            var viewModel = new InteriorImageViewModel
            {
                CropHeight = _locationService.InteriorImageHeight,
                CropWidth = _locationService.InteriorImageWidth,
                ImageId = interiorImage.Id,
                LocationName = location.Name,
                Slug = location.Stub
            };

            var altTexts = await _locationService
                .GetAllLanguageImageAltTextsAsync(interiorImage.Id);

            foreach (var languageItem in await _languageService.GetActiveAsync())
            {
                var altText = altTexts.SingleOrDefault(_ => _.LanguageId == languageItem.Id);
                viewModel.Languages.Add(languageItem.Id, languageItem.Description);
                viewModel.AltTexts.Add(languageItem.Id, altText.AltText);
            }

            return View("InteriorImage", viewModel);
        }

        [HttpPost("[action]/{slug}")]
        [SaveModelState]
        public async Task<IActionResult> UpdateInteriorImageData(InteriorImageViewModel viewModel,
            string slug)
        {
            if (viewModel == null) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var interiorImage = await _locationService
                .GetInteriorImageByIdAsync(viewModel.ImageId.Value);
            if (interiorImage == null) { return NotFound(); }

            var allAltTexts = await _locationService
                .GetAllLanguageImageAltTextsAsync(viewModel.ImageId.Value);

            int updates = 0;

            foreach (var altText in viewModel.AltTexts)
            {
                if (string.IsNullOrWhiteSpace(altText.Value))
                {
                    ShowAlertDanger("Unable to save empty Alt Texts.");
                    continue;
                }
                var inDatabase = allAltTexts.SingleOrDefault(_ => _.LanguageId == altText.Key);

                if (inDatabase == null)
                {
                    // add
                    await _locationService.AddImageAltTextAsync(new LocationInteriorImageAltText
                    {
                        AltText = altText.Value?.Trim(),
                        LanguageId = altText.Key,
                        LocationInteriorImageId = viewModel.ImageId.Value
                    });
                    updates++;
                }
                else if (inDatabase.AltText?.Trim() != altText.Value?.Trim())
                {
                    // changed
                    await _locationService.UpdateImageAltTextAsync(viewModel.ImageId.Value,
                        altText.Key,
                        altText.Value?.Trim());
                    updates++;
                }
            }

            if (updates == 0)
            {
                ShowAlertWarning("No updates were made.");
            }
            else
            {
                ShowAlertSuccess($"Updates made: {updates}");
            }

            return RedirectToAction(nameof(UpdateInteriorImage),
                new { interiorImageId = viewModel.ImageId, slug });
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> UpdateInteriorImages(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            if (string.IsNullOrEmpty(slug)) { return BadRequest(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var interiorImages = await _locationService.GetLocationInteriorImagesAsync(location.Id);
            foreach (var interiorImage in interiorImages)
            {
                interiorImage.AllAltTexts = await _locationService
                    .GetAllLanguageImageAltTextsAsync(interiorImage.Id);
            }

            return View("InteriorImages", new InteriorImagesViewModel
            {
                InteriorImages = interiorImages,
                LocationName = location.Name,
                Slug = location.Stub
            });
        }

        [HttpPost("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> UpdateLink(LinkViewModel viewModel)
        {
            if (viewModel == null) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(viewModel.FeatureId,
                    viewModel.LocationStub);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(feature.Id, location.Id);
            if (locationFeature == null) { return NotFound(); }

            locationFeature.RedirectUrl = viewModel.Link?.Trim();
            locationFeature.NewTab = viewModel.NewTab;

            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId = feature.Id
            });
        }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> UpdateLink(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(featureId, slug);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var viewModel = new LinkViewModel
            {
                Location = location,
                Feature = feature
            };

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature != null)
            {
                viewModel.Link = locationFeature.RedirectUrl;
                viewModel.NewTab = locationFeature.NewTab;
            }

            return View(nameof(UpdateLink), viewModel);
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> UpdateMapImage(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var viewModel = new UpdateMapImageViewModel
            {
                Location = await _locationService.GetLocationByStubAsync(slug),
                MapApiKey = _configuration[Configuration.OcudaGoogleAPI]
            };
            return View(viewModel);
        }

        [HttpPost("[action]/{slug}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Normalizing filename to lowercase")]
        public async Task<IActionResult> UpdateMapImage([FromBody] string imageBase64, string slug)
        {
            if (string.IsNullOrEmpty(slug)) { return NotFound(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            try
            {
                var (extension, imageBytes) = _imageService.ConvertFromBase64(imageBase64);
                // TODO: fix this using slugify the way the other image processes work?
                var fixedBase = location.Name
                    .ToLowerInvariant()
                    .Replace(" ", "-", StringComparison.InvariantCultureIgnoreCase)
                    .Replace(".", "", StringComparison.InvariantCultureIgnoreCase);
                var filename = $"{fixedBase}-map{extension}";

                try
                {
                    await _locationService.UpdateMapImageAsync(imageBytes, filename, slug);
                }
                catch (OcudaException oex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, oex.Message);
                }
                return new JsonResult("Image updated successfully!");
            }
            catch (ParameterException pex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, pex.Message);
            }
        }

        private async Task<(Feature, Location)> GetFeatureLocation(int featureId, string stub)
        {
            var location = await _locationService.GetLocationByStubAsync(stub);
            if (location == null)
            {
                _logger.LogError("Unable to find location with stub: {Stub}", stub);
                throw new OcudaException($"Unable to find location with stub: {stub}");
            }

            var feature = await _featureService.GetFeatureByIdAsync(featureId);
            if (feature == null)
            {
                _logger.LogError("Unable to find feature with id: {FeatureId}", featureId);
                throw new OcudaException($"Unable to find feature with id: {featureId}");
            }

            return (feature, location);
        }
    }
}