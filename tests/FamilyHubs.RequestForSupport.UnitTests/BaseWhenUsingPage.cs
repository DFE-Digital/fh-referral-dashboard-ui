using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace FamilyHubs.RequestForSupport.UnitTests;

public abstract class BaseWhenUsingPage
{
    protected readonly Mock<IReferralClientService> _mockReferralClientService;

    protected BaseWhenUsingPage()
    {
        _mockReferralClientService = new Mock<IReferralClientService>();
    }

    protected PageContext GetPageContext()
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

        return pageContext;
    }

    public IConfiguration GetConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            {"ShowTeam", "False"}
        };

        var cfgBuilder = new ConfigurationBuilder();
        cfgBuilder.AddInMemoryCollection(settings);
        IConfiguration configuration = cfgBuilder.Build();
        return configuration;
    }
}
