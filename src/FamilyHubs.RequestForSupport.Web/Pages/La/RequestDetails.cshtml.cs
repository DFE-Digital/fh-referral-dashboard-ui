using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace FamilyHubs.RequestForSupport.Web.Pages.La;

[Authorize(Roles = Roles.LaProfessionalOrDualRole)]
public class RequestDetailsModel : PageModel, IFamilyHubsHeader
{
    private readonly IReferralClientService _referralClientService;
    public ReferralDto Referral { get; set; } = default!;

    public RequestDetailsModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
    }

    public async Task<IActionResult> OnGet(int id)
    {
        try
        {
            Referral = await _referralClientService.GetReferralById(id);
        }
        catch (HttpRequestException httpEx)
        {
            // user has changed the id in the url to see a referral they shouldn't have access to
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Error/403");
            }
            throw;
        }
        return Page();
    }
}