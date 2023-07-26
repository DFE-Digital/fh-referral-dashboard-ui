using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Error;

public class Error403Model : PageModel, IFamilyHubsHeader
{
    // user should only ever see the 401 page if they've logged in
    //bool IFamilyHubsHeader.ShowActionLinks => PageContext.HttpContext.User.Identity?.IsAuthenticated == true;

    LinkStatus IFamilyHubsHeader.GetStatus(FhLinkOptions link)
    {
        return link.Text == "Received requests" ? LinkStatus.Active : LinkStatus.Visible;
    }
}