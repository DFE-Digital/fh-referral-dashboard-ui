using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.LaDashboard;
using FamilyHubs.RequestForSupport.Web.Models;
using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.Razor.AlternativeServices;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using FamilyHubs.SharedKernel.Razor.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FamilyHubs.RequestForSupport.Web.Pages.La;

//todo: make back button remember dashboard state?
//todo: check AccountStatus on claim? is it done auto?
//todo: add url for 401 (no access to service)
[Authorize(Roles = Roles.LaProfessionalOrDualRole)]
public class DashboardModel : PageModel, IFamilyHubsHeader, IDashboard<ReferralDto>, IAlternativeService
{
    public string ServiceName => "Connect";

    private static ColumnImmutable[] _columnImmutables =
    {
        new("Contact in family", Column.ContactInFamily.ToString()),
        new("Service", Column.Service.ToString()),
        new("Date updated", Column.DateUpdated.ToString()),
        new("Date sent", Column.DateSent.ToString()),
        new("Request number"),
        new("Status", Column.Status.ToString())
    };

    private readonly IReferralClientService _referralClientService;
    private readonly FamilyHubsUiOptions _familyHubsUiOptions;

    string? IDashboard<ReferralDto>.TableClass => "app-la-dashboard";

    public IPagination Pagination { get; set; }

    public const int PageSize = 20;

    private IEnumerable<IColumnHeader> _columnHeaders = Enumerable.Empty<IColumnHeader>();
    private IEnumerable<IRow<ReferralDto>> _rows = Enumerable.Empty<IRow<ReferralDto>>();
    IEnumerable<IColumnHeader> IDashboard<ReferralDto>.ColumnHeaders => _columnHeaders;
    IEnumerable<IRow<ReferralDto>> IDashboard<ReferralDto>.Rows => _rows;

    public DashboardModel(
        IReferralClientService referralClientService,
        IOptions<FamilyHubsUiOptions> familyHubsUiOptions)
    {
        _referralClientService = referralClientService;
        _familyHubsUiOptions = familyHubsUiOptions.Value;
        Pagination = IPagination.DontShow;
    }

    public async Task OnGet(string? columnName, SortOrder sort, int? currentPage = 1)
    {
        if (columnName == null || !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.DateUpdated;
            sort = SortOrder.descending;
        }

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, "/La/Dashboard", column.ToString(), sort)
            .CreateAll();

        var user = HttpContext.GetFamilyHubsUser();
        var searchResults = await GetConnections(user.AccountId, currentPage!.Value, column, sort);

        Uri laWebBaseUrl = _familyHubsUiOptions.GetAlternative(ServiceName).Url(UrlKeys.LaWeb);
        _rows = searchResults.Items.Select(r => new LaDashboardRow(r, laWebBaseUrl));

        Pagination = new LargeSetLinkPagination<Column>("/La/Dashboard", searchResults.TotalPages, currentPage.Value, column, sort);
    }

    private async Task<PaginatedList<ReferralDto>> GetConnections(
        string laProfessionalAccountId,
        int currentPage,
        Column column,
        SortOrder sort)
    {
        var referralOrderBy = column switch
        {
            Column.ContactInFamily => ReferralOrderBy.RecipientName,
            Column.Service => ReferralOrderBy.ServiceName,
            Column.DateUpdated => ReferralOrderBy.DateUpdated,
            Column.DateSent => ReferralOrderBy.DateSent,
            Column.Status => ReferralOrderBy.Status,
            _ => throw new InvalidOperationException($"Unexpected sort column {column}")
        };

        return await _referralClientService.GetRequestsByLaProfessional(
            laProfessionalAccountId, referralOrderBy, sort == SortOrder.ascending, currentPage, PageSize);
    }
}