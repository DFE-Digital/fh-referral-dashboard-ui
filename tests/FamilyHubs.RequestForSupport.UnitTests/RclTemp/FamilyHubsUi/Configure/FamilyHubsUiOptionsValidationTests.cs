using FamilyHubs.RequestForSupport.UnitTests.RclTemp.FamilyHubsUi.Configure.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RclTemp.FamilyHubsUi.Options.Configure;

namespace FamilyHubs.RequestForSupport.UnitTests.RclTemp.FamilyHubsUi.Configure;

public class FamilyHubsUiOptionsValidationTests : FamilyHubsUiOptionsTestBase
{
    public FamilyHubsUiOptionsValidation FamilyHubsUiOptionsValidation { get; set; }

    public FamilyHubsUiOptionsValidationTests()
    {
        FamilyHubsUiOptionsValidation = new FamilyHubsUiOptionsValidation();
    }

    [Fact]
    public void Validate_NoValidationErrorsTest()
    {
        // act
        var result = FamilyHubsUiOptionsValidation.Validate(null, FamilyHubsUiOptions);

        Assert.Equal(ValidateOptionsResult.Success, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("h ttp://example.com")]
    public void Validate_ValidationErrorsTest(string? url)
    {
        var link = FamilyHubsUiOptions.Footer.Links.First();
        link.Url = url;

        var expectedResult =
            ValidateOptionsResult.Fail($"Footer link for \"{link.Text}\" has invalid Url \"{url}\"");

        // act
        var result = FamilyHubsUiOptionsValidation.Validate(null, FamilyHubsUiOptions);

        result.Should().BeEquivalentTo(expectedResult);
    }
}