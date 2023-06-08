using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;

namespace FamilyHubs.RequestForSupport.Web.VcsDashboard;

public class VcsDashboardRow : IRow<ReferralDto>
{
    public ReferralDto Item { get; }

    public VcsDashboardRow(ReferralDto referral)
    {
        Item = referral;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            yield return new Cell(
                $"<a href=\"/VcsRequestForSupport/ConnectDetails?id={Item.Id}\" class=\"govuk-!-margin-right-1\">{Item.RecipientDto.Name}</a>");
            yield return new Cell(Item.Created?.ToString("dd-MMM-yyyy") ?? "");
            yield return new Cell(Item.Id.ToString("X4"));
            yield return new Cell(null, "_ConnectionStatus");
        }
    }
}