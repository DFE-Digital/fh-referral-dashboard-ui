using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.Models;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.SharedKernel.Razor.FamilyHubsUi.Delegators;

namespace FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;

//todo: support get/link and post/submit modes in pagination component

public enum Column
{
    RecipientName,
    DateReceived,
    Status,
    Last = Status
}

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
    public string OrganisationId { get; set; } = string.Empty;

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
        Sort columnNewSort;
        if (columnName != null)
        {
            if (!Enum.TryParse(columnName, true, out column))
            {
                //todo: throw? default? someone's been messing with the url!
            }

            columnNewSort = sort switch
            {
                Sort.ascending => Sort.descending,
                _ => Sort.ascending
            };
        }
        else
        {
            // default when first load the page
            column = Column.DateReceived;
            columnNewSort = Sort.descending;
        }

        ColumnSort = new Sort[(int)Column.Last+1];
        for (Column i = 0; i <= Column.Last; ++i)
        {
            ColumnSort[(int)i] = i == column ? columnNewSort : Sort.none;
        }

        OrganisationId = user.OrganisationId;
        //var team = Http   Context?.User.Claims.FirstOrDefault(x => x.Type == "Team");
        
        await GetConnections(OrganisationId, column, columnNewSort); 
    }

    //todo: need to add columnname and sort as hidden
    public async Task OnPost(string organisationId, string? columnName, Sort sort, int? currentPage)
    {
        if (!Enum.TryParse(columnName, true, out Column column))
        {
            //todo: throw? default? someone's been messing with the url!
        }

        //todo: currentpage
        await GetConnections(organisationId, column, sort);
    }

    private async Task GetConnections(string organisationId, Column column, Sort sort)
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

        SearchResults = await _referralClientService.GetRequestsForConnectionByOrganisationId(organisationId, referralOrderBy, sort == Sort.ascending, CurrentPage, PageSize);

        Pagination = new LargeSetPagination(SearchResults.TotalPages, CurrentPage);

        TotalResults = SearchResults.TotalCount;
    }

    public bool IsActive(SharedKernel.Razor.FamilyHubsUi.Options.LinkOptions link)
    {
        return link.Text == "Received requests";
    }
}
