using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Portable;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class LibraryProgramService : BaseService<LibraryProgramService>, ILibraryProgramService
    {
        private readonly IEventService _eventService;
        private readonly ILanguageService _languageService;
        private readonly ILibraryProgramRepository _libraryProgramRepository;
        private readonly ILocationService _locationService;
        private readonly ISegmentService _segmentService;
        private readonly IUserService _userService;

        public LibraryProgramService(ILogger<LibraryProgramService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEventService eventService,
            ILanguageService languageService,
            ILibraryProgramRepository libraryProgramRepository,
            ILocationService locationService,
            ISegmentService segmentService,
            IUserService userService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(eventService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(libraryProgramRepository);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _eventService = eventService;
            _languageService = languageService;
            _libraryProgramRepository = libraryProgramRepository;
            _locationService = locationService;
            _segmentService = segmentService;
            _userService = userService;
        }

        public async Task<LibraryProgram> GetAsync(int id)
        {
            var program = await _libraryProgramRepository.FindAsync(id);
            var languages = await _languageService.GetActiveAsync();
            var defaultLanguage = languages.Single(_ => _.IsDefault);
            var titleSegment = await _segmentService
                .GetBySegmentAndLanguageAsync(program.TitleSegmentId, defaultLanguage.Id);
            program.Title = titleSegment?.Text;
            var descrSegment = await _segmentService
                .GetBySegmentAndLanguageAsync(program.DescriptionSegmentId, defaultLanguage.Id);
            program.Description = descrSegment?.Text;

            return program;
        }

        public async Task<ImportResult> ImportAsync(int userId, string filename, bool performImport)
        {
            var result = new ImportResult();

            var filePath = Path.Combine(Path.GetTempPath(), filename);

            if (!System.IO.File.Exists(filePath))
            {
                throw new OcudaException($"No such file: {filePath}");
            }

            using (Serilog.Context.LogContext.PushProperty("ImportFile", filename))
            {
                await using FileStream stream = System.IO.File.OpenRead(filePath);
                var programs = await JsonSerializer
                    .DeserializeAsync<PortableList<ImportLibraryProgram>>(stream);

                if (programs.Items == null || programs.Items.Count == 0)
                {
                    throw new OcudaException("No programs found in import file.");
                }

                _logger.LogInformation("Found {ProgramsCount} programs", programs.Items.Count);

                var adminUserId = await _userService.GetSysadminIdAsync();

                var ageGroupLookup = await _eventService.GetAgeGroupLookupAsync();

                var locations = await _locationService.GetAllLocationsAsync();

                var languageList = await _languageService.GetActiveAsync();

                var tenPercent = programs.Items.Count / 10;

                var userCache = new Dictionary<string, int>();

                foreach (var importProgram in programs.Items)
                {
                    result.TotalRecords++;
                    Location location = null;
                    if (!importProgram.IsOnline)
                    {
                        location = locations
                            .SingleOrDefault(_ => _.Id == importProgram.LocationId);

                        if (location == null)
                        {
                            result.Issues.Add($"Skipped id {importProgram.Id}: references invalid location id {importProgram.LocationId}");
                            result.RecordsWithIssues++;
                            continue;
                        }
                    }

                    var program = new LibraryProgram
                    {
                        ContactEmail = importProgram.ContactEmail?.Trim(),
                        ContactName = importProgram.ContactName?.Trim(),
                        ContactPhone = importProgram.ContactPhone?.Trim(),
                        CreatedAt = importProgram.CreatedAt,
                        CreatedBy = adminUserId,
                        DurationMinutes = importProgram.DurationMinutes,
                        HistoricId = importProgram.Id,
                        ImportedBy = userId,
                        Instructor = importProgram.Instructor,
                        IsAllDay = importProgram.IsAllDay,
                        IsGuardianInfoRequired = importProgram.IsGuardianInfoRequired,
                        IsOnline = importProgram.IsOnline,
                        IsRegistrationRequired = importProgram.IsRegistrationRequired,
                        IsStaffRegistrationRequired = importProgram.IsStaffRegistrationRequired,
                        LocationId = importProgram.LocationId,
                        MaxAgeMonths = importProgram.MaxAgeMonths,
                        MaxPeople = importProgram.MaxPeople,
                        MaxWaitList = importProgram.MaxWaitList,
                        MinAgeMonths = importProgram.MinAgeMonths,
                        SignUpEnd = importProgram.SignUpEnd,
                        SignUpStart = importProgram.SignUpStart,
                        Sponsor = importProgram.Sponsor,
                        SponsorLink = importProgram.SponsorLink,
                        StartDate = importProgram.StartDate,
                        TotalAttendance = importProgram.TotalAttendance,
                        Type = importProgram.Type,
                    };

                    bool foundUser = false;
                    if (!string.IsNullOrEmpty(importProgram.InternalEmail))
                    {
                        if (userCache.TryGetValue(importProgram.InternalEmail.Trim(), out var id))
                        {
                            program.CreatedBy = id;
                            foundUser = true;
                        }
                        else
                        {
                            var userLookup = await _userService
                                .LookupUserByEmailAsync(importProgram.InternalEmail.Trim());
                            if (userLookup != null)
                            {
                                program.CreatedBy = userLookup.Id;
                                userCache.Add(importProgram.InternalEmail.Trim(), userLookup.Id);
                                foundUser = true;
                            }
                        }
                    }

                    if (!foundUser && !string.IsNullOrEmpty(importProgram.ContactEmail))
                    {
                        if (userCache.TryGetValue(importProgram.ContactEmail.Trim(), out var id))
                        {
                            program.CreatedBy = id;
                        }
                        else
                        {
                            var userLookup = await _userService
                               .LookupUserByEmailAsync(importProgram.ContactEmail.Trim());
                            if (userLookup != null)
                            {
                                program.CreatedBy = userLookup.Id;
                                userCache.Add(importProgram.ContactEmail.Trim(), userLookup.Id);
                            }
                        }
                    }

                    var language = languageList
                        .SingleOrDefault(_ => _.Name == importProgram.Text.LanguageCode)
                        ?? languageList.Single(_ => _.IsDefault);

                    // age groups
                    foreach (var ageGroup in importProgram.AgeGroups)
                    {
                        var inDatabase = ageGroupLookup.SingleOrDefault(_ => _.Name == ageGroup);
                        if (inDatabase != null)
                        {
                            program.AgeGroups.Add(inDatabase.Id);
                        }
                        else
                        {
                            var ageGroupSegment = new Segment
                            {
                                Name = $"Age group {ageGroup}",
                                IsActive = true,
                            };
                            var ageGroupSegmentText = new SegmentText
                            {
                                LanguageId = language.Id,
                                Text = ageGroup
                            };

                            if (performImport)
                            {
                                ageGroupSegment = await _segmentService
                                    .CreateAsync(ageGroupSegment);
                                ageGroupSegmentText.SegmentId = ageGroupSegment.Id;
                                await _segmentService.CreateSegmentTextAsync(ageGroupSegmentText);
                            }
                            var addedAgeGroup = new AgeGroup
                            {
                                Name = ageGroup,
                                DisplayNameId = ageGroupSegment.Id
                            };
                            if (performImport)
                            {
                                addedAgeGroup = await _eventService
                                    .AddSaveAgeGroupAsync(addedAgeGroup);
                                program.AgeGroups.Add(addedAgeGroup.Id);
                            }
                            ageGroupLookup.Add(addedAgeGroup);
                        }
                    }

                    var titleSegment = new Segment
                    {
                        Name = $"Program {importProgram.Text.Title} title",
                        IsActive = true
                    };

                    var titleSegmentText = new SegmentText
                    {
                        LanguageId = language.Id,
                        Text = importProgram.Text.Title
                    };

                    if (performImport)
                    {
                        titleSegment = await _segmentService.CreateAsync(titleSegment);
                        titleSegmentText.SegmentId = titleSegment.Id;
                        await _segmentService.CreateSegmentTextAsync(titleSegmentText);
                        program.TitleSegmentId = titleSegment.Id;
                    }

                    var descriptionSegment = new Segment
                    {
                        Name = $"Program {importProgram.Text.Title} description",
                        IsActive = true
                    };

                    var descriptionSegmentText = new SegmentText
                    {
                        LanguageId = language.Id,
                        Text = importProgram.Text.Description
                    };

                    if (performImport)
                    {
                        descriptionSegment = await _segmentService.CreateAsync(descriptionSegment);
                        descriptionSegmentText.SegmentId = descriptionSegment.Id;
                        await _segmentService.CreateSegmentTextAsync(descriptionSegmentText);
                        program.DescriptionSegmentId = descriptionSegment.Id;

                        await _libraryProgramRepository.AddAsync(program);
                        await _libraryProgramRepository.SaveAsync();
                    }

                    if (result.TotalRecords % tenPercent == 0)
                    {
                        var timePer = result.Stopwatch.ElapsedMilliseconds / result.TotalRecords;
                        var remainingRecords = programs.Items.Count - result.TotalRecords;

                        _logger.LogInformation("On {RecordNumber}/{TotalRecords} ({Percent}%) {ElapsedMs} elapsed, estimated {EstimatedRemainingMs} remaining",
                            result.TotalRecords,
                            programs.Items.Count,
                            result.TotalRecords * 100 / programs.Items.Count,
                            result.Stopwatch.ElapsedMilliseconds,
                            timePer * remainingRecords);
                    }
                }
            }

            return result;
        }

        public async Task<CollectionWithCount<LibraryProgram>> PaginateAsync(BaseFilter filter)
        {
            var languages = await _languageService.GetActiveAsync();
            var defaultLanguageId = languages.Single(_ => _.IsDefault).Id;

            // TODO this could use some caching

            var programs = await _libraryProgramRepository.PaginateAsync(filter);
            foreach (var program in programs.Data)
            {
                var segment = await _segmentService
                    .GetBySegmentAndLanguageAsync(program.TitleSegmentId, defaultLanguageId);
                if (segment != null)
                {
                    program.Title = segment.Text;
                }
                program.CreatedByUser = await _userService.GetByIdAsync(program.CreatedBy);
            }
            return programs;
        }
    }
}