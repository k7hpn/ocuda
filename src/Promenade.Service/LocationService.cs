﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchLocator.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class LocationService : BaseService<LocationService>
    {
        private const int CacheLocationFeatureHours = 1;
        private const int CacheLocationGroupHours = 1;
        private const int CacheLocationHours = 1;
        private const int CacheLocationIds = 1;
        private const int CacheLocationScheduleHours = 1;
        private const int CacheLocationStatus = 4;
        private const int CacheSlugIdMapHours = 12;
        private const int DaysInWeek = 7;
        private const char ndash = '\u2013';

        private readonly IOcudaCache _cache;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IFeatureRepository _featureRepository;
        private readonly IGoogleClient _googleClient;
        private readonly IGroupRepository _groupRepository;
        private readonly IImageAltTextRepository _imageAltTextRepository;
        private readonly LanguageService _languageService;
        private readonly IStringLocalizer<i18n.Resources.Shared> _localizer;
        private readonly ILocationFeatureRepository _locationFeatureRepository;
        private readonly ILocationGroupRepository _locationGroupRepository;
        private readonly ILocationHoursOverrideRepository _locationHoursOverrideRepository;
        private readonly ILocationHoursRepository _locationHoursRepository;
        private readonly ILocationInteriorImageRepository _locationInteriorImageRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly SegmentService _segmentService;

        public LocationService(IDateTimeProvider dateTimeProvider,
            IFeatureRepository featureRepository,
            IGoogleClient googleClient,
            IGroupRepository groupRepository,
            IHttpContextAccessor contextAccessor,
            IImageAltTextRepository imageAltTextRepository,
            ILocationFeatureRepository locationFeatureRepository,
            ILocationGroupRepository locationGroupRepository,
            ILocationHoursOverrideRepository locationHoursOverrideRepository,
            ILocationHoursRepository locationHoursRepository,
            ILocationInteriorImageRepository locationInteriorImageRepository,
            ILocationRepository locationRepository,
            ILogger<LocationService> logger,
            IOcudaCache cache,
            IStringLocalizer<i18n.Resources.Shared> localizer,
            LanguageService languageService,
            SegmentService segmentService)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(contextAccessor);
            ArgumentNullException.ThrowIfNull(featureRepository);
            ArgumentNullException.ThrowIfNull(googleClient);
            ArgumentNullException.ThrowIfNull(groupRepository);
            ArgumentNullException.ThrowIfNull(imageAltTextRepository);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(localizer);
            ArgumentNullException.ThrowIfNull(locationFeatureRepository);
            ArgumentNullException.ThrowIfNull(locationGroupRepository);
            ArgumentNullException.ThrowIfNull(locationHoursOverrideRepository);
            ArgumentNullException.ThrowIfNull(locationHoursRepository);
            ArgumentNullException.ThrowIfNull(locationInteriorImageRepository);
            ArgumentNullException.ThrowIfNull(locationRepository);
            ArgumentNullException.ThrowIfNull(segmentService);

            _cache = cache;
            _contextAccessor = contextAccessor;
            _featureRepository = featureRepository;
            _googleClient = googleClient;
            _groupRepository = groupRepository;
            _imageAltTextRepository = imageAltTextRepository;
            _languageService = languageService;
            _localizer = localizer;
            _locationFeatureRepository = locationFeatureRepository;
            _locationGroupRepository = locationGroupRepository;
            _locationHoursOverrideRepository = locationHoursOverrideRepository;
            _locationHoursRepository = locationHoursRepository;
            _locationInteriorImageRepository = locationInteriorImageRepository;
            _locationRepository = locationRepository;
            _segmentService = segmentService;
        }

        public async Task<(double? Latitude, double? Longitude)> GeocodeAddressAsync(string address)
        {
            return await _googleClient.GeocodeAsync(address);
        }

        public async Task<ICollection<Location>> GetAllLocationsAsync(bool forceReload)
        {
            string cacheKey = Utility.Keys.Cache.PromLocationIds;

            ICollection<int> allLocationIds = null;

            if (!forceReload)
            {
                allLocationIds = await _cache.GetObjectFromCacheAsync<ICollection<int>>(cacheKey);
            }

            if (allLocationIds == null)
            {
                allLocationIds = await _locationRepository.GetAllLocationIdsAsync();
                if (allLocationIds?.Count > 0)
                {
                    await _cache.SaveToCacheAsync(cacheKey, allLocationIds, CacheLocationIds);
                }
            }

            var locations = new List<Location>();

            if (allLocationIds != null)
            {
                foreach (var locationId in allLocationIds)
                {
                    locations.Add(await GetLocationAsync(locationId, forceReload));
                }
            }

            return locations;
        }

        public async Task<string> GetClosureInformationAsync(DateTime date)
        {
            return await _locationHoursOverrideRepository.GetClosureInformationAsync(date);
        }

        public async Task<LocationHoursResult> GetCurrentStatusAsync(Location location,
            bool forceReload)
        {
            ArgumentNullException.ThrowIfNull(location);

            var currentDefaultLanguageId = await GetCurrentDefaultLanguageIdAsync(_contextAccessor,
                _languageService);

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLocationStatus,
                location.Id,
                currentDefaultLanguageId.First());

            LocationHoursResult result = null;

            if (!forceReload)
            {
                result = await _cache.GetObjectFromCacheAsync<LocationHoursResult>(cacheKey);
                if (result != null)
                {
                    return result;
                }
            }

            if (location.IsAlwaysOpen)
            {
                result = new LocationHoursResult
                {
                    IsCurrentlyOpen = true,
                    Open = true,
                    StatusMessage = i18n.Keys.Promenade.LocationOpen,
                    TodaysHours = i18n.Keys.Promenade.LocationOpen
                };
            }

            if (location.HoursSegmentId != null)
            {
                result = new LocationHoursResult
                {
                    IsSpecialHours = true,
                    StatusMessage = i18n.Keys.Promenade.SpecialHours,
                    TodaysHours = i18n.Keys.Promenade.SpecialHours
                };
            }

            if (location.IsClosed)
            {
                result = new LocationHoursResult
                {
                    IsCurrentlyOpen = false,
                    Open = false,
                    StatusMessage = i18n.Keys.Promenade.LocationClosed,
                    TodaysHours = i18n.Keys.Promenade.LocationClosed
                };
            }

            if (result == null)
            {
                var now = _dateTimeProvider.Now;

                result = new LocationHoursResult();

                var todayOverride = await _locationHoursOverrideRepository
                    .GetByDateAsync(location.Id, now);

                if (todayOverride != null)
                {
                    result.Open = todayOverride.Open;
                    result.OpenTime = todayOverride.OpenTime;
                    result.CloseTime = todayOverride.CloseTime;
                }
                else
                {
                    var todayHours = await _locationHoursRepository.GetByDayOfWeek(location.Id, now);

                    result.Open = todayHours.Open;
                    result.OpenTime = todayHours.OpenTime;
                    result.CloseTime = todayHours.CloseTime;
                }

                var showNextOpen = false;

                if (result.Open)
                {
                    if (!result.OpenTime.HasValue || !result.CloseTime.HasValue)
                    {
                        result.StatusMessage = _localizer[i18n.Keys.Promenade.LocationOpen];
                        result.IsCurrentlyOpen = true;
                    }
                    else if (result.OpenTime.Value.TimeOfDay > now.TimeOfDay)
                    {
                        var opensAt = result.OpenTime.Value.ToString("t", CultureInfo.CurrentCulture);
                        result.StatusMessage = _localizer[i18n.Keys.Promenade.LocationOpensAtItem,
                            opensAt];
                        result.NextStatusChange = new DateTime(_dateTimeProvider.Now.Date.Ticks
                            + result.OpenTime.Value.TimeOfDay.Ticks);
                    }
                    else if (result.CloseTime.Value.TimeOfDay > now.TimeOfDay)
                    {
                        var openUntil = result.CloseTime.Value.ToString("t",
                            CultureInfo.CurrentCulture);
                        result.StatusMessage = _localizer[i18n.Keys.Promenade.LocationOpenUntilItem,
                            openUntil];
                        result.IsCurrentlyOpen = true;
                        result.NextStatusChange = new DateTime(_dateTimeProvider.Now.Date.Ticks
                            + result.CloseTime.Value.TimeOfDay.Ticks);
                    }
                    else
                    {
                        result.StatusMessage = _localizer[i18n.Keys.Promenade.LocationClosed];
                        showNextOpen = true;
                    }
                }
                else
                {
                    result.StatusMessage = _localizer[i18n.Keys.Promenade.LocationClosedToday];
                    showNextOpen = true;
                }

                if (showNextOpen)
                {
                    // get future overrides less than a week away for the location
                    var futureOverrides = (await _locationHoursOverrideRepository
                        .GetBetweenDatesAsync(location.Id, now.AddDays(1), now.AddDays(DaysInWeek - 1)))
                        .Select(_ => new LocationHours
                        {
                            Open = _.Open,
                            OpenTime = _.OpenTime,
                            DayOfWeek = _.Date.DayOfWeek
                        });

                    // get branch hours for the week excluding the current day and days that have overrides
                    var weeklyBranchHours = await _locationHoursRepository
                        .GetWeeklyHoursAsync(location.Id);
                    weeklyBranchHours = weeklyBranchHours
                        .Where(_ => _.DayOfWeek != now.DayOfWeek
                            && !futureOverrides.Select(d => d.DayOfWeek).Contains(_.DayOfWeek))
                        .ToList();

                    // combine the lists and order by the next upcoming day to get the next branch opening
                    var nextOpen = futureOverrides
                        .Concat(weeklyBranchHours)
                        .Where(_ => _.Open)
                        .OrderBy(_ => (_.DayOfWeek - now.DayOfWeek + DaysInWeek) % DaysInWeek)
                        .FirstOrDefault();

                    if (nextOpen != null)
                    {
                        var nextDay = "";
                        if ((int)nextOpen.DayOfWeek == ((int)now.DayOfWeek + 1) % DaysInWeek)
                        {
                            nextDay = _localizer[i18n.Keys.Promenade.LocationTomorrow];
                        }
                        else
                        {
                            nextDay = CultureInfo
                                .CurrentCulture
                                .DateTimeFormat
                                .GetAbbreviatedDayName(nextOpen.DayOfWeek);
                        }

                        var opensAt = nextOpen.OpenTime.Value.ToString("t",
                            CultureInfo.CurrentCulture);

                        result.StatusMessage = _localizer[i18n.Keys.Promenade.LocationOpensNextItem,
                            nextDay,
                            opensAt];

                        var nextOpenDayOfWeekDelta = ((int)nextOpen.DayOfWeek
                            - (int)_dateTimeProvider.Now.DayOfWeek + DaysInWeek) % DaysInWeek;

                        var nextOpenDay = _dateTimeProvider.Now.Date.AddDays(nextOpenDayOfWeekDelta);

                        result.NextStatusChange = nextOpenDay.Date + nextOpen.OpenTime.Value.TimeOfDay;
                        result.NextOpenDateTime = result.NextStatusChange;
                    }
                }
            }

            if (result.OpenTime.HasValue && result.CloseTime.HasValue)
            {
                result.TodaysHours = FormatOpeningHours(result.OpenTime.Value,
                    result.CloseTime.Value);
            }

            bool cached = false;
            if (result.NextStatusChange.HasValue)
            {
                var diffHours = result.NextStatusChange.Value - _dateTimeProvider.Now;
                if (diffHours.TotalHours < CacheLocationStatus)
                {
                    // change occurs prior to the default timeout, cache until the change
                    await _cache.SaveToCacheAsync(cacheKey, result, diffHours);
                    cached = true;
                }
            }

            if (!cached)
            {
                await _cache.SaveToCacheAsync(cacheKey, result, CacheLocationStatus);
            }

            return result;
        }

        public async Task<ICollection<LocationFeature>> GetFullLocationFeaturesAsync(int locationId,
            int? featureId,
            bool forceReload)
        {
            ICollection<LocationFeature> features = await GetFromCacheDatabaseAsync(
                Utility.Keys.Cache.PromLocationFeatures,
                locationId,
                CacheLocationFeatureHours,
                _cache,
                forceReload,
                _locationFeatureRepository.GetFullLocationFeaturesAsync);

            if (featureId.HasValue)
            {
                features = features.Where(_ => _.FeatureId == featureId.Value).ToList();
            }

            if (features != null)
            {
                foreach (var feature in features)
                {
                    // get feature name
                    var nameSegmentText = await _segmentService
                        .GetSegmentTextBySegmentIdAsync(feature.Feature.NameSegmentId, forceReload);
                    if (nameSegmentText == null)
                    {
                        _logger.LogError("Unable to find name segment id {NameSegmentId} for feature id {FeatureId} at location {LocationId}",
                            feature.Feature.NameSegmentId,
                            feature.FeatureId,
                            locationId);
                    }
                    feature.Feature.DisplayName = nameSegmentText?.Text;

                    // feature.SegmentId is for this location, feature.Feature.SegmentId is global
                    int? textSegmentId = feature.SegmentId ?? feature.Feature.TextSegmentId;
                    if (textSegmentId.HasValue)
                    {
                        var segmentText = await _segmentService
                            .GetSegmentTextBySegmentIdAsync(textSegmentId.Value, forceReload);
                        if (segmentText == null)
                        {
                            _logger.LogInformation("Unable to find text segment id {TextSegmentId} for feature id {FeatureId} at location {LocationId}",
                                textSegmentId.Value,
                                feature.FeatureId,
                                locationId);
                        }
                        feature.Text = segmentText?.Text;
                    }
                }
            }

            return features;
        }

        public async Task<List<LocationDayGrouping>> GetHoursAsync(int locationId,
            bool forceReload)
        {
            var location = await GetLocationAsync(locationId, forceReload);
            if (location?.IsAlwaysOpen != false || location.IsClosed)
            {
                return null;
            }
            var weeklyHours = await GetScheduleAsync(locationId, forceReload);
            return ComputeWeeklyHours(weeklyHours);
        }

        public async Task<Location> GetLocationAsync(int id, bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLocation,
                id);

            Location location = await GetFromCacheDatabaseAsync(
                Utility.Keys.Cache.PromLocation,
                id,
                CacheLocationHours,
                _cache,
                forceReload,
                _locationRepository.FindAsync);

            if (location == null)
            {
                return null;
            }

            location.InteriorImages = await GetFromCacheDatabaseAsync(
                Utility.Keys.Cache.PromLocationInteriorImages,
                location.Id,
                CacheLocationHours,
                _cache,
                forceReload,
                _locationInteriorImageRepository.GetLocationInteriorImagesAsync);

            var fixedSortOrder = 0;

            if (location.InteriorImages?.Count > 0)
            {
                var currentDefaultLanguageId = await GetCurrentDefaultLanguageIdAsync(_contextAccessor,
                    _languageService);

                foreach (var interiorImage in location.InteriorImages.OrderBy(_ => _.SortOrder))
                {
                    interiorImage.SortOrder = fixedSortOrder++;

                    var altText = await GetFromCacheDatabaseAsync(
                        Utility.Keys.Cache.PromLocationImageAltText,
                        interiorImage.Id,
                        currentDefaultLanguageId,
                        CacheLocationHours,
                        _cache,
                        forceReload,
                        _imageAltTextRepository.GetByImageIdAsync);

                    interiorImage.AltText = altText?.AltText;
                }
            }

            location.DescriptionSegment = await _segmentService
                .GetSegmentTextBySegmentIdAsync(location.DescriptionSegmentId,
                forceReload);

            if (location.HoursSegmentId.HasValue)
            {
                location.HoursSegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(location.HoursSegmentId.Value, forceReload);
            }

            if (location.PreFeatureSegmentId.HasValue)
            {
                location.PreFeatureSegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(location.PreFeatureSegmentId.Value,
                        forceReload);
            }

            if (location.PostFeatureSegmentId.HasValue)
            {
                location.PostFeatureSegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(location.PostFeatureSegmentId.Value,
                        forceReload);
            }

            location.CurrentStatus = await GetCurrentStatusAsync(location, forceReload);

            return location;
        }

        public async Task<LocationFeature> GetLocationFullFeatureAsync(int locationId,
            string featureSlug,
            bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromFeatureSlug,
                featureSlug);

            int? featureId = null;

            if (!forceReload)
            {
                featureId = await _cache.GetIntFromCacheAsync(cacheKey);
            }

            if (!featureId.HasValue)
            {
                featureId = await _featureRepository.GetIdBySlugAsync(featureSlug);

                if (!featureId.HasValue)
                {
                    return null;
                }

                await _cache.SaveToCacheAsync(cacheKey, featureId.Value, CacheLocationFeatureHours);
            }

            var features = await GetFullLocationFeaturesAsync(locationId, featureId, forceReload);

            return features.SingleOrDefault(_ => _.FeatureId == featureId.Value
                && _.LocationId == locationId);
        }

        public async Task<int?> GetLocationIdAsync(string slug, bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLocationSlugToId,
                slug);

            int? id = null;

            if (!forceReload)
            {
                id = await _cache.GetIntFromCacheAsync(cacheKey);
            }

            if (id != null)
            {
                return id;
            }

            id = await _locationRepository.GetIdBySlugAsync(slug);

            if (id != null)
            {
                await _cache.SaveToCacheAsync(cacheKey, id, CacheSlugIdMapHours);
            }

            return id;
        }

        public async Task<Group> GetLocationsNeighborGroup(int groupId, bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLocationNeighborGroup,
                groupId);

            Group group = null;

            if (!forceReload)
            {
                group = await _cache.GetObjectFromCacheAsync<Group>(cacheKey);
            }

            if (group != null)
            {
                return group;
            }

            group = await _groupRepository.FindAsync(groupId);

            if (group != null)
            {
                await _cache.SaveToCacheAsync(cacheKey, group, CacheLocationGroupHours);
            }

            return group;
        }

        public async Task<ICollection<LocationGroup>> GetLocationsNeighborsAsync(int groupId,
            bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLocationGroup,
                groupId);

            ICollection<LocationGroup> locationGroups = null;

            if (!forceReload)
            {
                locationGroups = await _cache.GetObjectFromCacheAsync<ICollection<LocationGroup>>(cacheKey);
            }

            if (locationGroups == null)
            {
                locationGroups = await _locationGroupRepository.GetLocationsByGroupIdAsync(groupId);

                if (locationGroups != null)
                {
                    await _cache.SaveToCacheAsync(cacheKey, locationGroups, CacheLocationGroupHours);
                }
                else
                {
                    locationGroups = new List<LocationGroup>();
                }
            }

            foreach (var locationGroup in locationGroups.Where(_ => _.HasSubscription))
            {
                locationGroup.Location
                    = await GetLocationAsync(locationGroup.LocationId, forceReload);
            }

            return locationGroups;
        }

        public async Task<IList<Location>> GetLocationsStatusAsync(double? latitude,
            double? longitude,
            bool forceReload)
        {
            var locations = await GetAllLocationsAsync(forceReload);

            foreach (var location in locations)
            {
                location.CurrentStatus = await GetCurrentStatusAsync(location, forceReload);
                if (latitude.HasValue
                    && longitude.HasValue
                    && !string.IsNullOrWhiteSpace(location.GeoLocation)
                    && location.GeoLocation.Contains(',', StringComparison.OrdinalIgnoreCase))
                {
                    var geolocation = location.GeoLocation.Split(',');
                    if (geolocation.Length == 2
                        && double.TryParse(geolocation[0], out var locationLatitude)
                        && double.TryParse(geolocation[1], out var locationLongitude))
                    {
                        location.Distance = Math.Ceiling(HaversineHelper.Calculate(
                                locationLatitude,
                                locationLongitude,
                                latitude.Value,
                                longitude.Value));
                    }
                }
            }

            return latitude.HasValue && longitude.HasValue
                ? locations.OrderBy(_ => _.Distance).ThenBy(_ => _.Name).ToList()
                : locations.OrderBy(_ => _.Name).ToList();
        }

        //public async Task<ICollection<LocationHoursResult>> GetWeeklyHoursAsync(int locationId,
        //    bool forceReload)
        //{
        //    var results = new List<LocationHoursResult>();

        //    var location = await GetLocationAsync(locationId, forceReload);

        //    if (location.IsAlwaysOpen)
        //    {
        //        for (int day = 0; day < DaysInWeek; day++)
        //        {
        //            results.Add(new LocationHoursResult
        //            {
        //                Open = true,
        //                DayOfWeek = (DayOfWeek)day,
        //                IsCurrentlyOpen = true
        //            });
        //        }
        //    }
        //    else if (location.IsClosed)
        //    {
        //        for (int day = 0; day < DaysInWeek; day++)
        //        {
        //            results.Add(new LocationHoursResult
        //            {
        //                Open = false,
        //                DayOfWeek = (DayOfWeek)day,
        //                IsCurrentlyOpen = false
        //            });
        //        }
        //    }
        //    else
        //    {
        //        // Add override days
        //        var now = DateTime.Now;
        //        var firstDayOfWeek = now.AddDays(-(int)now.DayOfWeek);
        //        var lastDayOfWeek = firstDayOfWeek.AddDays(DaysInWeek - 1);

        //        var overrides = await _locationHoursOverrideRepository.GetBetweenDatesAsync(
        //            locationId, firstDayOfWeek, lastDayOfWeek);

        //        foreach (var dayOverride in overrides)
        //        {
        //            results.Add(new LocationHoursResult
        //            {
        //                OpenTime = dayOverride.OpenTime,
        //                CloseTime = dayOverride.CloseTime,
        //                Open = dayOverride.Open,
        //                DayOfWeek = dayOverride.Date.DayOfWeek,
        //                IsOverride = true
        //            });
        //        }

        //        // Fill in non-override days
        //        if (results.Count < DaysInWeek)
        //        {
        //            var weeklyHours
        //                = await _locationHoursRepository.GetWeeklyHoursAsync(locationId);

        //            var remainingDays = weeklyHours
        //                .Where(_ => !results.Select(r => r.DayOfWeek).Contains(_.DayOfWeek));

        //            foreach (var day in remainingDays)
        //            {
        //                results.Add(new LocationHoursResult
        //                {
        //                    OpenTime = day.OpenTime,
        //                    CloseTime = day.CloseTime,
        //                    Open = day.Open,
        //                    DayOfWeek = day.DayOfWeek
        //                });
        //            }
        //        }

        //        // Set currently open
        //        foreach (var dayResult in results)
        //        {
        //            if (dayResult.Open && dayResult.OpenTime <= now && dayResult.CloseTime >= now)
        //            {
        //                dayResult.IsCurrentlyOpen = true;
        //            }
        //        }

        //        results = results.OrderBy(_ => _.DayOfWeek).ToList();
        //    }

        //    return results;
        //}

        public async Task<string> GetZipCodeAsync(double latitude, double longitude)
        {
            return await _googleClient.GetZipCodeAsync(latitude, longitude);
        }

        private static string FormatOpeningHours(DateTime openTime, DateTime closeTime)
        {
            var format = new StringBuilder("{0:%h");
            if (openTime.Minute != 0)
            {
                format.Append(":mm");
            }
            format.Append(" tt} ").Append(ndash).Append(" {1:%h");
            if (closeTime.Minute != 0)
            {
                format.Append(":mm");
            }
            format.Append(" tt}");
            return string.Format(CultureInfo.CurrentCulture,
                format.ToString(),
                openTime,
                closeTime);
        }

        private static string GetFormattedDayGroupings(List<DayOfWeek> days)
        {
            var dayFormatter = CultureInfo.CurrentCulture.DateTimeFormat;

            if (days.Count == 1)
            {
                return dayFormatter.GetAbbreviatedDayName(days[0]);
            }
            else
            {
                var firstDay = days[0];
                var lastDay = days.Last();

                if (days.Count == 2)
                {
                    return dayFormatter.GetAbbreviatedDayName(firstDay)
                        + " & " + dayFormatter.GetAbbreviatedDayName(lastDay);
                }
                else if (days.Count == lastDay - firstDay + 1)
                {
                    return dayFormatter.GetAbbreviatedDayName(firstDay) +
                        $" {ndash} {dayFormatter.GetAbbreviatedDayName(lastDay)}";
                }
                else
                {
                    return string.Join(", ",
                        days.Select(_ => dayFormatter.GetAbbreviatedDayName(_)));
                }
            }
        }

        private List<LocationDayGrouping> ComputeWeeklyHours(ICollection<LocationHours> weeklyHours)
        {
            // Order weeklyHours to start on Monday
            weeklyHours = weeklyHours.OrderBy(_ => ((int)_.DayOfWeek + 6) % 7).ToList();

            var dayGroupings = new List<(List<DayOfWeek> DaysOfWeek,
                DateTime OpenTime,
                DateTime CloseTime)>();

            var closedDays = new List<DayOfWeek>();
            foreach (var day in weeklyHours)
            {
                if (day.Open)
                {
                    var (DaysOfWeek, OpenTime, CloseTime) = dayGroupings.LastOrDefault();
                    if (dayGroupings.Count > 0
                        && OpenTime == day.OpenTime
                        && CloseTime == day.CloseTime)
                    {
                        DaysOfWeek.Add(day.DayOfWeek);
                    }
                    else
                    {
                        dayGroupings.Add((
                            DaysOfWeek: new List<DayOfWeek> { day.DayOfWeek },
                            OpenTime: day.OpenTime.Value,
                            CloseTime: day.CloseTime.Value));
                    }
                }
                else
                {
                    closedDays.Add(day.DayOfWeek);
                }
            }

            var formattedDayGroupings = new List<LocationDayGrouping>();
            foreach (var (DaysOfWeek, OpenTime, CloseTime) in dayGroupings)
            {
                var days = GetFormattedDayGroupings(DaysOfWeek);

                var locationDayGrouping = new LocationDayGrouping
                {
                    Close = CloseTime.TimeOfDay,
                    Days = days,
                    Open = OpenTime.TimeOfDay,
                    Time = FormatOpeningHours(OpenTime, CloseTime)
                };
                ((List<DayOfWeek>)locationDayGrouping.DaysOfWeek).AddRange(DaysOfWeek);
                formattedDayGroupings.Add(locationDayGrouping);
            }

            if (closedDays.Count > 0)
            {
                var formattedClosedDays = GetFormattedDayGroupings(closedDays);
                var locationDayGrouping = new LocationDayGrouping
                {
                    Days = formattedClosedDays,
                    Time = _localizer[i18n.Keys.Promenade.LocationClosed]
                };
                ((List<DayOfWeek>)locationDayGrouping.DaysOfWeek).AddRange(closedDays);
                formattedDayGroupings.Add(locationDayGrouping);
            }

            return formattedDayGroupings;
        }

        private async Task<ICollection<LocationHours>> GetScheduleAsync(int locationId,
            bool forceReload)
        {
            var currentDefaultLanguageId = await GetCurrentDefaultLanguageIdAsync(_contextAccessor,
                _languageService);

            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLocationWeeklyHours,
                locationId,
                currentDefaultLanguageId.First());

            ICollection<LocationHours> weeklyHours = null;

            if (!forceReload)
            {
                weeklyHours = await _cache
                    .GetObjectFromCacheAsync<ICollection<LocationHours>>(cacheKey);
            }

            if (weeklyHours != null)
            {
                return weeklyHours;
            }

            weeklyHours = await _locationHoursRepository.GetWeeklyHoursAsync(locationId);

            if (weeklyHours != null)
            {
                await _cache.SaveToCacheAsync(cacheKey, weeklyHours, CacheLocationScheduleHours);
            }

            return weeklyHours;
        }
    }
}