using System.Web;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;

namespace FamilyHubs.RequestForSupport.Web.LaDashboard;

public class LaDashboardRow : IRow<ReferralDto>
{
    private readonly Uri _connectWebBaseUrl;
    public ReferralDto Item { get; }

    public LaDashboardRow(ReferralDto referral, Uri connectWebBaseUrl)
    {
        _connectWebBaseUrl = connectWebBaseUrl;
        Item = referral;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            yield return new Cell(new Uri(_connectWebBaseUrl, 
                new Uri($"<a href=\"/La/RequestDetails?id={Item.Id}\">{HttpUtility.HtmlEncode(Item.RecipientDto.Name)}</a>")).ToString());
            yield return new Cell(Item.ReferralServiceDto.Name);
            yield return new Cell(Item.LastModified?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Created?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Id.ToString("X6"));
            yield return new Cell(null, "_LaConnectionStatus");
        }
    }
}