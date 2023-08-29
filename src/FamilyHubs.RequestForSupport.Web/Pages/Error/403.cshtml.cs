using FamilyHubs.SharedKernel.Razor.Header;
using FamilyHubs.SharedKernel.Razor.Links;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Error;

public class Error403Model : PageModel, IFamilyHubsHeader
{
    // user should only ever see the 401 page if they've logged in
    //bool IFamilyHubsHeader.ShowActionLinks => PageContext.HttpContext.User.Identity?.IsAuthenticated == true;

    //todo: do we do this in requests.web?
    //todo: need to do switcheroo, a la PageHeaderModel
    LinkStatus IFamilyHubsHeader.GetStatus(IFhRenderLink link)
    {
        return link.Text == "Received requests" ? LinkStatus.Active : LinkStatus.Visible;
    }
}