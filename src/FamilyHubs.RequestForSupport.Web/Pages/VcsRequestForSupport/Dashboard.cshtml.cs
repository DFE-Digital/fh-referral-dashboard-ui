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

//todo: move to shared
// matches aria-sort values
public enum Sort
{
    // ReSharper disable InconsistentNaming
    none,
    ascending,
    descending
    // ReSharper enable InconsistentNaming
}

[Authorize]
public class DashboardModel : PageModel, IFamilyHubsHeader
{
    private readonly IReferralClientService _referralClientService;

    public bool ShowNavigationMenu => true;
    public bool ShowTeam { get; private set; }

    public PaginatedList<ReferralDto>? SearchResults { get; set; }

    public IPagination Pagination { get; set; }

    [BindProperty]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

    public Sort[]? ColumnSort { get; set; }

    public DashboardModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
        Pagination = new DontShowPagination();
    }

    public async Task OnGet(string? columnName, Sort sort, int? currentPage)
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
            sort = Sort.descending;
        }

        ColumnSort = new Sort[(int)Column.Last+1];
        for (Column i = 0; i <= Column.Last; ++i)
        {
            //todo: tidy up
            ColumnSort[(int)i] = i == column ? sort switch
            {
                Sort.ascending => Sort.descending,
                _ => Sort.ascending
            } : Sort.none;
        }

        SearchResults = await GetConnections(user.OrganisationId, column, sort);
        TotalResults = SearchResults.TotalCount;

        //todo: no pagination
        Pagination = new DashboardPagination(SearchResults!.TotalPages, CurrentPage, column, sort);
    }

    private async Task<PaginatedList<ReferralDto>> GetConnections(string organisationId, Column column, Sort sort)
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

        return await _referralClientService.GetRequestsForConnectionByOrganisationId(organisationId, referralOrderBy, sort == Sort.ascending, CurrentPage, PageSize);
    }

    LinkStatus IFamilyHubsHeader.GetStatus(SharedKernel.Razor.FamilyHubsUi.Options.LinkOptions link)
    {
        return link.Text == "Received requests" ? LinkStatus.Active : LinkStatus.Visible;
    }
}
