﻿using System.Web;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.SharedKernel.Razor.Dashboard;

namespace FamilyHubs.RequestForSupport.Web.LaDashboard;

public class LaDashboardRow : IRow<ReferralDto>
{
    private readonly Uri _laWebBaseUrl;
    public ReferralDto Item { get; }

    public LaDashboardRow(ReferralDto referral, Uri laWebBaseUrl)
    {
        _laWebBaseUrl = laWebBaseUrl;
        Item = referral;
    }

    public IEnumerable<ICell> Cells
    {
        get
        {
            var requestDetailsUrl = new Uri(_laWebBaseUrl, new Uri($"/La/RequestDetails?id={Item.Id}")).ToString();
            yield return new Cell($"<a href=\"{requestDetailsUrl}\">{HttpUtility.HtmlEncode(Item.RecipientDto.Name)}</a>");
            yield return new Cell(Item.ReferralServiceDto.Name);
            yield return new Cell(Item.LastModified?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Created?.ToString("dd MMM yyyy") ?? "");
            yield return new Cell(Item.Id.ToString("X6"));
            yield return new Cell(null, "_LaConnectionStatus");
        }
    }
}