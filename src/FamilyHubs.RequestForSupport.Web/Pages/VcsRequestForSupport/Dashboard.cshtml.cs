using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.RequestForSupport.Web.Dashboard;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using FamilyHubs.SharedKernel.Razor.Pagination;

namespace FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;

//todo: make some of these internal when move to shared
public class ColumnImmutable
{
    public string DisplayName { get; }
    public string? SortName { get; }

    public ColumnImmutable(string displayName, string? sortName = null)
    {
        DisplayName = displayName;
        SortName = sortName;
    }
}

public class DashboardColumnHeaderFactory
{
    private readonly IEnumerable<ColumnImmutable> _columnsImmutable;
    private readonly string _sortedColumnName;
    private readonly SortOrder _sort;
    private readonly string _pagePath;

    public DashboardColumnHeaderFactory(
        IEnumerable<ColumnImmutable> columnsImmutable,
        string pagePath,
        string sortedColumnName,
        SortOrder sort)
    {
        _columnsImmutable = columnsImmutable;
        _sortedColumnName = sortedColumnName;
        _sort = sort;
        _pagePath = pagePath;
    }

    private IDashboardColumnHeader Create(ColumnImmutable columnImmutable)
    {
        //todo: here, or in ctor?

        SortOrder? sort = null;
        if (columnImmutable.SortName != null)
        {
            sort = columnImmutable.SortName == _sortedColumnName ? _sort : SortOrder.none;
        }

        return new DashboardColumnHeader(columnImmutable, sort, _pagePath);
    }

    public IDashboardColumnHeader[] CreateAll()
    {
        return _columnsImmutable.Select(Create).ToArray();
    }
}

public class DashboardColumnHeader : IDashboardColumnHeader
{
    private readonly ColumnImmutable _columnImmutable;
    private readonly string _pagePath;

    public DashboardColumnHeader(ColumnImmutable columnImmutable, SortOrder? sort, string pagePath)
    {
        Sort = sort;
        _columnImmutable = columnImmutable;
        _pagePath = pagePath;
    }

    public string ContentAsHtml
    {
        get
        {
            if (Sort == null)
            {
                return _columnImmutable.DisplayName;
            }

            SortOrder clickSort = Sort switch
            {
                SortOrder.ascending => SortOrder.descending,
                _ => SortOrder.ascending
            };

            return $"<a href = \"{_pagePath}?columnName={_columnImmutable.SortName}&sort={clickSort}\">{_columnImmutable.DisplayName}</a>";
        }
    }

    public SortOrder? Sort { get; }
}

public interface IDashboardColumnHeader
{
    SortOrder? Sort { get; }
    string ContentAsHtml { get; }
}

public interface IDashboardCell
{
    string? PartialName { get; }
    string? ContentAsHtml { get; }
}

public record DashboardCell(string? ContentAsHtml, string? PartialName = null) : IDashboardCell;

public interface IDashboardRow<out T>
{
    IEnumerable<IDashboardCell> Cells { get; }
    T Item { get; }
}

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

public interface IDashboard<out T>
{
    string? TableClass { get; }
    IEnumerable<IDashboardColumnHeader> ColumnHeaders => Enumerable.Empty<IDashboardColumnHeader>();
    IEnumerable<IDashboardRow<T>> Rows => Enumerable.Empty<IDashboardRow<T>>();
    IPagination Pagination { get; set; }
}

// matches aria-sort values
public enum SortOrder
{
    // ReSharper disable InconsistentNaming
    none,
    ascending,
    descending
    // ReSharper enable InconsistentNaming
}

[Authorize]
public class DashboardModel : PageModel, IFamilyHubsHeader, IDashboard<ReferralDto>
{
    private static ColumnImmutable[] _columnImmutables = 
    {
        new("Recipient name", Column.RecipientName.ToString()),
        new("Date received", Column.DateReceived.ToString()),
        new("Request number"),
        new("Status", Column.Status.ToString())
    };

    private readonly IReferralClientService _referralClientService;

    string? IDashboard<ReferralDto>.TableClass => "app-vcs-dashboard";
    public bool ShowNavigationMenu => true;

    public PaginatedList<ReferralDto>? SearchResults { get; set; }

    public IPagination Pagination { get; set; }

    [BindProperty]
    public int CurrentPage { get; set; } = 1;
    public const int PageSize = 10;

    public SortOrder[]? Sort { get; set; }

    private IEnumerable<IDashboardColumnHeader> _columnHeaders = Enumerable.Empty<IDashboardColumnHeader>();
    private IEnumerable<IDashboardRow<ReferralDto>> _rows = Enumerable.Empty<IDashboardRow<ReferralDto>>();
    IEnumerable<IDashboardColumnHeader> IDashboard<ReferralDto>.ColumnHeaders => _columnHeaders;
    IEnumerable<IDashboardRow<ReferralDto>> IDashboard<ReferralDto>.Rows => _rows;

    public DashboardModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
        Pagination = new DontShowPagination();
    }

    public async Task OnGet(string? columnName, SortOrder sort, int? currentPage)
    {
        var user = HttpContext.GetFamilyHubsUser();
        if (user.Role != "VcsAdmin")
        {
            RedirectToPage("/Error/401");
        }

        if (currentPage != null)
            CurrentPage = currentPage.Value;

        Column column;
        if (columnName != null)
        {
            if (!Enum.TryParse(columnName, true, out column))
            {
                //todo: throw? default? someone's been messing with the url!
            }
        }
        else
        {
            // default when first load the page
            column = Column.DateReceived;
            sort = SortOrder.descending;
        }

        _columnHeaders = new DashboardColumnHeaderFactory(_columnImmutables, "/VcsRequestForSupport/Dashboard", column.ToString(), sort).CreateAll();

        SearchResults = await GetConnections(user.OrganisationId, column, sort);

        _rows = SearchResults.Items.Select(r => new VcsDashboardRow(r));

        Pagination = new DashboardPagination(SearchResults!.TotalPages, CurrentPage, column, sort);
    }

    private async Task<PaginatedList<ReferralDto>> GetConnections(string organisationId, Column column, SortOrder sort)
    {
        var referralOrderBy = column switch
        {
            Column.RecipientName => ReferralOrderBy.RecipientName,
            //todo: check sent == received
            Column.DateReceived => ReferralOrderBy.DateSent,
            Column.Status => ReferralOrderBy.Status,
            //todo: throw instead?
            _ => ReferralOrderBy.NotSet
        };

        //todo: assert/throw if Sort is None?

        return await _referralClientService.GetRequestsForConnectionByOrganisationId(
            organisationId, referralOrderBy, sort == SortOrder.ascending, CurrentPage, PageSize);
    }

    LinkStatus IFamilyHubsHeader.GetStatus(SharedKernel.Razor.FamilyHubsUi.Options.LinkOptions link)
    {
        return link.Text == "Received requests" ? LinkStatus.Active : LinkStatus.Visible;
    }
}
