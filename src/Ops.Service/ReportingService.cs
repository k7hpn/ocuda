using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class ReportingService(ILogger<ReportingService> logger,
        IHttpContextAccessor httpContextAccessor,
        IPolarisHelper polarisHelper)
        : BaseService<ReportingService>(logger, httpContextAccessor), IReportingService
    {
        public CollectionWithCount<ReportDefinition> GetList(BaseFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            return new CollectionWithCount<ReportDefinition>
            {
                Count = ReportDefinitions.Definitions.Length,
                Data = [.. ReportDefinitions.Definitions
                    .OrderBy(_ => _.Name)
                    .Skip(filter.Skip.Value)
                    .Take(filter.Take.Value)]
            };
        }

        public async Task<object> ProcessImportAsync(string reportId, DateTime dataDate, Stream import)
        {
            var time = Stopwatch.StartNew();
            var elapsed = time.ElapsedMilliseconds;
            var reports = ReportDefinitions.Definitions;
            var selectedReport = reports.SingleOrDefault(_ => _.Id == reportId)
                ?? throw new OcudaException($"Unable to import data for report id: {reportId}");

            if (selectedReport.Id == ReportDefinitionId.HooplaCheckouts)
            {
                var orgs = polarisHelper.GetOrganizations();
                var fallback = orgs.First(_ => _.ParentOrganizationID == null);
                elapsed = time.ElapsedMilliseconds;
                _logger.LogInformation("Read {RecordCount} {Type} in {ElapsedMs} ms", orgs.Count(),
                    "organizations",
                    elapsed);
                var data = ParseCsv<HooplaCardDetail>(selectedReport, import);
                _logger.LogInformation("Read {RecordCount} {Type} for {Total} circulations in {ElapsedMs} ms",
                    data.Count(),
                    "CSV records",
                    data.Select(_ => int.Parse(_.Total,
                        NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture)).Sum(_ => _),
                    time.ElapsedMilliseconds - elapsed);
                elapsed = time.ElapsedMilliseconds;
                var aggregation = await BatchToLocationsAsync(data,
                    orgs.Select(_ => _.OrganizationID),
                    fallback.OrganizationID);
                _logger.LogInformation("Read {RecordCount} {Type} for {Total} circulations in {ElapsedMs} ms",
                    aggregation.Count,
                    "organizations with data",
                    aggregation.Sum(_ => _.Value),
                    time.ElapsedMilliseconds - elapsed);
                elapsed = time.ElapsedMilliseconds;
                _logger.LogInformation("Concluded reading and mapping file in {ElapsedMs} ms",
                    elapsed);
                //TODO REPORT save reporting data now that it's been read out of the CSV and parsed
            }
            else
            {
                throw new OcudaException($"Could import data, import process undefined for id: {reportId}");
            }

            return null;
        }

        private static IEnumerable<T> ParseCsv<T>(ReportDefinition report, Stream import)
        {
            // parse csv file from import
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // TODO REPORT move the delimiter to the report definition
                Delimiter = "\t",
                HasHeaderRecord = true,
                ShouldSkipRecord = (shouldSkip) => report
                    .SkipFirstColumn
                    .Contains(shouldSkip.Row[0].Trim())
            };

            using var reader = new StreamReader(import);
            using var csv = new CsvReader(reader, csvConfig);

            return [.. csv.GetRecords<T>()];
        }

        private async Task<IDictionary<int, int>> BatchToLocationsAsync(
                    IEnumerable<HooplaCardDetail> data,
            IEnumerable<int> organizationIds,
            int fallbackOrganizationId)
        {
            const int take = 100;

            var noLocation = new List<string>();
            int processedItems = 0;
            int totalItems = data.Count();
            var skip = 0;
            var totalsByOrganizationId = organizationIds.ToDictionary(k => k, _ => 0);

            while (processedItems < totalItems)
            {
                var barcodes = data.Skip(skip).Take(take).Select(_ => _.LibraryCard);
                skip += take;

                if (!barcodes.Any())
                {
                    break;
                }

                var barcodeOrgMap = await polarisHelper.GetOrganizationIdsBatchDirect(barcodes);

                foreach (var barcode in barcodes)
                {
                    var totalString = data.Single(_ => _.LibraryCard == barcode).Total;
                    if (!int.TryParse(totalString,
                        NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture,
                        out int total))
                    {
                        _logger.LogError("Unable to parse total value {Total} into a number",
                            totalString);
                    }
                    else
                    {
                        if (barcodeOrgMap.TryGetValue(barcode, out int organizationId))
                        {
                            // we found a barcode -> organization id mapping
                            totalsByOrganizationId[organizationId] += total;
                        }
                        else
                        {
                            // look up barcode as former id
                            var organizationIdNewBarcode = await polarisHelper
                                .GetOrganizationIdFormerDirect(barcode);

                            if (organizationIdNewBarcode.HasValue)
                            {
                                totalsByOrganizationId[organizationIdNewBarcode.Value] += total;
                            }
                            else
                            {
                                // this means that the barcode has been reassigned
                                _logger.LogWarning("Could not find {Barcode} as current or former barcode",
                                    barcode);
                                noLocation.Add(barcode);
                                totalsByOrganizationId[fallbackOrganizationId] += total;
                            }
                        }
                    }
                }

                processedItems += barcodes.Count();
            }

            _logger.LogInformation("Processed {Processed}/{Total} barcodes totalling {Count} circulations.",
                processedItems,
                totalItems,
                totalsByOrganizationId.Sum(_ => _.Value));

            // TODO REPORT chagne this return to include noLocation barcodes
            return totalsByOrganizationId;
        }

        /// <summary>
        /// This class represents the fields out of the CSV file we wish to import
        /// </summary>
        internal class HooplaCardDetail
        {
            [Name("Librarycard")]
            public string LibraryCard { get; set; }

            [Name("Total")]
            public string Total { get; set; }
        }
    }
}