using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using FamilyHubs.SharedKernel.Razor.Header;
using FamilyHubs.SharedKernel.Razor.Links;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Shared;

public class HeaderPageModel : PageModel, IFamilyHubsHeader
{
    IEnumerable<IFhRenderLink> IFamilyHubsHeader.NavigationLinks(
        FhLinkOptions[] navigationLinks,
        IFamilyHubsUiOptions familyHubsUiOptions)
    {
        string role = HttpContext.GetRole();

        return role is RoleTypes.VcsProfessional or RoleTypes.VcsDualRole
            ? familyHubsUiOptions.GetAlternative("VcsHeader").Header.NavigationLinks
            : navigationLinks;
    }
}