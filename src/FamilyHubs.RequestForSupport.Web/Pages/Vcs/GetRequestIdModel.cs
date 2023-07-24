using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
public class GetRequestIdModel : PageModel, IFamilyHubsHeader
{
    public int? RequestId { get; private set; }

    public void OnGet(int id)
    {
        RequestId = id;
    }
}