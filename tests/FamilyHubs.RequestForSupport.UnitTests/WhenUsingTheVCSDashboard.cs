using Moq;
using FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using System.Collections.Generic;
using Xunit.Abstractions;
using FluentAssertions;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDashboard
{
    private readonly DashboardModel _pageModel;
    private readonly Mock<IReferralClientService> _mockReferralClientService;

    public WhenUsingTheVcsDashboard()
    {
        _mockReferralClientService = new Mock<IReferralClientService>();
        List<ReferralDto> list = new() { GetReferralDto() };
        PaginatedList<ReferralDto> pagelist = new PaginatedList<ReferralDto>(list, 1, 1, 1);
        _mockReferralClientService.Setup(x => x.GetRequestsForConnectionByProfessional(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(pagelist);

        _pageModel = new DashboardModel(_mockReferralClientService.Object);
    }

    [Fact]
    public async Task ThenOnGetVcsDashboard()
    {
        //Act & Arrange
        await _pageModel.OnGet("Joe.Professional@email.com", null, 1);

        //Assert
        _pageModel.ProfessionalEmailAddress.Should().Be("Joe.Professional@email.com");
        _pageModel.SearchResults.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ThenOnPostVcsDashboard()
    {
        //Act & Arrange
        await _pageModel.OnPost("Joe.Professional@email.com", "Joe Blogs");

        //Assert
        _pageModel.ProfessionalEmailAddress.Should().Be("Joe.Professional@email.com");
        _pageModel.SearchResults.TotalCount.Should().Be(1);
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
                Country = "Country",
                PostCode = "B30 2TV"
            },
            ReferrerDto = new ReferrerDto
            {
                Id = 2,
                EmailAddress = "Bob.Referrer@email.com",
            },
            Status = new List<ReferralStatusDto>
                {
                    new ReferralStatusDto
                    {
                        Status = "New"
                    }
                },
            ReferralServiceDto = new ReferralServiceDto
            {
                Id = 2,
                Name = "Service",
                Description = "Service Description",
                ReferralOrganisationDto = new ReferralOrganisationDto
                {
                    Id = 2,
                    Name = "Organisation",
                    Description = "Organisation Description",
                }
            }

        };
    }
}
