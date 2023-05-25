using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;
using FluentAssertions;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDashboard : BaseWhenUsingPage
{
    private readonly DashboardModel _pageModel;

    public WhenUsingTheVcsDashboard()
    {
        _pageModel = new DashboardModel(_mockReferralClientService.Object, GetConfiguration());
        _pageModel.PageContext = GetPageContext();
    }

    [Fact]
    public async Task ThenOnGetVcsDashboard()
    {
        //Act & Arrange
        await _pageModel.OnGet(ReferralOrderBy.NotSet.ToString(), false, 1);

        //Assert
        _pageModel.OrganisationId.Should().Be("1");
        _pageModel.SearchResults.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ThenOnPostVcsDashboard()
    {
        //Act & Arrange
        await _pageModel.OnPost("1", ReferralOrderBy.NotSet.ToString(), null, 1);

        //Assert
        _pageModel.SearchResults.TotalCount.Should().Be(1);
    }

    
}
