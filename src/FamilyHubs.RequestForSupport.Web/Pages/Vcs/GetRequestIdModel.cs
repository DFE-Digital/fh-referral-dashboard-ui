using FamilyHubs.RequestForSupport.Web.Pages.Shared;
using FamilyHubs.RequestForSupport.Web.Security;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
public class GetRequestIdModel : HeaderPageModel
{
    public int? RequestId { get; private set; }

    public void OnGet(int id)
    {
        RequestId = id;
    }
}