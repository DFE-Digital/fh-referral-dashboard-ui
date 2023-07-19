using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.SharedKernel.Identity;
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

public abstract class BaseWhenUsingPage
{
    protected readonly Mock<IReferralClientService> MockReferralClientService;
    protected readonly Mock<ITempDataDictionary> MockTempDataDictionary;

    protected BaseWhenUsingPage()
    {
        MockReferralClientService = new Mock<IReferralClientService>();

        MockTempDataDictionary = new Mock<ITempDataDictionary>();
        MockTempDataDictionary.Setup(x => x["ReasonForDeclining"]).Returns("example reason");
    }

    protected PageContext GetPageContext()
    {
        MockReferralClientService
            .Setup(x => x.GetReferralById(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(WhenUsingTheVcsDashboard.GetReferralDto());

        List<ReferralDto> list = new() { GetReferralDto() };
        PaginatedList<ReferralDto> pagelist = new PaginatedList<ReferralDto>(list, 1, 1, 1);
        MockReferralClientService
            .Setup(x => x.GetRequestsForConnectionByOrganisationId(It.IsAny<string>(), It.IsAny<ReferralOrderBy>(), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagelist);

        var displayName = "User name";
        var identity = new GenericIdentity(displayName);
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.Role, "VcsAdmin"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.OrganisationId, "1"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.AccountStatus, "active"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.FullName, "Test User"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.ClaimsValidTillTime, DateTime.UtcNow.AddMinutes(30).ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Email, "vcsAdmin2.VcsAdmin@stub.com"));
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
