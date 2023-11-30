using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Events;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class EventsController : BaseController<EventsController>
    {
        private readonly IEventService _eventService;
        private readonly IPermissionGroupService _permissionGroupService;

        public EventsController(Controller<EventsController> context,
            IEventService eventService,
            IPermissionGroupService permissionGroupService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(eventService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);

            _eventService = eventService;
            _permissionGroupService = permissionGroupService;
        }

        public static string Area
        { get { return nameof(SiteManagement); } }

        public static string Name
        { get { return "Events"; } }

        [HttpGet("")]
        public async Task<IActionResult> Import()
        {
            if (!await HasPermission())
            {
                return RedirectToUnauthorized();
            }

            return View();
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
            if (!await HasPermission())
            {
                return RedirectToUnauthorized();
            }

            page = page == 0 ? 1 : page;

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            //var events = _eventService.PaginateAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = page,
                //ItemCount = formList.Count,
                //ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpPost("")]
        public async Task<IActionResult> Upload(ImportViewModel viewModel)
        {
            if (!await HasPermission())
            {
                return RedirectToUnauthorized();
            }

            if (viewModel != null && ModelState.IsValid)
            {
                using (Serilog.Context.LogContext.PushProperty("ImportFileName", viewModel.FileName))
                {
                    _logger.LogInformation("Inserting programs from {FileName}", viewModel.FileName);
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    var tempFile = Path.GetTempFileName();
                    using (var fileStream = new FileStream(tempFile, FileMode.Create))
                    {
                        await viewModel.Import.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var rosterResult
                            = await _eventService.ImportAsync(CurrentUserId, tempFile);
                        timer.Stop();
                        _logger.LogInformation("Roster {FileName} processed {Count} rows in {ElapsedMs} ms",
                            viewModel.FileName,
                            rosterResult.TotalItems,
                            timer.ElapsedMilliseconds);
                        var alert = new StringBuilder($"Imported {rosterResult.TotalItems} items in {timer.Elapsed} ms");
                        if (rosterResult.Issues?.Count > 0)
                        {
                            alert.Append("<ul>");
                            foreach (var item in rosterResult.Issues)
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

        private async Task<bool> HasPermission()
        {
            return await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EventManagement);
        }
    }
}