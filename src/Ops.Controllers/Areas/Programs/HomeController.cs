using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Programs.ViewModels;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;

namespace Ocuda.Ops.Controllers.Areas.Programs
{
    [Area(nameof(Programs))]
    [Route("[area]/[controller]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IEventService _eventService;
        private readonly ILibraryProgramService _libraryProgramService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(Controller<HomeController> context,
            IEventService eventService,
            ILibraryProgramService libraryProgramService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(eventService);
            ArgumentNullException.ThrowIfNull(libraryProgramService);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _eventService = eventService;
            _libraryProgramService = libraryProgramService;
            _locationService = locationService;
            _permissionGroupService = permissionGroupService;
        }

        public static string Area
        { get { return nameof(Programs); } }

        public static string Name
        { get { return "Home"; } }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = new DetailsViewModel
            {
                LibraryProgram = await _libraryProgramService.GetAsync(id),
                RegistrationChoice = "None",
            };

            if (viewModel.LibraryProgram.ScheduledEventId != null)
            {
                viewModel.ScheduledEvent = await _eventService
                    .GetEventAsync(viewModel.LibraryProgram.ScheduledEventId.Value);
            }

            if (viewModel.LibraryProgram?.IsRegistrationRequired == true)
            {
                viewModel.RegistrationChoice = viewModel
                    .LibraryProgram?
                    .IsStaffRegistrationRequired == true ? "Staff" : "Customer";
            }

            return View(viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Import()
        {
            if (!await HasImportPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            return View();
        }

        [HttpGet("")]
        [HttpGet("{page}")]
        public async Task<IActionResult> Index(int page)
        {
            page = page == 0 ? 1 : page;

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var programs = await _libraryProgramService.PaginateAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = page,
                HasImportPermission = await HasImportPermissionAsync(),
                ItemCount = programs.Count,
                ItemsPerPage = filter.Take.Value
            };

            viewModel.LocationNames
                .AddRange(await _locationService.GetAllNamesIncludingDeletedAsync());

            ((List<LibraryProgram>)viewModel.LibraryPrograms).AddRange(programs.Data);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> PublishEvent(int id)
        {
            var libraryProgram = await _libraryProgramService.GetAsync(id);

            if (libraryProgram == null)
            {
                ShowAlertDanger("Unable to find that library program.");
                return RedirectToAction(nameof(Index));
            }

            if (libraryProgram.ScheduledEventId.HasValue)
            {
                ShowAlertWarning("Program already has a scheduled event.");
                return RedirectToAction(nameof(Details), new { id });
            }

            var scheduledEvent = _libraryProgramService.CreateEventAsync(libraryProgram.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("")]
        public async Task<IActionResult> UploadPrograms(ImportViewModel viewModel)
        {
            if (!await HasImportPermissionAsync())
            {
                return RedirectToUnauthorized();
            }

            if (viewModel != null && ModelState.IsValid)
            {
                using (Serilog.Context.LogContext.PushProperty("ImportFileName", viewModel.FileName))
                {
                    _logger.LogInformation("Inserting programs from {FileName}", viewModel.FileName);
                    var tempFile = Path.GetTempFileName();
                    await using (var fileStream = new FileStream(tempFile, FileMode.Create))
                    {
                        await viewModel.Import.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var importResult = await _libraryProgramService.ImportAsync(CurrentUserId,
                            tempFile,
                            viewModel.PerformImport,
                            viewModel.CreateEvents);

                        _logger.LogInformation("File {FileName} processed {Count} items in {ElapsedMs} ms",
                            viewModel.FileName,
                            importResult.TotalRecords,
                            importResult.Stopwatch.ElapsedMilliseconds);

                        var alert = new StringBuilder("Imported ")
                            .Append(importResult.TotalRecords)
                            .Append(" items and experienced ")
                            .Append(importResult.RecordsWithIssues)
                            .Append(" records with issues in ")
                            .Append(importResult.Stopwatch.ElapsedMilliseconds)
                            .AppendLine(" ms");

                        if (importResult.Issues?.Count > 0)
                        {
                            alert.Append("<ul>");
                            foreach (var item in importResult.Issues)
                            {
                                alert.Append("<li>").Append(item).Append("</li>");
                            }
                            alert.Append("</ul>");
                            ShowAlertWarning(alert.ToString());
                        }
                        else
                        {
                            ShowAlertInfo(alert.ToString());
                        }
                    }
                    catch (OcudaException oex)
                    {
                        _logger.LogError(oex, "Error inserting imported event data: {Message}", oex.Message);
                        AlertDanger = "An error occured while importing the data.";
                    }
                    finally
                    {
                        System.IO.File.Delete(Path.Combine(Path.GetTempPath(), tempFile));
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var filenameErrors = ModelState[nameof(viewModel.FileName)]?.Errors?.ToList();
                if (filenameErrors?.Count > 0)
                {
                    foreach (var error in filenameErrors)
                    {
                        ModelState[nameof(viewModel.FileName)].Errors.Add(error);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> HasImportPermissionAsync()
        {
            // TODO fix this permission lookup
            return await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.UserSync);
        }
    }
}