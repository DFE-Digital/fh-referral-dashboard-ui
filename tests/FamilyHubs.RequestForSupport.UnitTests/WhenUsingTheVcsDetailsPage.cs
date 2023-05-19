using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;
using FamilyHubs.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDetailsPage
{
    private readonly VcsRequestDetailsPageModel _pageModel;
    private readonly Mock<IReferralClientService> _mockReferralClientService;

    public WhenUsingTheVcsDetailsPage()
    {
        List<ReferralStatusDto> listStatus = new List<ReferralStatusDto>()
        {
            new ReferralStatusDto()
            {
                Name = "New",
                SortOrder = 0
            },
            new ReferralStatusDto()
            {
                Name = "Opened",
                SortOrder = 1
            },
            new ReferralStatusDto()
            {
                Name = "Accepted",
                SortOrder = 2
            },
            new ReferralStatusDto()
            {
                Name = "Declined",
                SortOrder = 3
            },
        };
        _mockReferralClientService = new Mock<IReferralClientService>();
        _mockReferralClientService.Setup(x => x.GetReferralById(It.IsAny<long>())).ReturnsAsync(WhenUsingTheVcsDashboard.GetReferralDto());
        _mockReferralClientService.Setup(x => x.GetReferralStatuses()).ReturnsAsync(listStatus);
        

        var displayName = "User name";
        var identity = new GenericIdentity(displayName);
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.Role, "Professional"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.OrganisationId, "1"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.AccountStatus, "active"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.FullName, "Test User"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.LoginTime, DateTime.UtcNow.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Email, "Joe.Professional@email.com"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.PhoneNumber, "012345678"));
        var principle = new ClaimsPrincipal(identity);
        // use default context with user
        var httpContext = new DefaultHttpContext()
        {
            User = principle
        };

        //need these as well for the page context
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        // need page context for the page model
        var pageContext = new PageContext(actionContext)
        {
            ViewData = viewData
        };

        _pageModel = new VcsRequestDetailsPageModel(_mockReferralClientService.Object);
        _pageModel.PageContext = pageContext;
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
