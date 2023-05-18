using FamilyHubs.RequestForSupport.Core.ApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.Models;
using Microsoft.AspNetCore.Authorization;
using FamilyHubs.SharedKernel.Identity;
using Microsoft.AspNetCore.Http;

namespace FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IReferralClientService _referralClientService;

    public PaginatedList<ReferralDto> SearchResults { get; set; } = new PaginatedList<ReferralDto>();

    public IPagination Pagination { get; set; }

    [BindProperty]
    public string OrganisationId { get; set; } = string.Empty;

    [BindProperty]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

    public DashboardModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
        Pagination = new DontShowPagination();
    }
    public async Task OnGet(string? referralOrderBy, bool? isAssending, int? currentPage)
    {
        if (currentPage != null)
            CurrentPage = currentPage.Value;

        //var context = this.PageContext.HttpContext;
        var user = HttpContext.GetFamilyHubsUser();
        System.Diagnostics.Debug.WriteLine(user.LastName);
        OrganisationId = user.OrganisationId;

        var userFoo = HttpContext?.User;
        var team = HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "Team");
        //userFoo.Claims.FirstOrDefault
        System.Diagnostics.Debug.WriteLine(userFoo?.Claims.ElementAt(0).Type.ToString());
        System.Diagnostics.Debug.WriteLine(team?.Value);

        await GetConnections(OrganisationId, referralOrderBy, isAssending);

    }

    public async Task OnPost(string organisationId, string? referralOrderBy, bool? isAssending, int? currentPage)
    {
        
        //Check what we get
        await GetConnections(organisationId, referralOrderBy, isAssending);
    }

    private async Task GetConnections(string organisationId, string? referralOrderBy, bool? isAssending)
    {
        if (!Enum.TryParse<ReferralOrderBy>(referralOrderBy, true, out ReferralOrderBy referralOrder))
        {
            referralOrder = ReferralOrderBy.NotSet;
        }

        SearchResults = await _referralClientService.GetRequestsForConnectionByOrganisationId(organisationId, referralOrder, isAssending, CurrentPage, PageSize);

        Pagination = new LargeSetPagination(SearchResults.TotalPages, CurrentPage);

        TotalResults = SearchResults.TotalCount;
    }
}
