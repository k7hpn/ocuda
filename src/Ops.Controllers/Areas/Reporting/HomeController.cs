using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Reporting.ViewModel;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Serilog.Context;

namespace Ocuda.Ops.Controllers.Areas.Reporting
{
    [Area(nameof(Reporting))]
    [Route("[area]")]
    public class HomeController(Controller<HomeController> context,
        IPermissionGroupService permissionGroupService,
        IReportingService reportingService)
        : BaseController<HomeController>(context)
    {
        public static string Area
        { get { return nameof(Reporting); } }

        public static string Name
        { get { return "Home"; } }

        [HttpPost("[action]/{reportId}")]
        public async Task<IActionResult> Import(string reportId, ImportViewModel viewModel)
        {
            if (viewModel == null)
            {
                ShowAlertDanger("Unable to accept uploaded data file.");
                return RedirectToAction(nameof(Import));
            }

            var report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);
            if (report == null)
            {
                ShowAlertWarning($"Unable to find report with id: {reportId}");
                return RedirectToAction(nameof(Index));
            }

            if (!await HasImportPermissionAsync(report.Id))
            {
                return RedirectToUnauthorized();
            }

            // TODO REPORT: see if data exists for the selected date, if so prompt or overwrite?

            await using var stream = viewModel.DataFile.OpenReadStream();

            using (LogContext.PushProperty("ImportFilename", viewModel.DataFile.FileName))
            {
                var importResult = await reportingService.ProcessImportAsync(report.Id,
                    viewModel.DataDate,
                    viewModel.DataFile.FileName,
                    stream);
                return RedirectToAction(nameof(Results), new { id = importResult });
            }
        }

        [HttpGet("[action]/{reportId}")]
        public async Task<IActionResult> Import(string reportId)
        {
            var viewModel = new ImportViewModel
            {
                DataDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1),
                Report = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId)
            };

            if (viewModel.Report == null)
            {
                ShowAlertWarning($"Unable to find report with id: {reportId}");
                return RedirectToAction(nameof(Index));
            }

            if (!await HasImportPermissionAsync(viewModel.Report.Id))
            {
                return RedirectToUnauthorized();
            }

            viewModel.Heading = $"Import data for {viewModel.Report.Name}";
            SetPageTitle(viewModel.Heading);
            return View(viewModel);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var collectionWithCount = reportingService.GetList(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = page,
                Heading = "Reporting",
                IsIndex = true,
                ItemCount = collectionWithCount.Count,
                ItemsPerPage = filter.Take.Value,
                Reports = collectionWithCount.Data,
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            // loop through reports to see if the current user can import for them
            foreach (var report in viewModel.Reports)
            {
                report.Importable = await HasImportPermissionAsync(report.Id);
            }

            SetPageTitle(viewModel.Heading);
            return View(viewModel);
        }

        [Route("[action]/{reportId}/{year}/{month}")]
        public async Task<IActionResult> Results(string reportId, int year, int month)
        {
            var reportType = ReportDefinitions.Definitions.SingleOrDefault(_ => _.Id == reportId);

            if (reportType == null) { return StatusCode(404); }

            var results = await reportingService.GetResultsAsync(reportType.Id, year, month);

            if (results == null) { return StatusCode(404); }

            var viewModel = new ResultsViewModel
            {
                Results = results,
                Heading = reportType.Name,
                SecondaryHeading = $"{results.Month}/{results.Year}"
            };

            SetPageTitle(viewModel.Heading);
            return View(viewModel);
        }

        private async Task<bool> HasImportPermissionAsync(string reportId)
        {
            // TODO REPORT add permissions based on report the way we do with pages
            // TODO REPORT add permission check for if user can view report(s)
            return await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.ImportAllReports)
                || IsSiteManager();
        }
    }
}