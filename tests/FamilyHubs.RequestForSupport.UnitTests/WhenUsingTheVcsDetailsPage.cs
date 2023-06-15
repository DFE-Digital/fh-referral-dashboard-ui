using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Web.Pages.Vcs;
using FluentAssertions;
using Moq;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDetailsPage : BaseWhenUsingPage
{
    private readonly VcsRequestDetailsPageModel _pageModel;
    
    public WhenUsingTheVcsDetailsPage()
    {
        _pageModel = new VcsRequestDetailsPageModel(_mockReferralClientService.Object, GetConfiguration());
        _pageModel.PageContext = GetPageContext();
    }

    [Fact]
    public async Task ThenOnGetVcsDetails()
    {
        //Act & Arrange
        await _pageModel.OnGet(1L);

        //Assert
        _pageModel.Referral.Should().BeEquivalentTo(WhenUsingTheVcsDashboard.GetReferralDto());
    }

    [Theory]
    [InlineData("Accepted")]
    [InlineData("Declined")]
    public async Task ThenOnPostVcsDetails(string serviceRequestResponse)
    {
        //Arrange
        _pageModel.ServiceRequestResponse = serviceRequestResponse;
        _pageModel.ReasonForRejection = "Reason for Rejection";
        int updateCallback = 0;
        _mockReferralClientService.Setup(x => x.UpdateReferral(It.IsAny<ReferralDto>()))
            .Callback(() => updateCallback++)
            .ReturnsAsync("1");

        //Act
        await _pageModel.OnPost(1L);

        //Assert
        updateCallback.Should().Be(1);
    }
}
