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
using Microsoft.AspNetCore.Http.HttpResults;

namespace FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;

public class VcsColumnImmutable
{
    public string DisplayName { get; }
    public string? Name { get; }
    public Column? Column { get; }

    public VcsColumnImmutable(string displayName, Column? column)
    {
        DisplayName = displayName;
        Name = column?.ToString();
        Column = column;
    }
}

//todo: inject
//public interface IVcsDashboardColumnHeaderFactory
//{
//    IDashboardColumnHeader Create(string displayName, Column column);
//}

public class VcsDashboardColumnHeaderFactory //: IVcsDashboardColumnHeaderFactory
{
    private readonly Column _sortedColumn;
    private readonly SortOrder _sort;

    public VcsDashboardColumnHeaderFactory(Column sortedColumn, SortOrder sort)
    {
        _sortedColumn = sortedColumn;
        _sort = sort;
    }

    public IDashboardColumnHeader Create(VcsColumnImmutable columnImmutable)
    {
        //todo: here, or in ctor?

        SortOrder? sort = null;
        if (columnImmutable.Column != null)
        {
            sort = columnImmutable.Column == _sortedColumn ? _sort : SortOrder.none;
        }

        return new VcsDashboardColumnHeader(columnImmutable, sort);
    }

    public IDashboardColumnHeader[] CreateAll(IEnumerable<VcsColumnImmutable> columnsImmutable)
    {
        return columnsImmutable.Select(Create).ToArray();
    }
}

public class VcsDashboardColumnHeader : IDashboardColumnHeader
{
    private readonly VcsColumnImmutable _columnImmutable;

    public VcsDashboardColumnHeader(VcsColumnImmutable columnImmutable, SortOrder? sort)
    {
        Sort = sort;
        _columnImmutable = columnImmutable;
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

            return $"<a href = \"/VcsRequestForSupport/Dashboard?columnName={_columnImmutable.Name}&sort={clickSort}\">{_columnImmutable.DisplayName}</a>";
        }
    }

    //public string DisplayName => _columnImmutable.DisplayName;
    //public string? Name => _columnImmutable.Name;
    public SortOrder? Sort { get; }
}

public interface IDashboardColumnHeader
{
    //string DisplayName { get; }
    //string? Name { get; }
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
        //Cells = new DashboardCell[]
        //{
        //    new($"<a href=\"/VcsRequestForSupport/ConnectDetails?id={referral.Id}\" class=\"govuk-!-margin-right-1\">{referral.RecipientDto.Name}</a>"),
        //    new(referral.Created?.ToString("dd-MMM-yyyy") ?? ""),
        //    new(referral.Id.ToString("X4")),
        //    //todo: don't repeat partial name for each row??
        //    new(null, "_ConnectionStatus")
        //};
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

    //public IEnumerable<IDashboardCell> Cells { get; }
}

public interface IDashboard<out T>
{
    string? TableClass { get; }
    IEnumerable<IDashboardColumnHeader> ColumnHeaders => Enumerable.Empty<IDashboardColumnHeader>();
    IEnumerable<IDashboardRow<T>> Rows => Enumerable.Empty<IDashboardRow<T>>();
    IPagination Pagination { get; set; }
}

//todo: move to shared
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
    private static VcsColumnImmutable[] _columnImmutables = 
    {
        new("Recipient name", Column.RecipientName),
        new("Date received", Column.DateReceived),
        new("Request number", null),
        new("Status", Column.Status)
    };

    private readonly IReferralClientService _referralClientService;

    string? IDashboard<ReferralDto>.TableClass => "app-vcs-dashboard";
    public bool ShowNavigationMenu => true;

    public PaginatedList<ReferralDto>? SearchResults { get; set; }

    public IPagination Pagination { get; set; }

    [BindProperty]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

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

        _columnHeaders = new VcsDashboardColumnHeaderFactory(column, sort).CreateAll(_columnImmutables);

        SearchResults = await GetConnections(user.OrganisationId, column, sort);
        TotalResults = SearchResults.TotalCount;

        _rows = SearchResults.Items.Select(r => new VcsDashboardRow(r));

        //todo: no pagination
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
