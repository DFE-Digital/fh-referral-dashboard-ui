using System.Collections.Immutable;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

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

public enum ErrorId
{
    SelectAnOption,
    EnterReasonForDeclining
}

public record Error(string HtmlElementId, string ErrorMessage);

[Authorize]
public class VcsRequestDetailsPageModel : PageModel, IFamilyHubsHeader
{
    private readonly IReferralClientService _referralClientService;
    public ReferralDto Referral { get; set; } = default!;

    [BindProperty]
    public string? ReasonForRejection { get; set; }

    [BindProperty]
    public AcceptDecline? AcceptOrDecline { get; set; }

    public static readonly ImmutableDictionary<ErrorId, Error> PossibleErrors = ImmutableDictionary
        .Create<ErrorId, Error>()
        .Add(ErrorId.SelectAnOption, new Error("accept-request", "Select an option"))
        .Add(ErrorId.EnterReasonForDeclining, new Error("reason-for-declining", "Enter a reason for declining"));

    public List<ErrorId>? Errors { get; set; }

    public VcsRequestDetailsPageModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
    }

    //todo: need to guard against user changing the id in the url to see a request they shouldn't have access to
    public async Task OnGet(int id, List<ErrorId> errors)
    {
        Errors = errors;
        //todo: check errorIds are valid

        Referral = await _referralClientService.GetReferralById(id);
    }

    //todo: PRG?
    public async Task<IActionResult> OnPost(UserAction userAction, int id)
    {
        ReferralStatus? newStatus = null;
        string? redirectUrl = null;
        string? reason = null;
        Errors = new List<ErrorId>();

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
                        if (string.IsNullOrEmpty(ReasonForRejection))
                        {
                            Errors.Add(ErrorId.EnterReasonForDeclining);
                        }
                        newStatus = ReferralStatus.Declined;
                        redirectUrl = "/Vcs/RequestDeclined";
                        reason = ReasonForRejection;
                        break;
                    default:
                        Errors.Add(ErrorId.SelectAnOption);
                        break;
                }
                break;
            case UserAction.ReturnLater:
                newStatus = ReferralStatus.Opened;
                redirectUrl = "/Vcs/Dashboard";
                break;
        }

        if (Errors.Any())
        {
            return RedirectToPage(new {id, Errors});
        }

        if (newStatus == null)
        {
            throw new InvalidOperationException("Unexpected values in posted form");
        }

        await _referralClientService.UpdateReferralStatus(id, newStatus.Value, reason);

        return RedirectToPage(redirectUrl);
    }

    public Error GetError(ErrorId errorId)
    {
        return PossibleErrors[errorId];
    }
}
