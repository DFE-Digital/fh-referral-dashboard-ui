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
        if(user.Role is RoleTypes.VcsProfessional or RoleTypes.VcsDualRole)
        {
            return RedirectToPage("/Vcs/Dashboard");
        }

        if (user.Role is RoleTypes.LaProfessional or RoleTypes.LaDualRole)
        {
            return RedirectToPage("/La/Dashboard");
        }

        return RedirectToPage("/Error/401");
    }
}