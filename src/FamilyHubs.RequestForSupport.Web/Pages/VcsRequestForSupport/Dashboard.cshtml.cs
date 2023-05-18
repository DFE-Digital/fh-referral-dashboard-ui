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
    public string? SearchText { get; set;}

    [BindProperty]
    public string ProfessionalEmailAddress { get; set; } = string.Empty;

    [BindProperty]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

    public DashboardModel(IReferralClientService referralClientService)
    {
        _referralClientService = referralClientService;
        Pagination = new DontShowPagination();
    }
    public async Task OnGet(string professional, string? searchText, int? currentPage)
    {
        ProfessionalEmailAddress = professional;
        if (currentPage != null)
            CurrentPage = currentPage.Value;

        //var context = this.PageContext.HttpContext;
        var user = HttpContext.GetFamilyHubsUser();
        System.Diagnostics.Debug.WriteLine(user.LastName);

        var userFoo = HttpContext?.User;
        var team = HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "Team");
        //userFoo.Claims.FirstOrDefault
        System.Diagnostics.Debug.WriteLine(userFoo?.Claims.ElementAt(0).Type.ToString());
        System.Diagnostics.Debug.WriteLine(team?.Value);

        await SearchConnections(searchText);

    }

    public async Task OnPost(string professional, string? searchText)
    {
        ProfessionalEmailAddress = professional;
        //Check what we get
        await SearchConnections(SearchText);
    }

    private async Task SearchConnections(string? searchText)
    {
        SearchResults = await _referralClientService.GetRequestsForConnectionByProfessional(ProfessionalEmailAddress, CurrentPage, PageSize, searchText);

        Pagination = new LargeSetPagination(SearchResults.TotalPages, CurrentPage);

        TotalResults = SearchResults.TotalCount;
    }
}
