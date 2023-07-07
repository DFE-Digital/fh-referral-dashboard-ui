﻿using FamilyHubs.Notification.Api.Client;
using FamilyHubs.Notification.Api.Client.Templates;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Errors;
using FamilyHubs.RequestForSupport.Web.Pages.Vcs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDetailsPage : BaseWhenUsingPage
{
    private readonly VcsRequestDetailsPageModel _pageModel;

    public Mock<INotifications> Notifications { get; set; }
    public Mock<INotificationTemplates<NotificationType>> NotificationTemplates { get; set; }
    public Mock<IConfiguration> Configuration { get; set; }
    public Mock<ILogger<VcsRequestDetailsPageModel>> Logger { get; set; }

    public WhenUsingTheVcsDetailsPage()
    {
        Notifications = new Mock<INotifications>();
        NotificationTemplates = new Mock<INotificationTemplates<NotificationType>>();
        Configuration = new Mock<IConfiguration>();
        Logger = new Mock<ILogger<VcsRequestDetailsPageModel>>();

        _pageModel = new VcsRequestDetailsPageModel(
            MockReferralClientService.Object,
            Notifications.Object,
            NotificationTemplates.Object,
            Configuration.Object,
            Logger.Object)
        {
            PageContext = GetPageContext(),
            TempData = MockTempDataDictionary.Object
        };
    }

    [Fact]
    public async Task OnGetShouldRetrieveReferral()
    {
        //Act
        await _pageModel.OnGet(1, Enumerable.Empty<ErrorId>());

        //Assert
        _pageModel.Referral.Should().BeEquivalentTo(WhenUsingTheVcsDashboard.GetReferralDto());
    }

    [Fact]
    public async Task OnGet_SetsReasonForRejection()
    {
        // Act
        await _pageModel.OnGet(123, new List<ErrorId> { ErrorId.ReasonForDecliningTooLong });

        _pageModel.ReasonForRejection.Should().Be("example reason");
    }

    [Theory]
    [InlineData(ReferralStatus.Accepted, AcceptDecline.Accepted)]
    [InlineData(ReferralStatus.Declined, AcceptDecline.Declined)]
    public async Task OnPostShouldSetAcceptOrDeclinedStatusCorrectly(
        ReferralStatus expectedReferralStatus, AcceptDecline acceptDecline)
    {
        const int referralId = 123;

        _pageModel.AcceptOrDecline = acceptDecline;
        _pageModel.ReasonForRejection = "Reason for Rejection";

        MockReferralClientService.Setup(x => x.UpdateReferralStatus(referralId, expectedReferralStatus, It.IsAny<string>()))
            .ReturnsAsync("1");

        // Act
        await _pageModel.OnPost(UserAction.AcceptDecline, referralId);

        MockReferralClientService.Verify(x => x.UpdateReferralStatus(referralId, expectedReferralStatus, It.IsAny<string>()), Times.Once);
    }
}
