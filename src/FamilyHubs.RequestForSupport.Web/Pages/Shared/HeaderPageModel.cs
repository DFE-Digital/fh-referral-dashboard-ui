using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Shared;

public class HeaderPageModel : PageModel, IFamilyHubsHeader
{
    LinkStatus IFamilyHubsHeader.GetStatus(FhLinkOptions link)
    {
        //todo: ordering
        return link.Text == "Requests sent" ? LinkStatus.Active : LinkStatus.Visible;
    }

    IEnumerable<FhLinkOptions> IFamilyHubsHeader.NavigationLinks(FhLinkOptions[] navigationLinks)
    {
        string role = HttpContext.GetRole();
        //todo: new copy ctor for FhLinkOptions?
        //todo: also is post configure, so perhaps introduce interface (+ class) for that
        return role is RoleTypes.VcsProfessional or RoleTypes.VcsDualRole
            ? navigationLinks.Select(nl => nl.Text == "Requests sent"
                ? new FhLinkOptions
                {
                    Text = "My requests",
                    Url = nl.Url,
                    OpenInNewTab = nl.OpenInNewTab
                } : nl)
            : navigationLinks;

        //or
        //return role is RoleTypes.VcsProfessional or RoleTypes.VcsDualRole
        //    ? familyHubsUiOptions.GetAlternative("VcsHeader").Header.NavigationLinks
        //    : navigationLinks;
    }
}