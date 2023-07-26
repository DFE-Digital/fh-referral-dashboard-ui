using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var user = HttpContext.GetFamilyHubsUser();

        string redirect = user.Role switch
        {
            // this case should be picked up by the middleware, but we leave it here, so that there's no way we can end up with a 403, when it should be a 401
            null or "" => "/Error/401",
            RoleTypes.VcsProfessional or RoleTypes.VcsDualRole => "/Vcs/Dashboard",
            RoleTypes.LaProfessional or RoleTypes.LaDualRole => "/La/Dashboard",
            _ => "/Error/403"
        };

        return RedirectToPage(redirect);
    }
}