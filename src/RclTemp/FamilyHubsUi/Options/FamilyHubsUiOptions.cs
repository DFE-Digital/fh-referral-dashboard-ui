using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RclTemp.FamilyHubsUi.Models;

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

public class FamilyHubsUiOptionsValidation : IValidateOptions<FamilyHubsUiOptions>
{
    private readonly FamilyHubsUiOptions? _familyHubsUiOptions;

    public FamilyHubsUiOptionsValidation(IConfiguration configuration)
    {
        _familyHubsUiOptions = configuration.GetSection(FamilyHubsUiOptions.FamilyHubsUi)
            .Get<FamilyHubsUiOptions>();
    }

    public ValidateOptionsResult Validate(string? name, FamilyHubsUiOptions options)
    {
        return ValidateOptionsResult.Success;
    }
}

public class FamilyHubsUiOptionsConfigure : IConfigureOptions<FamilyHubsUiOptions>
{
    private readonly IConfiguration _configuration;

    public FamilyHubsUiOptionsConfigure(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(FamilyHubsUiOptions options)
    {
        foreach (var footerLink in options.Footer.Links)
        {
            if (footerLink.ConfigUrl != null)
            {
                footerLink.Url = _configuration[footerLink.ConfigUrl];
            }

            if (string.IsNullOrEmpty(footerLink.Url))
            {
                footerLink.Url = $"/{footerLink.Text.ToLowerInvariant().Replace(' ', '-')}";
            }
        }
    }
}