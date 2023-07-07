using System.Net;
using FamilyHubs.Notification.Api.Client;
using FamilyHubs.Notification.Api.Client.Templates;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Errors;
using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.SharedKernel.Razor.Errors;
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

public enum NotificationType
{
    ProfessionalAcceptedRequest,
    ProfessionalDeclinedRequest
}

[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
public class VcsRequestDetailsPageModel : PageModel, IFamilyHubsHeader
{
    private readonly IReferralClientService _referralClientService;
    private readonly INotifications _notifications;
    private readonly INotificationTemplates<NotificationType> _notificationTemplates;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VcsRequestDetailsPageModel> _logger;
    public ReferralDto Referral { get; set; } = default!;

    [BindProperty]
    public string? ReasonForRejection { get; set; }

    [BindProperty]
    public AcceptDecline? AcceptOrDecline { get; set; }

    public IErrorState ErrorState { get; private set; }

    public VcsRequestDetailsPageModel(
        IReferralClientService referralClientService,
        INotifications notifications,
        INotificationTemplates<NotificationType> notificationTemplates,
        IConfiguration configuration,
        ILogger<VcsRequestDetailsPageModel> logger)
    {
        _referralClientService = referralClientService;
        _notifications = notifications;
        _notificationTemplates = notificationTemplates;
        _configuration = configuration;
        _logger = logger;
        //todo: do something so doesn't have to be fully qualified
        ErrorState = SharedKernel.Razor.Errors.ErrorState.Empty;
    }

    public async Task<IActionResult> OnGet(int id, IEnumerable<ErrorId> errors)
    {
        ErrorState = SharedKernel.Razor.Errors.ErrorState.Create(PossibleErrors.All, errors);

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

        //todo: extract

        // we _could_ currently use INotificationTemplates<ReferralStatus> directly, but this is more future proof
        NotificationType? notificationType = newStatus switch
        {
            ReferralStatus.Accepted => NotificationType.ProfessionalAcceptedRequest,
            ReferralStatus.Declined => NotificationType.ProfessionalDeclinedRequest,
            _ => null
        };
        if (notificationType == null)
        {
            return RedirectToPage(redirectUrl, redirectRouteValues);
        }

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

        await TrySendNotificationEmails(Referral.ReferralUserAccountDto.EmailAddress, notificationType.Value, Referral.ReferralServiceDto.Name, id);

        return RedirectToPage(redirectUrl, redirectRouteValues);
    }

    private async Task TrySendNotificationEmails(
        string emailAddress,
        NotificationType notificationType,
        string serviceName,
        int requestNumber)
    {
        try
        {
            await SendNotificationEmails(emailAddress, notificationType, requestNumber, serviceName);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to send {NotificationType} email for request {RequestNumber}", notificationType, requestNumber);
        }
    }

    private async Task SendNotificationEmails(
        string emailAddress,
        NotificationType notificationType,
        int requestNumber,
        string serviceName)
    {
        string dashboardUrl = GetDashboardUrl();
        var viewConnectionRequestUrl = new UriBuilder(dashboardUrl)
        {
            Path = "La/RequestDetails",
            Query = $"id={requestNumber}"
        }.Uri;

        var emailTokens = new Dictionary<string, string>
        {
            { "RequestNumber", requestNumber.ToString("X6") },
            { "ServiceName", serviceName },
            { "ViewConnectionRequestUrl", viewConnectionRequestUrl.ToString()}
        };

        string templateId = _notificationTemplates.GetTemplateId(notificationType);

        //todo: change api to accept IEnumerable and dynamic dictionary
        await _notifications.SendEmailsAsync(new List<string> { emailAddress }, templateId, emailTokens);
    }

    private string GetDashboardUrl()
    {
        string? requestsSent = _configuration["DashboardUiUrl"];

        //todo: config exception
        if (string.IsNullOrEmpty(requestsSent))
        {
            //todo: use config exception
            throw new InvalidOperationException("DashboardUiUrl not set in config");
        }

        return requestsSent;
    }
}

