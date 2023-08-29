using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using FamilyHubs.SharedKernel.Razor.Header;
using FamilyHubs.SharedKernel.Razor.Links;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Shared;

public class HeaderPageModel : PageModel, IFamilyHubsHeader
{
    //todo: we could add the status to IFhRenderLink and let the consumer use either method
    IEnumerable<IFhRenderLink> IFamilyHubsHeader.NavigationLinks(
        FhLinkOptions[] navigationLinks,
        IFamilyHubsUiOptions familyHubsUiOptions)
    {
        string role = HttpContext.GetRole();

        return role is RoleTypes.VcsProfessional or RoleTypes.VcsDualRole
            ? navigationLinks.Select(nl => nl.Text == "Requests sent"
                ? (IFhRenderLink)new FhRenderLink("My requests")
                {
                    Url = nl.Url,
                    OpenInNewTab = nl.OpenInNewTab
                } : nl)
            : navigationLinks;

        //or
        //return role is RoleTypes.VcsProfessional or RoleTypes.VcsDualRole
        //    ? familyHubsUiOptions.GetAlternative("VcsHeader").Header.NavigationLinks
        //    : navigationLinks;
    }

    LinkStatus IFamilyHubsHeader.GetStatus(IFhRenderLink link)
    {
        return link.Text is "Requests sent" or "My requests" ? LinkStatus.Active : LinkStatus.Visible;
    }
}