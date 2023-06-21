using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Pages.Vcs;
using FluentAssertions;
using Moq;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDetailsPage : BaseWhenUsingPage
{
    private readonly VcsRequestDetailsPageModel _pageModel;

    public WhenUsingTheVcsDetailsPage()
    {
        _pageModel = new VcsRequestDetailsPageModel(MockReferralClientService.Object)
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
