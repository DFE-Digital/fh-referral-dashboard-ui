using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using FamilyHubs.RequestForSupport.Web.Models;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using FamilyHubs.SharedKernel.Razor.Header;
using Microsoft.Extensions.Options;

namespace FamilyHubs.RequestForSupport.Web.Pages.La;

//todo: new base model for header switcheroo
[Authorize(Roles = Roles.LaProfessionalOrDualRole)]
public class RequestDetailsModel : PageModel, IFamilyHubsHeader
{
    private readonly IReferralClientService _referralClientService;
    public ReferralDto Referral { get; set; } = default!;
    private readonly FamilyHubsUiOptions _familyHubsUiOptions;
    private readonly string _serviceUrl;

    public RequestDetailsModel(
        IReferralClientService referralClientService,
        IOptions<FamilyHubsUiOptions> familyHubsUiOptions)
    {
        _referralClientService = referralClientService;
        _familyHubsUiOptions = familyHubsUiOptions.Value;
        _serviceUrl = _familyHubsUiOptions.Url(UrlKeys.ConnectWeb,
            "ProfessionalReferral/LocalOfferDetail?serviceid=").ToString();
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
                return Redirect(_familyHubsUiOptions.Url(UrlKeys.ThisWeb, "/Error/403").ToString());
            }
            throw;
        }
        return Page();
    }

    public string GetReferralServiceUrl(long serviceId)
    {
        return $"{_serviceUrl}{serviceId}";
    }
}