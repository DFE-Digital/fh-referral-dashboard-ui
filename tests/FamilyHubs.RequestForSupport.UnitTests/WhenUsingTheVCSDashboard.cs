using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Pages.Vcs;
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
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Extensions;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDashboard
{
    private readonly DashboardModel _pageModel;
    private readonly Mock<IReferralClientService> _mockReferralClientService;
    private readonly Mock<IOptions<FamilyHubsUiOptions>> _mockOptionsFamilyHubsUiOptions;
    private readonly FamilyHubsUiOptions _familyHubsUiOptions;

    public WhenUsingTheVcsDashboard()
    {
        _mockReferralClientService = new Mock<IReferralClientService>();
        _mockOptionsFamilyHubsUiOptions = new Mock<IOptions<FamilyHubsUiOptions>>();
        _familyHubsUiOptions = new FamilyHubsUiOptions();

        List<ReferralDto> list = new() { GetReferralDto() };
        PaginatedList<ReferralDto> pagelist = new PaginatedList<ReferralDto>(list, 1, 1, 1);
        _mockReferralClientService.Setup(x => x.GetRequestsForConnectionByOrganisationId(It.IsAny<string>(), It.IsAny<ReferralOrderBy>(), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(pagelist);

        var displayName = "User name";
        var identity = new GenericIdentity(displayName);
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.Role, "Professional"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.OrganisationId, "1"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.AccountStatus, "active"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.FullName, "Test User"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.ClaimsValidTillTime, DateTime.UtcNow.AddMinutes(30).ToString()));
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

        _familyHubsUiOptions.Urls.Add("ThisWeb", new Uri("http://example.com").ToString());

        _mockOptionsFamilyHubsUiOptions.Setup(options => options.Value)
            .Returns(_familyHubsUiOptions);

        _pageModel = new DashboardModel(_mockReferralClientService.Object, _mockOptionsFamilyHubsUiOptions.Object)
        {
            PageContext = pageContext
        };
    }

    //todo: bit light on actual tests

    [Fact]
    public async Task ThenOnGetOneRowIsPrepared()
    {
        //Act & Arrange
        await _pageModel.OnGet("ContactInFamily", SortOrder.ascending);

        //Assert
        var dashboard = _pageModel as IDashboard<ReferralDto>;
        dashboard.Rows.Should().HaveCount(1);
    }

    public static ReferralDto GetReferralDto()
    {
        return new ReferralDto
        {
            Id = 2,
            ReasonForSupport = "Reason For Support",
            EngageWithFamily = "Engage With Family",
            RecipientDto = new RecipientDto
            {
                Id = 2,
                Name = "Joe Blogs",
                Email = "JoeBlog@email.com",
                Telephone = "078123456",
                TextPhone = "078123456",
                AddressLine1 = "Address Line 1",
                AddressLine2 = "Address Line 2",
                TownOrCity = "Town or City",
                County = "County",
                PostCode = "B30 2TV"
            },
            ReferralUserAccountDto = new UserAccountDto
            {
                Id = 2,
                EmailAddress = "Bob.Referrer@email.com",
                UserAccountRoles = new List<UserAccountRoleDto>
                {
                    new()
                    {
                        UserAccount = new UserAccountDto
                        {
                            EmailAddress = "Bob.Referrer@email.com",
                        },
                        Role = new RoleDto
                        {
                            Name = "LaProfessional"
                        }
                    }
                }
            },
            Status = new ReferralStatusDto
            {
                Name = "New",
                SortOrder = 0
            },
            ReferralServiceDto = new ReferralServiceDto
            {
                Id = 2,
                Name = "Service",
                Description = "Service Description",
                OrganisationDto = new OrganisationDto
                {
                    Id = 2,
                    Name = "Organisation",
                    Description = "Organisation Description",
                }
            }
        };
    }
}
