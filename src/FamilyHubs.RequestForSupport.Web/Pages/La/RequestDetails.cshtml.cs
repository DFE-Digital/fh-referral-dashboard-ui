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
    public Uri ReferralUiUrl { get; set; }

    public RequestDetailsModel(
        IReferralClientService referralClientService,
        IConfiguration configuration)
    {
        _referralClientService = referralClientService;
        //todo: bring in our config exception
        ReferralUiUrl = new Uri(configuration["ReferralUiUrl"]
            ?? throw new InvalidOperationException("Missing config for ReferralUiUrl"));
    }

    public async Task<IActionResult> OnGet(int id)
    {
        try
        {
            //todo: api will need to be updated to check the user has access to this referral
            Referral = await _referralClientService.GetReferralById(id);
        }
        catch (ReferralClientServiceException ex)
        {
            // user has changed the id in the url to see a referral they shouldn't have access to
            if (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Error/403");
            }
            throw;
        }
        return Page();
    }

    public Uri GetReferralServiceUrl(long serviceId)
    {
        return new Uri(ReferralUiUrl, $"ProfessionalReferral/LocalOfferDetail?serviceid={serviceId}");
    }
}