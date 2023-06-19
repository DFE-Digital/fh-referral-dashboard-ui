using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.RequestForSupport.Web.VcsDashboard;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using FamilyHubs.SharedKernel.Razor.Pagination;
using FamilyHubs.RequestForSupport.Web.Security;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

//todo: most of this can go in a base class
//todo: check handling of 401 (no access to service)
//todo: we've had to add the standard MS identity role to the stub users for now in addition to the existing role claim
// Mike is going to update the shared lib to handle this better
[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
public class DashboardModel : PageModel, IFamilyHubsHeader, IDashboard<ReferralDto>
{
    private static ColumnImmutable[] _columnImmutables = 
    {
        new("Contact in family", Column.ContactInFamily.ToString()),
        new("Date received", Column.DateReceived.ToString()),
        new("Request number"),
        new("Status", Column.Status.ToString())
    };

    private readonly IReferralClientService _referralClientService;

    string? IDashboard<ReferralDto>.TableClass => "app-vcs-dashboard";

    public IPagination Pagination { get; set; }

    public const int PageSize = 20;

    private IEnumerable<IColumnHeader> _columnHeaders = Enumerable.Empty<IColumnHeader>();
    private IEnumerable<IRow<ReferralDto>> _rows = Enumerable.Empty<IRow<ReferralDto>>();
    IEnumerable<IColumnHeader> IDashboard<ReferralDto>.ColumnHeaders => _columnHeaders;
    IEnumerable<IRow<ReferralDto>> IDashboard<ReferralDto>.Rows => _rows;

    public DashboardModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
        //todo: nullable, so don't have to create this dummy?
        Pagination = new DontShowPagination();
    }

    public async Task OnGet(string? columnName, SortOrder sort, int? currentPage = 1)
    {
        if (columnName == null|| !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.DateReceived;
            sort = SortOrder.descending;
        }

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/Vcs/Dashboard", column.ToString(), sort)
            .CreateAll();

        var user = HttpContext.GetFamilyHubsUser();
        var searchResults = await GetConnections(user.OrganisationId, currentPage!.Value, column, sort);

        _rows = searchResults.Items.Select(r => new VcsDashboardRow(r));

        Pagination = new DashboardPagination(searchResults.TotalPages, currentPage.Value, column, sort);
    }

    private async Task<PaginatedList<ReferralDto>> GetConnections(
        string organisationId,
        int currentPage,
        Column column,
        SortOrder sort)
    {
        var referralOrderBy = column switch
        {
            Column.ContactInFamily => ReferralOrderBy.RecipientName,
            //todo: check sent == received
            Column.DateReceived => ReferralOrderBy.DateSent,
            Column.Status => ReferralOrderBy.Status,
            _ => throw new InvalidOperationException($"Unexpected sort column {column}")
        };

        return await _referralClientService.GetRequestsForConnectionByOrganisationId(
            organisationId, referralOrderBy, sort == SortOrder.ascending, currentPage, PageSize);
    }
}
