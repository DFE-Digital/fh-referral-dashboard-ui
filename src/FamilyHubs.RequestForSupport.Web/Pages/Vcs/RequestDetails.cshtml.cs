using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

//todo: status to open when viewed?
//todo: encoding strings is converting /r/n/ to &#xD;&#xA; convert line endings to <br /> first

public enum UserAction
{
    AcceptDecline,
    ReturnLater
}

public enum AcceptDecline
{
    Accepted,
    Declined
}

[Authorize]
public class VcsRequestDetailsPageModel : PageModel
{
    private readonly IReferralClientService _referralClientService;
    public ReferralDto Referral { get; set; } = default!;

    [BindProperty]
    public string? ReasonForRejection { get; set; }

    [BindProperty]
    public AcceptDecline? AcceptOrDecline { get; set; }

    public VcsRequestDetailsPageModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
    }

    public async Task OnGet(int id)
    {
        Referral = await _referralClientService.GetReferralById(id);
    }

    public async Task<IActionResult> OnPost(UserAction userAction, int id)
    {
        var referral = await _referralClientService.GetReferralById(id);

        //todo: handle when they don't click accept or decline

        //todo: api client shouldn't have to do this
        List<ReferralStatusDto> statuses = await _referralClientService.GetReferralStatuses();

        //todo: consts, or even better enum for status

        string? newStatus = null;
        string? redirectUrl = null;
        switch (userAction)
        {
            case UserAction.AcceptDecline:
                switch (AcceptOrDecline)
                {
                    case AcceptDecline.Accepted:
                        newStatus = "Accepted";
                        redirectUrl = "/Vcs/RequestAccepted";
                        break;
                    case AcceptDecline.Declined:
                        newStatus = "Declined";
                        redirectUrl = "/Vcs/RequestDeclined";
                        referral.ReasonForDecliningSupport = ReasonForRejection;
                        break;
                }
                break;
            case UserAction.ReturnLater:
                newStatus = "Opened";
                redirectUrl = "/Vcs/Dashboard";
                break;
        }

        if (newStatus == null)
        {
            throw new InvalidOperationException("Unexpected values in posted form");
        }

        referral.Status = statuses.Single(x => x.Name == newStatus);
        await _referralClientService.UpdateReferral(referral);

        return RedirectToPage(redirectUrl);
    }
}
