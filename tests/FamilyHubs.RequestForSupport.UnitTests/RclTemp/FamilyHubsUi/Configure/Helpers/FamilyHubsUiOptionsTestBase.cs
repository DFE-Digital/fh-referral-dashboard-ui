using KellermanSoftware.CompareNetObjects;
using RclTemp.FamilyHubsUi.Options;

namespace FamilyHubs.RequestForSupport.UnitTests.RclTemp.FamilyHubsUi.Configure.Helpers;

public class FamilyHubsUiOptionsTestBase
{
    public FamilyHubsUiOptions FamilyHubsUiOptions { get; set; }
    //public CompareLogic Compare { get; set; }

    public FamilyHubsUiOptionsTestBase()
    {
        //Compare = new CompareLogic();

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

    //public void AssertEquivalent(object expectedObject, object actualObject)
    //{
    //    var compareResult = Compare.Compare(expectedObject, actualObject);
    //    Assert.True(compareResult.AreEqual, $"Not equal: {compareResult.DifferencesString}");
    //}
}