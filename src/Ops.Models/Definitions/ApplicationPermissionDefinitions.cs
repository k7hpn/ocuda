using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Keys;

namespace Ocuda.Ops.Models.Definitions
{
    public static class ApplicationPermissionDefinitions
    {
        public static readonly ApplicationPermissionDefinition[] ApplicationPermissions =
            {
            new() {
                Id = ApplicationPermission.CoverIssueManagement,
                Name = "Cover Issue Management",
                Info = "Users with this permission are able to mark cover issues as resolved."
            },
            new() {
                Id = ApplicationPermission.DigitalDisplayContentManagement,
                Name = "Digital Display Content Management",
                Info = "Users with this permission can manage assets in digital display sets."
            },
            new() {
                Id = ApplicationPermission.EmediaManagement,
                Name = "Emedia Management",
                Info = "Users with this permission can manage the Promenade Emedia page."
            },
            new() {
                Id = ApplicationPermission.ImportPrograms,
                Name = "Import programs in bulk",
                Info = "Users with this permission can import a JSON file of programs and create associated events."
            },
            new() {
                Id = ApplicationPermission.IntranetFrontPageManagement,
                Name = "Intranet Front Page Management",
                Info = "Users with this permission can push section posts to the front page and pin them."
            },
            new() {
                Id = ApplicationPermission.NavigationManagement,
                Name = "Navigation Management",
                Info = "Users with this permission can manage the Promenade navigations."
            },
            new() {
                Id = ApplicationPermission.MultiUserAccount,
                Name = "Multi-user Account",
                Info = "Users with this permission are accessing via a multi-user domain account."
            },
            new() {
                Id = ApplicationPermission.PodcastShowNotesManagement,
                Name = "Podcast Show Notes Management",
                Info = "Users with this permission can add and edit podcast show notes."
            },
            new() {
                Id = ApplicationPermission.RosterManagement,
                Name = "Roster upload and unit mapping management",
                Info = "Users with this permission can upload rosters and manage mapping units to locations."
            },
            new() {
                Id = ApplicationPermission.UpdateProfilePictures,
                Name = "Update Profile Pictures",
                Info = "Users with this permission can update profile pictures for all users."
            },
            new() {
                Id = ApplicationPermission.UserSync,
                Name = "User sync with LDAP/AD",
                Info = "Users with this permission can synchronize users and locations with LDAP/AD."
            },
            new() {
                Id = ApplicationPermission.ViewAllIncidentReports,
                Name = "View All Incident Reports",
                Info = "Users with this permission can view all incident reports, not just their own."
            },
            new() {
                Id = ApplicationPermission.WebPageContentManagement,
                Name = "Web Page Content Management",
                Info = "Users with this permission can edit all pages."
            }
        };
    }
}