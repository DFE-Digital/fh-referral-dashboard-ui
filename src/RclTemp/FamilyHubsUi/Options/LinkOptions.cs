using System.ComponentModel.DataAnnotations;

namespace RclTemp.FamilyHubsUi.Options;

public class LinkOptions
{
    /// <summary>
    /// The (visible) text of the link.
    /// </summary>
    [Required]
    public string Text { get; set; } = "";

    /// <summary>
    /// The URL for the link. If left blank, defaults to Text in lowercase with spaces converted to hyphens (-).
    /// </summary>
    /// <remarks>
    /// [Url]'s validation isn't fit for our purposes, so we perform custom validation in FamilyHubsUiOptionsValidation instead.
    /// </remarks>
    public string? Url { get; set; }

    /// <summary>
    /// If supplied, the Url is populated from the config value found at the given config key.
    /// </summary>
    /// <example>
    /// "FamilyHubsUi:FeedbackUrl"
    /// </example>
    public string? ConfigUrl { get; set; }
}