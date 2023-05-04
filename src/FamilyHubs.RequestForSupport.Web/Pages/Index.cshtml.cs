using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/VcsRequestForSupport/Dashboard", new
        {
            professional = "Joe.Professional@email.com"
        });
    }
}