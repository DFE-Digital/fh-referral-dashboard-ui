using System.Collections.Immutable;
using System.Net;
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
    //todo: have NoError, or use null?
    NoError,
    SelectAResponse,
    EnterReasonForDeclining,
    ReasonForDecliningTooLong
}

public interface IErrorSummary
{
    bool HasErrors { get; }
    IEnumerable<ErrorId> ErrorIds { get; }
    Error GetError(ErrorId errorId);
}

public record Error(string HtmlElementId, string ErrorMessage);

[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
public class VcsRequestDetailsPageModel : PageModel, IFamilyHubsHeader, IErrorSummary
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

    public IEnumerable<ErrorId> ErrorIds { get; private set; } = Enumerable.Empty<ErrorId>();

    public VcsRequestDetailsPageModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
    }

    public async Task<IActionResult> OnGet(int id, IEnumerable<ErrorId> errors)
    {
        ErrorIds = errors;

        // if the user enters a reason for declining that's too long, then refreshes the page with the corresponding error message on, they'll lose their reason. quite an edge case though, and the site will still work, they'll just have to enter a shorter reason from scratch
        ReasonForRejection  = TempData["ReasonForDeclining"] as string;
        //todo: check errorIds are valid?

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

    //todo: component for character count (especially as there are some gotchas with line endings)
    //todo: component for standard error summary
    public async Task<IActionResult> OnPost(UserAction userAction, int id)
    {
        ReferralStatus? newStatus = null;
        string? redirectUrl = null;
        object? redirectRouteValues = null;
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
                        redirectRouteValues = new {id};
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
                            redirectRouteValues = new { id };
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

        return RedirectToPage(redirectUrl, redirectRouteValues);
    }

    public Error? GetCurrentError(params ErrorId[] mutuallyExclusiveErrorIds)
    {
        // SingleOrDefault would be safer, but this is faster
        ErrorId currentErrorId = ErrorIds.FirstOrDefault(mutuallyExclusiveErrorIds.Contains);
        return currentErrorId != ErrorId.NoError ? PossibleErrors[currentErrorId] : null;
    }

    Error IErrorSummary.GetError(ErrorId errorId)
    {
        return PossibleErrors[errorId];
    }

    bool IErrorSummary.HasErrors => ErrorIds.Any(e => e != ErrorId.NoError);
}

