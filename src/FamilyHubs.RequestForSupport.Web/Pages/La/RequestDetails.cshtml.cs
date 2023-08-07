using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using FamilyHubs.RequestForSupport.Web.Models;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.RequestForSupport.Web.Pages.La;

[Authorize(Roles = Roles.LaProfessionalOrDualRole)]
public class RequestDetailsModel : PageModel, IFamilyHubsHeader
{
    private readonly IReferralClientService _referralClientService;
    public ReferralDto Referral { get; set; } = default!;
    public string ServiceUrl { get; }

    public RequestDetailsModel(
        IReferralClientService referralClientService,
        IOptions<FamilyHubsUiOptions> familyHubsUiOptions)
    {
        _referralClientService = referralClientService;
        ServiceUrl = familyHubsUiOptions.Value
            .Url(UrlKeys.ConnectWeb, "ProfessionalReferral/LocalOfferDetail").ToString();
        ServiceUrl += "?serviceid=";
    }

    public async Task<IActionResult> OnGet(int id)
    {
        try
        {
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

    public string GetReferralServiceUrl(long serviceId)
    {
        return $"{ServiceUrl}{serviceId}";
    }
}