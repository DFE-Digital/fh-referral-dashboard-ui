using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using System.Web;

namespace FamilyHubs.RequestForSupport.Web.VcsDashboard;

public class VcsDashboardRow : IRow<ReferralDto>
{
    private readonly Uri _vcsWebBaseUrl;
    public ReferralDto Item { get; }

    public VcsDashboardRow(ReferralDto referral, Uri vcsWebBaseUrl)
    {
        _vcsWebBaseUrl = vcsWebBaseUrl;
        Item = referral;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            var requestDetailsUrl = $"{_vcsWebBaseUrl}Vcs/RequestDetails?id={Item.Id}";
            yield return new Cell($"<a href=\"{requestDetailsUrl}\">{HttpUtility.HtmlEncode(Item.RecipientDto.Name)}</a>");
            yield return new Cell(Item.Created?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Id.ToString("X6"));
            yield return new Cell(null, "_VcsConnectionStatus");
        }
    }
}