using System;
using System.Collections.Generic;
using System.Globalization;
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
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Models;
using Slugify;

namespace Ocuda.Ops.Service
{
    public class LibraryProgramService : BaseService<LibraryProgramService>, ILibraryProgramService
    {
        private const string ProgramAgeGroupIdAlreadyPresent = "Imported program ID {ImportProgramId} already contains Age Group ID {AgeGroupId}";
        private const string StatusFormatted = "On {RecordNumber}/{TotalRecords} ({Percent}%) {Elapsed:hh\\:mm\\:ss} elapsed, estimated: {EstimatedTotal:hh\\:mm\\:ss} total, {EstimatedRemaining:hh\\:mm\\:ss} remaining";
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEventService _eventService;
        private readonly ILanguageService _languageService;
        private readonly ILibraryProgramRepository _libraryProgramRepository;
        private readonly ILocationService _locationService;
        private readonly IScheduledEventAgeGroupRepository _scheduledEventAgeGroupRepository;
        private readonly IScheduledEventRegistrationHistoryRepository _scheduledEventRegistrationHistoryRepository;
        private readonly IScheduledEventRegistrationRepository _scheduledEventRegistrationRepository;
        private readonly ISegmentService _segmentService;
        private readonly IUserService _userService;

        public LibraryProgramService(ILogger<LibraryProgramService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IEventService eventService,
            ILanguageService languageService,
            ILibraryProgramRepository libraryProgramRepository,
            ILocationService locationService,
            IScheduledEventAgeGroupRepository scheduledEventAgeGroupRepository,
            IScheduledEventRegistrationHistoryRepository scheduledEventRegistrationHistoryRepository,
            IScheduledEventRegistrationRepository scheduledEventRegistrationRepository,
            ISegmentService segmentService,
            IUserService userService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(eventService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(libraryProgramRepository);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(scheduledEventAgeGroupRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRegistrationHistoryRepository);
            ArgumentNullException.ThrowIfNull(scheduledEventRegistrationRepository);
            ArgumentNullException.ThrowIfNull(segmentService);

            _dateTimeProvider = dateTimeProvider;
            _eventService = eventService;
            _languageService = languageService;
            _libraryProgramRepository = libraryProgramRepository;
            _locationService = locationService;
            _scheduledEventAgeGroupRepository = scheduledEventAgeGroupRepository;
            _scheduledEventRegistrationHistoryRepository = scheduledEventRegistrationHistoryRepository;
            _scheduledEventRegistrationRepository = scheduledEventRegistrationRepository;
            _segmentService = segmentService;
            _userService = userService;
        }

        public async Task<ScheduledEvent> CreateEventAsync(int libraryProgramId)
        {
            var libraryProgram = await _libraryProgramRepository.FindAsync(libraryProgramId)
                ?? throw new OcudaException($"Unable to find program id {libraryProgramId}");
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
            var titleSegment = await _segmentService
                .GetBySegmentAndLanguageAsync(libraryProgram.TitleSegmentId, defaultLanguageId);

            var slugBase = libraryProgram.StartDate.Year.ToString("0000",
                    CultureInfo.InvariantCulture)
                + "-"
                + libraryProgram.StartDate.Month.ToString("00", CultureInfo.InvariantCulture)
                + "-"
                + titleSegment.Text;

            var slugHelper = new SlugHelper();
            var slug = slugHelper.GenerateSlug(slugBase).TruncateTo(255);

            var isSlugInUse = await _eventService.IsSlugInUseAsync(slug);

            int counter = 0;
            while (isSlugInUse)
            {
                counter++;
                slug = slugHelper.GenerateSlug(slugBase.TruncateTo(252)
                    + "-"
                    + counter).TruncateTo(255);
                isSlugInUse = await _eventService.IsSlugInUseAsync(slug);
            }

            var scheduledEvent = await _eventService.CreateEventAsync(libraryProgram, slug);

            await _libraryProgramRepository
                .UpdateScheduledEventIdAsync(libraryProgramId, scheduledEvent.Id);

            return scheduledEvent;
        }

        public async Task<LibraryProgram> GetAsync(int id)
        {
            var program = await _libraryProgramRepository.FindAsync(id);
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
            var titleSegment = await _segmentService
                .GetBySegmentAndLanguageAsync(program.TitleSegmentId, defaultLanguageId);
            program.Title = titleSegment?.Text;
            var descrSegment = await _segmentService
                .GetBySegmentAndLanguageAsync(program.DescriptionSegmentId, defaultLanguageId);
            program.Description = descrSegment?.Text;

            return program;
        }

        public async Task<int?> GetIdByEventIdAsync(int scheduledEventId)
        {
            return await _libraryProgramRepository.GetIdByEventIdAsync(scheduledEventId);
        }

        public async Task<ImportResult> ImportAsync(int importUserId,
            string filename,
            bool performImport,
            bool createEvents)
        {
            var importedAt = _dateTimeProvider.Now;
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
                    var thisProgramAgeGroups = new List<int>();
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
                        ImportedBy = importUserId,
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
                        OwnedByUserId = adminUserId,
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
                            program.OwnedByUserId = id;
                            foundUser = true;
                        }
                        else
                        {
                            var userLookup = await _userService
                                .LookupUserByEmailAsync(importProgram.InternalEmail.Trim());
                            if (userLookup != null)
                            {
                                program.OwnedByUserId = userLookup.Id;
                                userCache.Add(importProgram.InternalEmail.Trim(), userLookup.Id);
                                foundUser = true;
                            }
                        }
                    }

