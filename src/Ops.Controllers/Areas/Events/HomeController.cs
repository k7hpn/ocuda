using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Events.ViewModels;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Controllers.Areas.Events
{
    [Area(nameof(Events))]
    [Route("[area]/[controller]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IEventService _eventService;
        private readonly IPermissionGroupService _permissionGroupService;

        public HomeController(Controller<HomeController> context,
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
        { get { return "Home"; } }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
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
    }
}