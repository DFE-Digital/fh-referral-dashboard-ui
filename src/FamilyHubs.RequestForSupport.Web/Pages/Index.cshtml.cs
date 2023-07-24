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
            null or "" => "/Error/401",
            RoleTypes.VcsProfessional or RoleTypes.VcsDualRole => "/Vcs/Dashboard",
            RoleTypes.LaProfessional or RoleTypes.LaDualRole => "/La/Dashboard",
            _ => "/Error/403"
        };

        return RedirectToPage(redirect);
    }
}