                    if (!foundUser && !string.IsNullOrEmpty(importProgram.ContactEmail))
                    {
                        if (userCache.TryGetValue(importProgram.ContactEmail.Trim(), out var id))
                        {
                            program.OwnedByUserId = id;
                        }
                        else
                        {
                            var userLookup = await _userService
                               .LookupUserByEmailAsync(importProgram.ContactEmail.Trim());
                            if (userLookup != null)
                            {
                                program.OwnedByUserId = userLookup.Id;
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
                            if (!thisProgramAgeGroups.Contains(inDatabase.Id))
                            {
                                thisProgramAgeGroups.Add(inDatabase.Id);
                            }
                            else
                            {
                                _logger.LogWarning(ProgramAgeGroupIdAlreadyPresent,
                                    importProgram.Id,
                                    inDatabase.Id);
                            }
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
                                SegmentId = ageGroupSegment.Id
                            };
                            if (performImport)
                            {
                                addedAgeGroup = await _eventService
                                    .AddSaveAgeGroupAsync(addedAgeGroup);

                                if (!thisProgramAgeGroups.Contains(addedAgeGroup.Id))
                                {
                                    thisProgramAgeGroups.Add(addedAgeGroup.Id);
                                }
                                else
                                {
                                    _logger.LogWarning(ProgramAgeGroupIdAlreadyPresent,
                                        program.Id,
                                        addedAgeGroup.Id);
                                }
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

                        if (createEvents)
                        {
                            var scheduledEvent = await CreateEventAsync(program.Id);
                            await _libraryProgramRepository
                                .UpdateScheduledEventIdAsync(program.Id, scheduledEvent.Id);

                            foreach (var ageGroupId in thisProgramAgeGroups)
                            {
                                try
                                {
                                    await _scheduledEventAgeGroupRepository
                                        .AddAsync(scheduledEvent.Id, ageGroupId);
                                }
                                catch (OcudaException oex)
                                {
                                    _logger.LogWarning("Insert failed: {ErrorMessage}",
                                        oex.Message);
                                }
                            }
                            foreach (var registration in importProgram.Registrations)
                            {
                                var scheduledEventRegistration = new ScheduledEventRegistration
                                {
                                    Email = string.IsNullOrWhiteSpace(registration.Email)
                                        ? null
                                        : registration.Email.Trim(),
                                    FirstName = registration.FirstName?.Trim(),
                                    IsActive = true,
                                    LastName = registration.LastName?.Trim(),
                                    Phone = registration.Phone?.Trim(),
                                    RegisteredAt = registration.RegisteredAt,
                                    RegisteredByStaff = registration.StaffRegistered,
                                    ScheduledEventId = scheduledEvent.Id
                                };

                                scheduledEventRegistration
                                    = await _scheduledEventRegistrationRepository
                                        .AddSaveAsync(scheduledEventRegistration);

                                await _scheduledEventRegistrationHistoryRepository
                                    .AddImportAsync(scheduledEventRegistration.Id,
                                        registration.RegisteredAt,
                                        importedAt,
                                        registration.StaffRegistered ? adminUserId : null,
                                        importUserId);
                            }
                        }
                    }

                    if (result.TotalRecords % (tenPercent + 1) == 0
                        || result.TotalRecords == programs.Items.Count)
                    {
                        var timePer = result.Stopwatch.Elapsed.TotalSeconds / result.TotalRecords;
                        var remainingRecords = programs.Items.Count - result.TotalRecords;

                        _logger.LogInformation(StatusFormatted,
                            result.TotalRecords,
                            programs.Items.Count,
                            result.TotalRecords * 100 / programs.Items.Count,
                            result.Stopwatch.Elapsed,
                            TimeSpan.FromSeconds(timePer * programs.Items.Count),
                            TimeSpan.FromSeconds(timePer * remainingRecords));
                    }
                }
            }

            return result;
        }

        public async Task<CollectionWithCount<LibraryProgram>> PaginateAsync(BaseFilter filter)
        {
            var languages = await _languageService.GetActiveAsync();
            var defaultLanguageId = languages.Single(_ => _.IsDefault).Id;

            var programs = await _libraryProgramRepository.PaginateAsync(filter);

            var registrationCounts = await _eventService.GetRegistrationCountAsync(programs.Data
                .Where(_ => _.ScheduledEventId.HasValue && _.MaxPeople > 0)
                .Select(_ => _.ScheduledEventId.Value));

            foreach (var program in programs.Data)
            {
                var segment = await _segmentService
                    .GetBySegmentAndLanguageAsync(program.TitleSegmentId, defaultLanguageId);

                if (segment != null)
                {
                    program.Title = segment.Text;
                }

                program.OwnedByUser = await _userService.GetByIdAsync(program.OwnedByUserId);

                if (program.ScheduledEventId.HasValue
                    && registrationCounts.TryGetValue(program.ScheduledEventId.Value, out int value))
                {
                    program.RegistrationCount = value;
                }
            }
            return programs;
        }
    }
}