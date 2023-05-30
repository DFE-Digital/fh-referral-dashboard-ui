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

[Authorize]
public class DashboardModel : PageModel, IFamilyHubsHeader
{
    private readonly IReferralClientService _referralClientService;

    public bool ShowNavigationMenu => true;
    public bool ShowTeam { get; private set; }

    public PaginatedList<ReferralDto> SearchResults { get; set; } = new ();

    public IPagination Pagination { get; set; }

    [BindProperty]
    public string OrganisationId { get; set; } = string.Empty;

    [BindProperty]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

    public Dictionary<string, bool> ColumnSort { get; set; } = new()
    {
        { "Team", true },
        { "DateSent", true },
        { "Status", true }
    };

    public DashboardModel(IReferralClientService referralClientService, IConfiguration configuration)
    {
        _referralClientService = referralClientService;
        //todo: don't want this as a config, comment out with searchable tag instead
        ShowTeam = configuration.GetValue<bool>("ShowTeam");
        Pagination = new DontShowPagination();
    }

    public async Task OnGet(string? referralOrderBy, bool isAscending, int? currentPage)
    {
        var user = HttpContext.GetFamilyHubsUser();
        if (user.Role != "VcsAdmin")
        {
            RedirectToPage("/Error/401");
        }

        if (currentPage != null)
            CurrentPage = currentPage.Value;

        if (referralOrderBy != null)
        {
            ColumnSort[referralOrderBy] = !isAscending;
        }

        OrganisationId = user.OrganisationId;
        //var team = HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "Team");
        
        await GetConnections(OrganisationId, referralOrderBy, !isAscending); 
    }

    public async Task OnPost(string organisationId, string? referralOrderBy, bool? isAscending, int? currentPage)
    {
        //Check what we get
        await GetConnections(organisationId, referralOrderBy, isAscending);
    }

    private async Task GetConnections(string organisationId, string? referralOrderBy, bool? isAscending)
    {
        if (!Enum.TryParse(referralOrderBy, true, out ReferralOrderBy referralOrder))
        {
            referralOrder = ReferralOrderBy.NotSet;
        }

        SearchResults = await _referralClientService.GetRequestsForConnectionByOrganisationId(organisationId, referralOrder, isAscending, CurrentPage, PageSize);

        Pagination = new LargeSetPagination(SearchResults.TotalPages, CurrentPage);

        TotalResults = SearchResults.TotalCount;
    }

    public bool IsActive(SharedKernel.Razor.FamilyHubsUi.Options.LinkOptions link)
    {
        return link.Text == "Received requests";
    }
}
