
namespace RclTemp.FamilyHubsUi.Options;

public class FooterOptions
{
    public LinkOptions[] Links { get; set; } = Array.Empty<LinkOptions>();
}

public class LinkOptions
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string? ConfigUrl { get; set; }
}