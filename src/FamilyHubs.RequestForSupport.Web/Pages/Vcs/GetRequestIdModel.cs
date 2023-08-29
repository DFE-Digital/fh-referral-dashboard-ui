using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.SharedKernel.Razor.Header;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

//todo: new base model for header switcheroo
[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
public class GetRequestIdModel : PageModel, IFamilyHubsHeader
{
    public int? RequestId { get; private set; }

    public void OnGet(int id)
    {
        RequestId = id;
    }
}