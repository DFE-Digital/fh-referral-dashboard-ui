using System.ComponentModel.DataAnnotations;

namespace RclTemp.FamilyHubsUi.Options;

public class LinkOptions
{
    [Required]
    public string Text { get; set; } = "";
    public string? Url { get; set; }
    public string? ConfigUrl { get; set; }
}