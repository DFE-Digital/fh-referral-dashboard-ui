
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace RclTemp.FamilyHubsUi.Models;

public enum Phase
{
    Alpha,
    Beta,
    Release
}

public interface IPhaseBanner
{
    Phase Phase { get; }
    string? FeedbackUrl { get; }
}

public class PhaseBanner : IPhaseBanner
{
    public Phase Phase { get; init; }

    public string? FeedbackUrl { get; init; }

    public PhaseBanner(Phase phase, string? feedbackUrl)
    {
        Phase = phase;
        FeedbackUrl = feedbackUrl;
        Debug.Assert(FeedbackUrl != null, "A feedback URL will be required before release to production.");
    }
}

public interface IConfigurationHelper
{
    //todo: do we want to support a default value?
    string GetUrl(
        string configurationKey,
        string? expectedValueDescription = null,
        string? example = "http://example.com",
        UriKind uriKind = UriKind.RelativeOrAbsolute);
}

public class ConfigurationHelper : IConfigurationHelper
{
    private readonly IConfiguration _configuration;

    public ConfigurationHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetUrl(
        string configurationKey,
        string? expectedValueDescription = null,
        string? example = "http://example.com",
        UriKind uriKind = UriKind.RelativeOrAbsolute)
    {
        string? url = _configuration[configurationKey];

        ConfigurationException.ThrowIfNotUrl(configurationKey, url, expectedValueDescription, example, uriKind);

        return url;
    }
}

#pragma warning disable S3925
public class ConfigurationException : Exception
{
    public ConfigurationException(string key, string? value, string? expected = null, string? example = null)
        : base(CreateMessage(key, value, expected, example))
    {
    }

    public ConfigurationException(string key, string? value, Exception? innerException, string? expected = null, string? example = null)
        : base(CreateMessage(key, value, expected, example), innerException)
    {
    }

    private static string CreateMessage(string key, string? value, string? expected, string? example)
    {
        return $"""
Configuration issue
Key      : "{key}"
Found    : "{value}"
Expected : {expected}
Example  : "{example}"
""";
    }

    /// <exception cref="ConfigurationException"></exception>
    public static string ThrowIfNotUrl(
        string key,
        [NotNull]
        string? url,
        string? expected = null,
        string? example = "http://example.com",
        UriKind uriKind = UriKind.RelativeOrAbsolute)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, uriKind))
        {
            throw new ConfigurationException(key, url, expected, example);
        }

        return url;
    }
}