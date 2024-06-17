﻿namespace Ocuda.Utility.Keys
{
    public static class Cache
    {
        /// <summary>
        /// Date and time of last expired slide purge
        /// </summary>
        public static readonly string OpsCleanupSlides = "cleanupslides";

        /// <summary>
        /// Status of a digital display, {0} is the digital display id
        ///
        /// </summary>
        public static readonly string OpsDigitalDisplayStatus = "dd.{0}";

        /// <summary>
        /// Date and time of last digital display status, {0} is the digital display id
        /// </summary>
        public static readonly string OpsDigitalDisplayStatusAt = "dd.at.{0}";

        /// <summary>
        /// Cached email setup element, {0} is the id of the element, {1} is the language id
        /// </summary>
        public static readonly string OpsEmailSetup = "emailsetup.{0}.{1}";

        /// <summary>
        /// Cached email template element, {0} is the id of the element, {1} is the language id
        /// </summary>
        public static readonly string OpsEmailTemplate = "emailtemplate.{0}.{1}";

        /// <summary>
        /// A user's groups as provided by the authentication system. Replace {0} with the user's
        /// identifier and {1} with a number starting with 1. Once the return is empty you have
        /// enumerated all of the available groups.
        /// </summary>
        public static readonly string OpsGroup = "auth.{0}.group{1}";

        /// <summary>
        /// Instance that runs jobs and how often it runs them
        /// </summary>
        public static readonly string OpsJobRunner = "jobrunner";

        /// <summary>
        /// Send message to stop the job runner, replace {0} with the instance name
        /// </summary>
        public static readonly string OpsJobStop = "jobstop.{0}";

        /// <summary>
        /// Left navigation JSON
        /// </summary>
        public static readonly string OpsLeftNav = "leftnav";

        /// <summary>
        /// A dictionary keyed off location slugs with values of the location names
        /// </summary>
        public static readonly string OpsLocationList = "locationlist";

        /// <summary>
        /// The return URL once the user is authenticated, replace {0} with the user's identifier.
        /// </summary>
        ///
        public static readonly string OpsReturn = "auth.{0}.return";

        /// <summary>
        /// Ops sections
        /// </summary>
        public static readonly string OpsSections = "sections";

        /// <summary>
        /// Cached site settings, {0} is the site setting key
        /// </summary>
        public static readonly string OpsSiteSetting = "sitesetting.{0}";

        /// <summary>
        /// The user's username, replace {0} with the user's identitifer.
        /// </summary>
        public static readonly string OpsUsername = "auth.{0}.username";

        /// <summary>
        /// The filename of the user's profile picture
        /// </summary>
        public static readonly string OpsUserProfilePicture = "user.picture.{0}";

        /// <summary>
        /// A count of how many volunteer emails have been sent recently, {0} is the email addrress
        /// </summary>
        public static readonly string OpsVolunteerEmails = "email.notification.volunteer.{0}";

        /// <summary>
        /// Cached segment id, looked up by age group id which is {0}
        /// </summary>
        public static readonly string PromAgeGroupIdToSegmentId = "agesegment.{0}";

        /// <summary>
        /// Cached card, {0} is the language id and {1} is the card id
        /// </summary>
        public static readonly string PromCardDetail = "card.{0}.{1}";

        /// <summary>
        /// Cached carousel, {0} is the carousel id
        /// </summary>
        public static readonly string PromCarousel = "carousel.{0}";

        /// <summary>
        /// Cached carousel button label text, {0} is the language id, {1} is the label id
        /// </summary>
        public static readonly string PromCarouselButtonLabelText = "carouselbuttonlabeltext.{0}.{1}";

        /// <summary>
        /// Cached carousel item for page header, {0} is the carousel item id, {1} is the
        /// page layout id
        /// </summary>
        public static readonly string PromCarouselItemForPageLayout = "carousel.{0}.{1}";

        /// <summary>
        /// Cached carousel item text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromCarouselItemText = "carouselitemtext.{0}.{1}";

        /// <summary>
        /// Cached carousel text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromCarouselText = "carouseltext.{0}.{1}";

        /// <summary>
        /// Cached emedia categories
        /// </summary>
        public static readonly string PromCategories = "categories";

        /// <summary>
        /// Cached emedia text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromCategoryText = "categorytext.{0}.{1}";

        /// <summary>
        /// A list of card IDs associated with a deck, in order
        /// </summary>
        public static readonly string PromDeckCardIds = "deck-cardids.{0}";

        /// <summary>
        /// Default language ID
        /// </summary>
        public static readonly string PromDefaultLanguageId = "langid-default";

        /// <summary>
        /// Cached emedia categories
        /// </summary>
        public static readonly string PromEmediaCategories = "emediacategories";

        /// <summary>
        /// Cached emedia groups
        /// </summary>
        public static readonly string PromEmediaGroups = "emediagroups";

        /// <summary>
        /// Cached emedia groups
        /// </summary>
        public static readonly string PromEmedias = "emedias";

        /// <summary>
        /// Cached emedia text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromEmediaText = "emediatext.{0}.{1}";

        /// <summary>
        /// Cached external resource list
        /// </summary>
        public static readonly string PromExternalResources = "externalresources";

        /// <summary>
        /// Feature slug to ID mapping, {0} is the feature slug
        /// </summary>
        public static readonly string PromFeatureSlug = "feat-slug.{0}";

        /// <summary>
        /// Cached image feature, {0} is the image feature id
        /// </summary>
        public static readonly string PromImageFeature = "imagefeature.{0}";

        /// <summary>
        /// Cached image feature item text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromImageFeatureItemText = "imagefeatureitemtext.{0}.{1}";

        /// <summary>
        /// Language id, {0} is the culture
        /// </summary>
        public static readonly string PromLanguageId = "langid.{0}";

        /// <summary>
        /// Language name, {0} is the language id
        /// </summary>
        public static readonly string PromLanguageName = "langname.{0}";

        /// <summary>
        /// Location, {0} is the location's id
        /// </summary>
        public static readonly string PromLocation = "loc.{0}";

        /// <summary>
        /// Location features, {0} is the location's id
        /// </summary>
        public static readonly string PromLocationFeatures = "loc-features.{0}";

        /// <summary>
        /// Location group, {0} is the location group id
        /// </summary>
        public static readonly string PromLocationGroup = "loc-group.{0}";

        /// <summary>
        /// An integer array of location ids
        /// </summary>
        public static readonly string PromLocationIds = "loc-ids.{0}";

        /// <summary>
        /// Alt text for a location interior image, {0} is image id, {1} is language id
        /// </summary>
        public static readonly string PromLocationImageAltText = "loc-image-alt.{0}.{1}";

        /// <summary>
        /// A list of location interior images, {0} is the location id
        /// </summary>
        public static readonly string PromLocationInteriorImages = "loc-interior-images.{0}";

        /// <summary>
        /// Location neighbor group
        /// </summary>
        public static readonly string PromLocationNeighborGroup = "loc-neighbor-group.{0}";

        /// <summary>
        /// Location id, {0} is the location's slug
        /// </summary>
        public static readonly string PromLocationSlugToId = "loc-id.{0}";

        /// <summary>
        /// Location status, {0} is the location's id, {1} is the language id
        /// </summary>
        public static readonly string PromLocationStatus = "loc-status.{0}.{1}";

        /// <summary>
        /// Weekly hours, {0} is the location's id, {1} is the language id
        /// </summary>
        public static readonly string PromLocationWeeklyHours = "loc-hours.{0}.{1}";

        /// <summary>
        /// Cached NavBanner image, {0} is the NavBanner id, {1} is the language id
        /// </summary>
        public static readonly string PromNavBannerImage = "navbannerimage.{0}.{1}";

        /// <summary>
        /// Cached NavBanner links, {0} is the nav banner id
        /// </summary>
        public static readonly string PromNavBannerLinks = "navbannerlinks.{0}";

        /// <summary>
        /// Cached NavBanner link text, {0} is the nav banner link id, {1} is the language id
        /// </summary>
        public static readonly string PromNavBannerLinkText = "navbannerlinktext.{0}.{1}";

        /// <summary>
        /// Cached navigation element, {0} is the id of the element, {1} is the language id
        /// </summary>
        public static readonly string PromNavLang = "nav.{0}.{1}";

        /// <summary>
        /// Cached page, {0} is the language id, {1} is the type, {2} is the stub
        /// </summary>
        public static readonly string PromPage = "page.{0}.{1}.{2}";

        /// <summary>
        /// Cached current page layout id from header, {0} is the header id
        /// </summary>
        public static readonly string PromPageCurrentLayoutId = "currentpagelayoutid.{0}";

        /// <summary>
        /// Cached page header info, {0} is the stub, {1} is the page type
        /// </summary>
        public static readonly string PromPageHeader = "pageheader.{0}.{1}";

        /// <summary>
        /// Cached page header info, {0} is the id, {1} is the page type
        /// </summary>
        public static readonly string PromPageHeaderSlug = "pageheaderslug.{0}.{1}";

        /// <summary>
        /// Cached page layout, {0} is the layout id
        /// </summary>
        public static readonly string PromPageLayout = "pagelayout.{0}";

        /// <summary>
        /// Cached page layout text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromPageLayoutText = "pagelayouttext.{0}.{1}";

        /// <summary>
        /// Cached podcast info, {0} is the podcast stub, {1} is whether to include blocked items
        /// </summary>
        public static readonly string PromPodcast = "podcast.{0}.{1}";

        /// <summary>
        /// Cached podcast items, {0} is the podcast id, {1} is whether to include blocked items
        /// </summary>
        public static readonly string PromPodcastItems = "podcastitems.{0}.{1}";

        /// <summary>
        /// Cached product, {0} is the product slug
        /// </summary>
        public static readonly string PromProduct = "product.{0}";

        /// <summary>
        /// Cached product inventory, {0} is the product id
        /// </summary>
        public static readonly string PromProductInventory = "productinventory.{0}";

        /// <summary>
        /// Caches set of product objects with just name and slug populated
        /// </summary>
        public static readonly string PromProductsSlugs = "productsslugs";

        /// <summary>
        /// Cached redirect path, {0} is the requested path
        /// </summary>
        public static readonly string PromRedirectPath = "redir.{0}";

        /// <summary>
        /// Cached schedule subjects
        /// </summary>
        public static readonly string PromScheduleSubjects = "schedulesubjects";

        /// <summary>
        /// Cached schedule subject texts, {0} is the language id, {1} is the schedule subject id
        /// </summary>
        public static readonly string PromScheduleSubjectTexts = "schedulesubjecttexts.{0}.{1}";

        /// <summary>
        /// Cached segment, {0} is the segment id
        /// </summary>
        public static readonly string PromSegment = "segment.{0}";

        /// <summary>
        /// Cached segment text, {0} is the language id, {1} is the id
        /// </summary>
        public static readonly string PromSegmentText = "segmenttext.{0}.{1}";

        /// <summary>
        /// Cached segment wrap, {0} is the segment wrap id
        /// </summary>
        public static readonly string PromSegmentWrap = "segmentwrap.{0}";

        /// <summary>
        /// Cached site settings, {0} is the site setting key
        /// </summary>
        public static readonly string PromSiteSetting = "sitesetting.{0}";

        /// <summary>
        /// Cached social card, {0} is the card id
        /// </summary>
        public static readonly string PromSocialCard = "socialcard.{0}";

        /// <summary>
        /// Cached volunteer forms, {0} is VolunteerFormType, {1} is location id
        /// </summary>
        public static readonly string PromVolunteerForm = "volform.{0}.{1}";

        /// <summary>
        /// Cached volunteer form location relationships, {0} is form id, {1} is location idm
        /// </summary>
        public static readonly string PromVolunteerFormLocation = "volformloc.{0}.{1}";
    }
}