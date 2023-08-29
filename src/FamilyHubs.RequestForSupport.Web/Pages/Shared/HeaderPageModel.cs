using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using FamilyHubs.SharedKernel.Razor.Header;
using FamilyHubs.SharedKernel.Razor.Links;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Shared;

public class HeaderPageModel : PageModel, IFamilyHubsHeader
{
    LinkStatus IFamilyHubsHeader.GetStatus(IFhRenderLink link)
    {
        //todo: ordering
        return link.Text == "Requests sent" ? LinkStatus.Active : LinkStatus.Visible;
    }

    IEnumerable<IFhRenderLink> IFamilyHubsHeader.NavigationLinks(
        FhLinkOptions[] navigationLinks,
        IFamilyHubsUiOptions familyHubsUiOptions)
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