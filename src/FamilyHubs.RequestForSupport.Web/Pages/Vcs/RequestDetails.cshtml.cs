using System.Collections.Immutable;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.SharedKernel.Identity;
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

[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
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

    public IEnumerable<ErrorId> Errors { get; private set; } = Enumerable.Empty<ErrorId>();

    public VcsRequestDetailsPageModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
    }

    //todo: when error is enter a reason for declining, we should preselect declining radio button
    //todo: when the error is reason too long, we should populate the reason field with the reason they entered (cut off to a decent limit)
    // where are we going to store that? url too long? session cookie?* redis?
    //todo: need to guard against user changing the id in the url to see a request they shouldn't have access to
    public async Task OnGet(int id, IEnumerable<ErrorId> errors)
    {
        //todo: service is being updated to check user has access to the referral. we might need a custom error page to handle it

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
        var errors = new List<ErrorId>();

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
                            errors.Add(ErrorId.EnterReasonForDeclining);
                        }
                        newStatus = ReferralStatus.Declined;
                        redirectUrl = "/Vcs/RequestDeclined";
                        reason = ReasonForRejection;
                        break;
                    default:
                        errors.Add(ErrorId.SelectAnOption);
                        break;
                }
                break;
            case UserAction.ReturnLater:
                newStatus = ReferralStatus.Opened;
                redirectUrl = "/Vcs/Dashboard";
                break;
        }

        if (errors.Any())
        {
            return RedirectToPage(new {id, errors});
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

    public bool HasErrors()
    {
        return Errors?.Any() == true;
    }
}

