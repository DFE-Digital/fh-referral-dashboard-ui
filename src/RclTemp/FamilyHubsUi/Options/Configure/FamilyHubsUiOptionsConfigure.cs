using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace RclTemp.FamilyHubsUi.Options.Configure;

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
            else if (string.IsNullOrEmpty(footerLink.Url))
            {
                footerLink.Url = $"/{footerLink.Text.ToLowerInvariant().Replace(' ', '-')}";
            }
        }
    }
}