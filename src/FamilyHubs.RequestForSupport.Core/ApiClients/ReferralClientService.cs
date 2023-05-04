using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using System.Text.Json;

namespace FamilyHubs.RequestForSupport.Core.ApiClients;

public interface IReferralClientService
{
    Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByProfessional(string professionalEmailAddress, int pageNumber = 1, int pageSize = 10, string? searchText = null);
}

public class ReferralClientService : ApiService, IReferralClientService
{
    public ReferralClientService(HttpClient client) : base(client)
    {
    }

    public async Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByProfessional(string professionalEmailAddress, int pageNumber=1, int pageSize=10, string? searchText = null)
    {
        var url = $"api/referrals/{professionalEmailAddress}?pageNumber={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrEmpty(searchText)) 
        {
            url += $"&searchText={searchText}";
            
        }
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
