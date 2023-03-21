using Microsoft.Extensions.Configuration;
using Moq;
using RclTemp.FamilyHubsUi.Options;
using RclTemp.FamilyHubsUi.Options.Configure;
using System.Text.Json;
using KellermanSoftware.CompareNetObjects;

namespace FamilyHubs.RequestForSupport.UnitTests.RclTemp.FamilyHubsUi.Configure;

public class FamilyHubsUiOptionsConfigureTests
{
    public FamilyHubsUiOptionsConfigure FamilyHubsUiOptionsConfigure { get; set; }
    public Mock<IConfiguration> Configuration { get; set; }
    public FamilyHubsUiOptions FamilyHubsUiOptions { get; set; }
    public CompareLogic Compare { get; set; }

    public FamilyHubsUiOptionsConfigureTests()
    {
        Compare = new CompareLogic();
        Configuration = new Mock<IConfiguration>();
        FamilyHubsUiOptionsConfigure = new FamilyHubsUiOptionsConfigure(Configuration.Object);

        FamilyHubsUiOptions = new FamilyHubsUiOptions
        {
            ServiceName = "ServiceName",
            Phase = Phase.Alpha,
            FeedbackUrl = "FeedbackUrl",
            Analytics = new AnalyticsOptions
            {
                CookieName = "CookieName",
                MeasurementId = "MeasurementId",
                ContainerId = "ContainerId"
            },
            Footer = new FooterOptions
            {
                Links = new[]
                {
                    new LinkOptions
                    {
                        Text = "Text",
                        Url = "text",
                        ConfigUrl = null
                    }
                }
            }
        };
    }

    [Fact]
    public void Configure_NoMutationTest()
    {
        var expectedFamilyHubsUiOptions = DeepClone(FamilyHubsUiOptions);

        // act
        FamilyHubsUiOptionsConfigure.Configure(FamilyHubsUiOptions);

        var compareResult = Compare.Compare(expectedFamilyHubsUiOptions, FamilyHubsUiOptions);
        Assert.True(compareResult.AreEqual, $"Not equal: {compareResult.DifferencesString}");
    }

    protected static T DeepClone<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}