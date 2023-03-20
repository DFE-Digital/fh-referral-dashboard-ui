using RclTemp.FamilyHubsUi.Models;

namespace RclTemp.FamilyHubsUi.Options;

public class AnalyticsOptions
{
    public string CookieName { get; set; } = "";
    public string MeasurementId { get; set; } = "";
    public string ContainerId { get; set; } = "";
}

public class FamilyHubsUiOptions
{
    public const string FamilyHubsUi = "FamilyHubsUi";

    public string ServiceName { get; set; } = "";
    public Phase Phase { get; set; }
    public string FeedbackUrl { get; set; } = "";

    public AnalyticsOptions? Analytics { get; set; }
}