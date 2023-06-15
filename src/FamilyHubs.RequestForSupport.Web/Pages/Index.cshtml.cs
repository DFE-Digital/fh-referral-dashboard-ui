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
        //todo: vcs admin or pro
        if(user.Role == "VcsAdmin")
        {
            return RedirectToPage("/Vcs/Dashboard");
        }

        return RedirectToPage("/Error/401");
    }
}