﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Podcasts;
using Ocuda.Promenade.Service;
using Ocuda.Promenade.Service.Filters;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class PodcastsController : BaseController<PodcastsController>
    {
        private readonly IPathResolverService _pathResolverService;
        private readonly PodcastService _podcastService;
        private readonly SegmentService _segmentService;

        public PodcastsController(ServiceFacades.Controller<PodcastsController> context,
            IPathResolverService pathResolverService,
            PodcastService podcastService,
            SegmentService segmentService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(pathResolverService);
            ArgumentNullException.ThrowIfNull(podcastService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _pathResolverService = pathResolverService;
            _podcastService = podcastService;
            _segmentService = segmentService;
        }

        public static string Name
        { get { return "Podcasts"; } }

        [Route("{podcastStub}/{episodeStub}")]
        public async Task<IActionResult> Episode(string podcastStub, string episodeStub)
        {
            var podcast = await _podcastService.GetByStubAsync(podcastStub?.Trim());

            if (podcast == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var podcastItem = await _podcastService
                .GetItemByStubAsync(podcast.Id, episodeStub?.Trim());

            if (podcastItem == null)
            {
                return RedirectToAction(nameof(Podcast), new { stub = podcastStub });
            }

            podcast.ImageUrl = _pathResolverService.GetPublicContentLink(podcast.ImageUrl);
            if (!string.IsNullOrEmpty(podcast.ImageThumbnailUrl))
            {
                podcast.ImageThumbnailUrl
                    = _pathResolverService.GetPublicContentLink(podcast.ImageThumbnailUrl);
            }

            podcastItem.MediaUrl = _pathResolverService.GetPublicContentLink(podcastItem.MediaUrl);
            if (!string.IsNullOrWhiteSpace(podcastItem.ImageUrl))
            {
                podcastItem.ImageUrl = _pathResolverService
                    .GetPublicContentLink(podcastItem.ImageUrl);
            }

            var viewModel = new EpisodeViewModel
            {
                Podcast = podcast,
                PodcastItem = podcastItem
            };

            PageTitle = $"{podcastItem.Title} - {podcast.Title}";

            return View(viewModel);
        }

        [Route("")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);

            var podcasts = await _podcastService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = podcasts.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            if (podcasts.Count == 1)
            {
                return RedirectToAction(nameof(Podcast), new { stub = podcasts.Data.First().Stub });
            }
            else if (podcasts.Count == 0)
            {
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            foreach (var podcast in podcasts.Data)
            {
                podcast.ImageUrl = _pathResolverService.GetPublicContentLink(podcast.ImageUrl);
                if (!string.IsNullOrEmpty(podcast.ImageThumbnailUrl))
                {
                    podcast.ImageThumbnailUrl
                        = _pathResolverService.GetPublicContentLink(podcast.ImageThumbnailUrl);
                }
            }

            var viewModel = new IndexViewModel
            {
                Podcasts = podcasts.Data,
                PaginateModel = paginateModel
            };

            PageTitle = Name;

            return View(viewModel);
        }

        [Route("{stub}")]
        public async Task<IActionResult> Podcast(string stub, int page = 1)
        {
            var podcast = await _podcastService.GetByStubAsync(stub?.Trim());

            if (podcast == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var filter = new PodcastFilter(page)
            {
                SerialOrdering = podcast.IsSerial
            };

            var podcastItems = await _podcastService.GetPaginatedItemsByPodcastIdAsync(podcast.Id,
                filter);
            var paginateModel = new PaginateModel
            {
                ItemCount = podcastItems.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            if (podcastItems.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            podcast.ImageUrl = _pathResolverService.GetPublicContentLink(podcast.ImageUrl);

            if (!string.IsNullOrEmpty(podcast.ImageThumbnailUrl))
            {
                podcast.ImageThumbnailUrl
                    = _pathResolverService.GetPublicContentLink(podcast.ImageThumbnailUrl);
            }

            var viewModel = new PodcastViewModel
            {
                Podcast = podcast,
                PodcastDirectoryInfos = await _podcastService.GetDirectoryInfosByPodcastIdAsync(
                    podcast.Id),
                PodcastItems = podcastItems.Data,
                PaginateModel = paginateModel
            };

            var scheme = (await _siteSettingService
                .GetSettingBoolAsync(Models.Keys.SiteSetting.Site.IsTLS)) ? "https" : "http";
            viewModel.RSSUrl = Url.Action(nameof(RSS),
                Name,
                new { stub = podcast.Stub, culture = string.Empty },
                scheme);

            foreach (var item in viewModel.PodcastItems)
            {
                item.MediaUrl = _pathResolverService.GetPublicContentLink(item.MediaUrl);

                if (!string.IsNullOrWhiteSpace(item.ImageUrl))
                {
                    item.ImageUrl = _pathResolverService.GetPublicContentLink(item.ImageUrl);
                    viewModel.ShowEpisodeImages = true;
                }
            }

            PageTitle = podcast.Title;

            return View(viewModel);
        }

        [Route("[action]/{stub}")]
        public async Task<IActionResult> RSS(string stub)
        {
            var podcast = await _podcastService.GetByStubAsync(stub?.Trim(), true);

            if (podcast == null)
            {
                return NotFound();
            }

            long lastModified = podcast.UpdatedAt.Ticks;

            var podcastItems = await _podcastService.GetItemsByPodcastIdAsync(podcast.Id, true);

            if (podcastItems.Count == 0)
            {
                return NotFound();
            }

            var googleName = new XmlQualifiedName("googleplay", "http://www.w3.org/2000/xmlns/");
            XNamespace googleNS = "http://www.google.com/schemas/play-podcasts/1.0";

            var itunesName = new XmlQualifiedName("itunes", "http://www.w3.org/2000/xmlns/");
            XNamespace itunesNS = "http://www.itunes.com/dtds/podcast-1.0.dtd";

            var scheme = (await _siteSettingService
                .GetSettingBoolAsync(Models.Keys.SiteSetting.Site.IsTLS)) ? "https" : "http";

            var podcastUri = new Uri(
                Url.Action(nameof(Podcast),
                    Name,
                    new { stub = podcast.Stub, culture = string.Empty },
                    scheme),
                    UriKind.Absolute);

            var imageUrl = new UriBuilder
            {
                Host = podcastUri.Host,
                Path = _pathResolverService.GetPublicContentLink(podcast.ImageUrl),
                Port = podcastUri.Port,
                Scheme = podcastUri.Scheme
            }.Uri;

            var feed = new SyndicationFeed(
                    podcast.Title,
                    podcast.Description,
                    podcastUri)
            {
                Language = podcast.Language,
                ImageUrl = imageUrl,
            };

            if (!string.IsNullOrEmpty(podcast.Copyright))
            {
                feed.Copyright = new TextSyndicationContent(podcast.Copyright);
            }

            feed.AttributeExtensions.Add(googleName, googleNS.ToString());
            feed.AttributeExtensions.Add(itunesName, itunesNS.ToString());

            feed.ElementExtensions.Add("title", itunesNS.ToString(), podcast.Title);

            if (!string.IsNullOrEmpty(podcast.Subtitle))
            {
                feed.ElementExtensions.Add("subtitle", itunesNS.ToString(), podcast.Subtitle);
            }

            feed.ElementExtensions.Add("description", googleNS.ToString(), podcast.Description);
            feed.ElementExtensions.Add("summary", itunesNS.ToString(), podcast.Description);

            feed.ElementExtensions.Add(new XElement(googleNS + "image",
                new XAttribute("href", imageUrl)));
            feed.ElementExtensions.Add(new XElement(itunesNS + "image",
               new XAttribute("href", imageUrl)));

            feed.ElementExtensions.Add("author", googleNS.ToString(), podcast.Author);
            feed.ElementExtensions.Add("author", itunesNS.ToString(), podcast.Author);

            var itunesOwner = new XElement(itunesNS + "owner",
                    new XElement(itunesNS + "email", podcast.OwnerEmail),
                    new XElement(itunesNS + "name", podcast.OwnerName));

            feed.ElementExtensions.Add("owner", googleNS.ToString(), podcast.OwnerEmail);
            feed.ElementExtensions.Add(itunesOwner);

            var categories = podcast.Category.Split(':').Select(_ => _.Split(','))
                .Select(_ => (
                    Name: _[0],
                    Subcategory: _.Skip(1).FirstOrDefault())
                );

            if (categories.Any())
            {
                foreach (var category in categories)
                {
                    feed.ElementExtensions.Add(new XElement(googleNS + "category",
                        new XAttribute("text", category.Name)));

                    var itunesCategory = new XElement(itunesNS + "category",
                        new XAttribute("text", category.Name));
                    if (!string.IsNullOrWhiteSpace(category.Subcategory))
                    {
                        itunesCategory.Add(new XElement(itunesNS + "category",
                            new XAttribute("text", category.Subcategory)));
                    }
                    feed.ElementExtensions.Add(itunesCategory);
                }
            }

            if (podcast.IsExplicit)
            {
                feed.ElementExtensions.Add("explicit", googleNS.ToString(), "yes");
                feed.ElementExtensions.Add("explicit", itunesNS.ToString(), "true");
            }
            else
            {
                feed.ElementExtensions.Add("explicit", googleNS.ToString(), "no");
                feed.ElementExtensions.Add("explicit", itunesNS.ToString(), "false");
            }

            if (podcast.IsSerial)
            {
                feed.ElementExtensions.Add("type", itunesNS.ToString(), "serial");
            }

            if (podcast.IsBlocked)
            {
                feed.ElementExtensions.Add("block", googleNS.ToString(), "yes");
                feed.ElementExtensions.Add("block", itunesNS.ToString(), "Yes");
            }

            if (podcast.IsCompleted)
            {
                feed.ElementExtensions.Add("complete", itunesNS.ToString(), "Yes");
            }

            var items = new List<SyndicationItem>();

            foreach (var podcastItem in podcastItems)
            {
                var itemUri = new Uri(Url.Action(nameof(Episode), Name,
                    new { podcastStub = podcast.Stub, episodeStub = podcastItem.Stub, culture = string.Empty },
                    scheme),
                    UriKind.Absolute);

                var item = new SyndicationItem
                (
                    podcastItem.Title,
                    podcastItem.Description,
                    itemUri
                );

                item.ElementExtensions.Add("title", itunesNS.ToString(), podcastItem.Title);

                if (!string.IsNullOrEmpty(podcastItem.Subtitle))
                {
                    item.ElementExtensions.Add("subtitle",
                        itunesNS.ToString(),
                        podcastItem.Subtitle);
                }

                item.ElementExtensions.Add("description", googleNS.ToString(),
                    podcastItem.Description);
                item.ElementExtensions.Add("summary", itunesNS.ToString()
                    , podcastItem.Description);

                item.ElementExtensions.Add("author", googleNS.ToString(), podcast.Author);
                item.ElementExtensions.Add("author", itunesNS.ToString(), podcast.Author);

                item.Links.Add(SyndicationLink.CreateMediaEnclosureLink(
                    new UriBuilder
                    {
                        Host = itemUri.Host,
                        Path = _pathResolverService.GetPublicContentLink(podcastItem.MediaUrl),
                        Port = itemUri.Port,
                        Scheme = itemUri.Scheme
                    }.Uri,
                    podcastItem.MediaType,
                    podcastItem.MediaSize
                    ));

                item.PublishDate = podcastItem.PublishDate.Value;

                item.ElementExtensions.Add(new XElement("guid", podcastItem.Guid,
                    new XAttribute("isPermaLink", podcastItem.GuidPermaLink ? "true" : "false")));

                item.ElementExtensions.Add("duration", itunesNS.ToString(), podcastItem.Duration);

                if (!string.IsNullOrWhiteSpace(podcastItem.Keywords))
                {
                    item.ElementExtensions.Add("keywords",
                        itunesNS.ToString(),
                        podcastItem.Keywords);
                }

                if (!string.IsNullOrWhiteSpace(podcastItem.ImageUrl))
                {
                    var itemImageUrl = new UriBuilder
                    {
                        Host = itemUri.Host,
                        Path = _pathResolverService.GetPublicContentLink(podcastItem.ImageUrl),
                        Port = itemUri.Port,
                        Scheme = itemUri.Scheme
                    }.Uri;

                    item.ElementExtensions.Add(new XElement(itunesNS + "image",
                        new XAttribute("href", itemImageUrl)));
                }

                if (podcastItem.Episode.HasValue)
                {
                    if (podcastItem.Season.HasValue)
                    {
                        item.ElementExtensions.Add("season", itunesNS.ToString(),
                        podcastItem.Season.Value);
                    }

                    item.ElementExtensions.Add("episode", itunesNS.ToString(),
                        podcastItem.Episode.Value);
                }

                if (podcastItem.IsExplicit)
                {
                    item.ElementExtensions.Add("explicit", googleNS.ToString(), "yes");
                    item.ElementExtensions.Add("explicit", itunesNS.ToString(), "true");
                }
                else
                {
                    item.ElementExtensions.Add("explicit", googleNS.ToString(), "no");
                    item.ElementExtensions.Add("explicit", itunesNS.ToString(), "false");
                }

                if (podcastItem.IsBlocked)
                {
                    item.ElementExtensions.Add("block", googleNS.ToString(), "yes");
                    item.ElementExtensions.Add("block", itunesNS.ToString(), "Yes");
                }

                var itemLastModified = Math.Max(podcastItem.UpdatedAt.Ticks,
                    podcastItem.PublishDate?.Ticks ?? 0);

                if (itemLastModified > 0)
                {
                    item.LastUpdatedTime = new DateTime(itemLastModified);
                }

                items.Add(item);

                lastModified = Math.Max(itemLastModified, lastModified);
            }

            feed.Items = items;

            feed.LastUpdatedTime = new DateTime(lastModified);

            var settings = new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };

            await using var stream = new MemoryStream();
            await using var xmlWriter = XmlWriter.Create(stream, settings);

            var rssFormatter = new Rss20FeedFormatter(feed, false);
            rssFormatter.WriteTo(xmlWriter);
            await xmlWriter.FlushAsync();

            Response.Headers.LastModified = new DateTime(lastModified)
                    .ToUniversalTime()
                    .ToString("R", CultureInfo.InvariantCulture);

            return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
        }

        [Route("{podcastStub}/{episodeStub}/[action]")]
        public async Task<IActionResult> ShowNotes(string podcastStub, string episodeStub)
        {
            var podcast = await _podcastService.GetByStubAsync(podcastStub?.Trim());

            if (podcast == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var podcastItem = await _podcastService
                .GetItemByStubAsync(podcast.Id, episodeStub?.Trim());

            if (podcastItem == null)
            {
                return RedirectToAction(nameof(Podcast), new { stub = podcastStub });
            }

            if (podcastItem.ShowNotesSegmentId.HasValue)
            {
                var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

                var segmentText = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    podcastItem.ShowNotesSegmentId.Value, forceReload);

                segmentText.Text = FormatForDisplay(segmentText);

                var viewModel = new ShowNotesViewModel
                {
                    Podcast = podcast,
                    PodcastItem = podcastItem,
                    ShowNotesSegment = segmentText
                };

                PageTitle = $"{_localizer[i18n.Keys.Promenade.ShowNotesFor, podcastItem.Title]} - {podcast.Title}";

                return View(viewModel);
            }
            else
            {
                return NotFound();
            }
        }
    }
}