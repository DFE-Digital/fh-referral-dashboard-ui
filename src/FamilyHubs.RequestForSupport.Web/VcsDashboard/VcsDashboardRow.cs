﻿using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using System.Web;

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
                $"<a href=\"/VcsRequestForSupport/ConnectDetails?id={Item.Id}\" class=\"govuk-!-margin-right-1\">{HttpUtility.HtmlEncode(Item.RecipientDto.Name)}</a>");
            yield return new Cell(Item.Created?.ToString("dd-MMM-yyyy") ?? "");
            yield return new Cell(Item.Id.ToString("X6"));
            yield return new Cell(null, "_ConnectionStatus");
        }
    }
}