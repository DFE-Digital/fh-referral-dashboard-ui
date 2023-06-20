using System.Collections.Immutable;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Security;
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
    NoError,
    SelectAResponse,
    EnterReasonForDeclining,
    ReasonForDecliningTooLong
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
        .Add(ErrorId.SelectAResponse, new Error("accept-request", "You must select a response"))
        .Add(ErrorId.EnterReasonForDeclining, new Error("decline-reason", "Enter a reason for declining"))
        .Add(ErrorId.ReasonForDecliningTooLong, new Error("decline-reason", "Reason for declining must be 500 characters or less"));

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
        //todo: service has being updated to check user has access to the referral.
        // redirect to error page if service returns a 403

        Errors = errors;
        // if the user enters a reason for declining that's too long, then refreshes the page with the corresponding error message on, they'll lose their reason. quite an edge case though, and the site will still work, they'll just have to enter a shorter reason from scratch    
        ReasonForRejection  = TempData["ReasonForDeclining"] as string;
        //todo: check errorIds are valid

        Referral = await _referralClientService.GetReferralById(id);
    }

    //todo: component for character count (especially as there are some gotchas with line endings)
    //todo: component for standard error summary
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
                        // workaround the front end counting line endings as 1 chars (\n) as per HTML spec,
                        // and the http transport/.net/windows using 2 chars for line ends (\r\n)
                        else if (ReasonForRejection.Replace("\r", "").Length > 500)
                        {
                            errors.Add(ErrorId.ReasonForDecliningTooLong);
                            // truncate at some large value, although it gets stored in cookie(s), so large values will only affect the user that sends them
                            TempData["ReasonForDeclining"] = ReasonForRejection[..Math.Min(ReasonForRejection.Length, 4000)];
                        }
                        else
                        {
                            newStatus = ReferralStatus.Declined;
                            redirectUrl = "/Vcs/RequestDeclined";
                            reason = ReasonForRejection;
                        }
                        break;
                    default:
                        errors.Add(ErrorId.SelectAResponse);
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

    public Error? GetCurrentError(params ErrorId[] mutuallyExclusiveErrorIds)
    {
        // SingleOrDefault would be safer, but this is faster
        ErrorId currentErrorId = Errors.FirstOrDefault(mutuallyExclusiveErrorIds.Contains);
        return currentErrorId != ErrorId.NoError ? PossibleErrors[currentErrorId] : null;
    }

    public Error GetError(ErrorId errorId)
    {
        return PossibleErrors[errorId];
    }

    public bool HasErrors => Errors.Any(e => e != ErrorId.NoError);
}

