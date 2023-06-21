using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using System.Text;
using System.Text.Json;

namespace FamilyHubs.RequestForSupport.Core.ApiClients;

public enum ReferralStatus
{
    New = 1,
    Opened,
    Accepted,
    Declined
}

public interface IReferralClientService
{
    Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByProfessional(string professionalEmailAddress, ReferralOrderBy? orderBy, bool? isAscending, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByOrganisationId(string organisationId, ReferralOrderBy? orderBy, bool? isAscending, int pageNumber = 1, int pageSize = 10);
    Task<ReferralDto> GetReferralById(long referralId);
    Task<List<ReferralStatusDto>> GetReferralStatuses();
    Task<string> UpdateReferral(ReferralDto referralDto);
    Task<string> UpdateReferralStatus(long referralId, ReferralStatus referralStatus, string? reason = null);
}

public class ReferralClientService : ApiService, IReferralClientService
{
    public ReferralClientService(HttpClient client) : base(client)
    {
    }

    public async Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByProfessional(string professionalEmailAddress, ReferralOrderBy? orderBy, bool? isAscending, int pageNumber=1, int pageSize=10)
    {
        orderBy ??= ReferralOrderBy.NotSet;

        isAscending ??= true;

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
        orderBy ??= ReferralOrderBy.NotSet;

        isAscending ??= true;

        //todo: fix spelling in url
        var url = $"api/organisationreferrals/{organisationId}?orderBy={orderBy}&isAssending={isAscending}&pageNumber={pageNumber}&pageSize={pageSize}&includeDeclined=false";

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

    public async Task<ReferralDto> GetReferralById(long referralId)
    {
        var url = $"api/referral/{referralId}";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(Client.BaseAddress + url),
        };

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<ReferralDto>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? default!;
    }

    public async Task<List<ReferralStatusDto>> GetReferralStatuses()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(Client.BaseAddress + "api/referralstatuses"),
        };

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<ReferralStatusDto>>(await response.Content.ReadAsStreamAsync(), options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ReferralStatusDto>();
    }

    public async Task<string> UpdateReferral(ReferralDto referralDto)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(Client.BaseAddress + $"api/referrals/{referralDto.Id}"),
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(referralDto), Encoding.UTF8, "application/json"),
        };

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }

    public async Task<string> UpdateReferralStatus(long referralId, ReferralStatus referralStatus, string? reason = null)
    {
        var request = new HttpRequestMessage
        {
            //todo: this would be more restful
            //Method = HttpMethod.Put,
            //RequestUri = new Uri(Client.BaseAddress + $"api/referrals/{referralId}/status/{referralStatusId}"),
            Method = HttpMethod.Post,
            RequestUri = new Uri(Client.BaseAddress + $"api/referralStatus/{referralId}/{referralStatus}/{reason}"),
        };

        using var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stringResult = await response.Content.ReadAsStringAsync();
        return stringResult;
    }
}
