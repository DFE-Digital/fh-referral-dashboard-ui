using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.RequestForSupport.Web.VcsDashboard;
using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;
using FamilyHubs.RequestForSupport.Web.Security;
using FamilyHubs.RequestForSupport.Web.Models;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Options;
using Microsoft.Extensions.Options;
using FamilyHubs.RequestForSupport.Web.Pages.Shared;

namespace FamilyHubs.RequestForSupport.Web.Pages.Vcs;

//todo: make back button remember dashboard state?
//todo: most of this can go in a base class
[Authorize(Roles = Roles.VcsProfessionalOrDualRole)]
public class DashboardModel : HeaderPageModel, IDashboard<ReferralDto>
{
    private static ColumnImmutable[] _columnImmutables = 
    {
        new("Contact in family", Column.ContactInFamily.ToString()),
        new("Date received", Column.DateReceived.ToString()),
        new("Request number"),
        new("Status", Column.Status.ToString())
    };

    private readonly IReferralClientService _referralClientService;
    private readonly FamilyHubsUiOptions _familyHubsUiOptions;

    string IDashboard<ReferralDto>.TableClass => "app-vcs-dashboard";

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
        if (columnName == null|| !Enum.TryParse(columnName, true, out Column column))
        {
            // default when first load the page, or user has manually changed the url
            column = Column.DateReceived;
            sort = SortOrder.descending;
        }

        Uri thisWebBaseUrl = _familyHubsUiOptions.Url(UrlKeys.ThisWeb);
        string vcsDashboardUrl = $"{thisWebBaseUrl}Vcs/Dashboard";

        _columnHeaders = new ColumnHeaderFactory(_columnImmutables, vcsDashboardUrl, column.ToString(), sort)
            .CreateAll();

        var user = HttpContext.GetFamilyHubsUser();
        var searchResults = await GetConnections(user.OrganisationId, currentPage!.Value, column, sort);

        _rows = searchResults.Items.Select(r => new VcsDashboardRow(r, thisWebBaseUrl));

        Pagination = new LargeSetLinkPagination<Column>(vcsDashboardUrl, searchResults.TotalPages, currentPage.Value, column, sort);
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
