using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using System.Text.Json;

namespace FamilyHubs.RequestForSupport.Core.ApiClients;

public interface IReferralClientService
{
    Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByProfessional(string professionalEmailAddress, ReferralOrderBy? orderBy, bool? isAscending, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByOrganisationId(string organisationId, ReferralOrderBy? orderBy, bool? isAscending, int pageNumber = 1, int pageSize = 10);
}

public class ReferralClientService : ApiService, IReferralClientService
{
    public ReferralClientService(HttpClient client) : base(client)
    {
    }

    public async Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByProfessional(string professionalEmailAddress, ReferralOrderBy? orderBy, bool? isAscending, int pageNumber=1, int pageSize=10)
    {
        if (orderBy == null)
            orderBy = ReferralOrderBy.NotSet;

        if (isAscending == null)
            isAscending = true;

        //todo: fix spelling in url
        var url = $"api/referrals/{professionalEmailAddress}?orderBy={orderBy}&isAssending={isAscending}pageNumber={pageNumber}&pageSize={pageSize}";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(Client.BaseAddress + url),
        };

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<ReferralDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new PaginatedList<ReferralDto>();
    }

    public async Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByOrganisationId(string organisationId, ReferralOrderBy? orderBy, bool? isAscending, int pageNumber = 1, int pageSize = 10)
    {
        if (orderBy == null)
            orderBy = ReferralOrderBy.NotSet;

        if (isAscending == null)
            isAscending = true;

        var url = $"api/organisationreferrals/{organisationId}?orderBy={orderBy}&isAssending={isAscending}&pageNumber={pageNumber}&pageSize={pageSize}";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(Client.BaseAddress + url),
        };

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<PaginatedList<ReferralDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new PaginatedList<ReferralDto>();
    }
}
