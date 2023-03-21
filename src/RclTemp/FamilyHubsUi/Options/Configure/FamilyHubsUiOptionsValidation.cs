using Microsoft.Extensions.Options;

namespace RclTemp.FamilyHubsUi.Options.Configure;

public class FamilyHubsUiOptionsValidation : IValidateOptions<FamilyHubsUiOptions>
{
    public ValidateOptionsResult Validate(string? _, FamilyHubsUiOptions options)
    {
        var validationErrors = new List<string>();
        foreach (var footerLink in options.Footer.Links)
        {
            if (!Uri.IsWellFormedUriString(footerLink.Url, UriKind.RelativeOrAbsolute))
            {
                validationErrors.Add($"Footer link for \"{footerLink.Text}\" has invalid Url \"{footerLink.Url}\"");
            }
        }

        if (validationErrors.Any())
        {
            return ValidateOptionsResult.Fail(validationErrors);
        }
        return ValidateOptionsResult.Success;
    }
}