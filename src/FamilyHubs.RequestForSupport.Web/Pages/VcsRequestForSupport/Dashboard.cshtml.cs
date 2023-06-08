using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.RequestForSupport.Web.Dashboard;
using FamilyHubs.RequestForSupport.Web.VcsDashboard;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using FamilyHubs.SharedKernel.Razor.Pagination;

namespace FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;

//todo: most of this can go in a base class
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

    public IPagination Pagination { get; set; }

    public const int PageSize = 10;

    public SortOrder[]? Sort { get; set; }

    private IEnumerable<IDashboardColumnHeader> _columnHeaders = Enumerable.Empty<IDashboardColumnHeader>();
    private IEnumerable<IDashboardRow<ReferralDto>> _rows = Enumerable.Empty<IDashboardRow<ReferralDto>>();
    IEnumerable<IDashboardColumnHeader> IDashboard<ReferralDto>.ColumnHeaders => _columnHeaders;
    IEnumerable<IDashboardRow<ReferralDto>> IDashboard<ReferralDto>.Rows => _rows;

    public DashboardModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
        //todo: nullable, so don't have to create this dummy?
        Pagination = new DontShowPagination();
    }

    public async Task OnGet(string? columnName, SortOrder sort, int? currentPage)
    {
        var user = HttpContext.GetFamilyHubsUser();
        if (user.Role != "VcsAdmin")
        {
            RedirectToPage("/Error/401");
        }

        if (columnName == null|| !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.DateReceived;
            sort = SortOrder.descending;
        }

        _columnHeaders = new DashboardColumnHeaderFactory(_columnImmutables, "/VcsRequestForSupport/Dashboard", column.ToString(), sort).CreateAll();

        currentPage ??= 1;

        var searchResults = await GetConnections(user.OrganisationId, currentPage.Value, column, sort);

        _rows = searchResults.Items.Select(r => new VcsDashboardRow(r));

        Pagination = new DashboardPagination(searchResults!.TotalPages, currentPage.Value, column, sort);
    }

    private async Task<PaginatedList<ReferralDto>> GetConnections(
        string organisationId,
        int currentPage,
        Column column,
        SortOrder sort)
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
            organisationId, referralOrderBy, sort == SortOrder.ascending, currentPage, PageSize);
    }

    LinkStatus IFamilyHubsHeader.GetStatus(SharedKernel.Razor.FamilyHubsUi.Options.LinkOptions link)
    {
        return link.Text == "Received requests" ? LinkStatus.Active : LinkStatus.Visible;
    }
}
