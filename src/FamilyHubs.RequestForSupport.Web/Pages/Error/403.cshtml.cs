using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Error;

//todo: only show account/sign-out link if user is signed in
public class Error403Model : PageModel, IFamilyHubsHeader
{
    LinkStatus IFamilyHubsHeader.GetStatus(FhLinkOptions link)
    {
        return link.Text == "Received requests" ? LinkStatus.Active : LinkStatus.Visible;
    }
}