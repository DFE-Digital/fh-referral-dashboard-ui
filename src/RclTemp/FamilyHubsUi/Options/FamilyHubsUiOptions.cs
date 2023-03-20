using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RclTemp.FamilyHubsUi.Models;
using System.Reflection.Emit;

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
    //private readonly IHttpContextAccessor _accessor;
    //private readonly LinkGenerator _generator;

    public FamilyHubsUiOptionsConfigure(IConfiguration configuration) //, IHttpContextAccessor accessor, LinkGenerator generator)
    {
        _configuration = configuration;
        //_accessor = accessor;
        //_generator = generator;
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
                //footerLink.Url = _generator.GetUriByPage(_accessor.HttpContext!, $"/{footerLink.Text}/Index");
                footerLink.Url = $"/{footerLink.Text.ToLowerInvariant().Replace(' ', '-')}";
            }
        }
    }
}