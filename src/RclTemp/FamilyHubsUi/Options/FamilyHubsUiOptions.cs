﻿
namespace RclTemp.FamilyHubsUi.Options;

public class FamilyHubsUiOptions
{
    public const string FamilyHubsUi = "FamilyHubsUi";

    public string ServiceName { get; set; } = "";
    public Phase Phase { get; set; }
    public string FeedbackUrl { get; set; } = "";

    public AnalyticsOptions? Analytics { get; set; }

    public FooterOptions Footer { get; set; } = new();
}