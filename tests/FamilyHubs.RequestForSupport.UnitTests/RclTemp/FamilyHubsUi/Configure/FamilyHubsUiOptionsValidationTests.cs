using FamilyHubs.RequestForSupport.UnitTests.RclTemp.FamilyHubsUi.Configure.Helpers;
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
}