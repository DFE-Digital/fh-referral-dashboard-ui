using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

//todo: encoding strings is converting /r/n/ to &#xD;&#xA; convert line endings to <br /> first
//todo: declined requests shouldn't appear on the requests page

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
        //todo: handle when they don't click accept or decline

        ReferralStatus? newStatus = null;
        string? redirectUrl = null;
        string? reason = null;
        switch (userAction)
        {
            case UserAction.AcceptDecline:
                switch (AcceptOrDecline)
                {
                    case AcceptDecline.Accepted:
                        newStatus = ReferralStatus.Accepted;
                        redirectUrl = "/Vcs/RequestAccepted";
                        break;
                    case AcceptDecline.Declined:
                        newStatus = ReferralStatus.Declined;
                        redirectUrl = "/Vcs/RequestDeclined";
                        reason = ReasonForRejection;
                        break;
                }
                break;
            case UserAction.ReturnLater:
                newStatus = ReferralStatus.Opened;
                redirectUrl = "/Vcs/Dashboard";
                break;
        }

        if (newStatus == null)
        {
            throw new InvalidOperationException("Unexpected values in posted form");
        }

        await _referralClientService.UpdateReferralStatus(id, newStatus.Value, reason);

        return RedirectToPage(redirectUrl);
    }
}
