using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Events.ViewModels;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;

namespace Ocuda.Ops.Controllers.Areas.Events
{
    [Area(nameof(Events))]
    [Route("[area]/[controller]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEventService _eventService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ILibraryProgramService _programService;
        private readonly IUserService _userService;

        public HomeController(Controller<HomeController> context,
            IDateTimeProvider dateTimeProvider,
            IEventService eventService,
            ILibraryProgramService programService,
            IPermissionGroupService permissionGroupService,
            IUserService userService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(eventService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(programService);
            ArgumentNullException.ThrowIfNull(userService);

            _dateTimeProvider = dateTimeProvider;
            _eventService = eventService;
            _permissionGroupService = permissionGroupService;
            _programService = programService;
            _userService = userService;
        }

        public static string Area
        { get { return nameof(Events); } }

        public static string Name
        { get { return "Home"; } }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = new DetailsViewModel
            {
                Now = _dateTimeProvider.Now,
                ScheduledEvent = await _eventService.GetEventAsync(id),
                ScheduledEventRegistrations = await _eventService.GetEventRegistrationsAsync(id)
            };

            return View(viewModel);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            page = page == 0 ? 1 : page;

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var events = await _eventService.PaginateAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = page,
                ItemCount = events.Count,
                ItemsPerPage = filter.Take.Value
            };

            ((List<ScheduledEvent>)viewModel.ScheduledEvents).AddRange(events.Data);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Registration(Guid id)
        {
            return View(new RegistrationViewModel
            {
                ScheduledEventRegistration = await _eventService.GetEventRegistrationAsync(id),
                ScheduledEventRegistrationHistories = await _eventService
                    .GetRegistrationHistoryAsync(id)
            });
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> ViewParent(int id)
        {
            var scheduledEvent = await _eventService.GetEventAsync(id);
            if (scheduledEvent == null)
            {
                return StatusCode(404);
            }

            switch (scheduledEvent.ScheduledEventType)
            {
                case Utility.Models.ScheduledEventType.Program:
                    var programId = await _programService.GetIdByEventIdAsync(scheduledEvent.Id);
                    if (!programId.HasValue)
                    {
                        return StatusCode(404);
                    }
                    return RedirectToAction(nameof(Programs.HomeController.Details),
                        Programs.HomeController.Name,
                        new
                        {
                            Programs.HomeController.Area,
                            Id = programId
                        });

                default:
                    return StatusCode(501);
            }
        }
    }
}