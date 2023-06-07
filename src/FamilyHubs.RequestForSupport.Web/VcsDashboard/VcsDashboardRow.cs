using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;

namespace FamilyHubs.RequestForSupport.Web.VcsDashboard;

public class VcsDashboardRow : IDashboardRow<ReferralDto>
{
    public ReferralDto Item { get; }

    public VcsDashboardRow(ReferralDto referral)
    {
        Item = referral;
    }

    public IEnumerable<IDashboardCell> Cells
    {
        get
        {
            yield return new DashboardCell(
                $"<a href=\"/VcsRequestForSupport/ConnectDetails?id={Item.Id}\" class=\"govuk-!-margin-right-1\">{Item.RecipientDto.Name}</a>");
            yield return new DashboardCell(Item.Created?.ToString("dd-MMM-yyyy") ?? "");
            yield return new DashboardCell(Item.Id.ToString("X4"));
            yield return new DashboardCell(null, "_ConnectionStatus");
        }
    }
}