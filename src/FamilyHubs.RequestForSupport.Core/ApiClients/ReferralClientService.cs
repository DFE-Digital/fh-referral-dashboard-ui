using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using System.Text.Json;

namespace FamilyHubs.RequestForSupport.Core.ApiClients;

//todo: construct Uri's better
public class ReferralClientService : ApiService, IReferralClientService
{
    public ReferralClientService(HttpClient client) : base(client)
    {
    }

    private async Task<PaginatedList<ReferralDto>> GetRequests(
        string urlPath,
        ReferralOrderBy? orderBy,
        bool? isAscending,
        int pageNumber = 1,
        int pageSize = 10,
        bool includeDeclined = true,
        CancellationToken cancellationToken = default)
        {
            orderBy ??= ReferralOrderBy.NotSet;

            isAscending ??= true;

            //todo: fix spelling in url
            var url = $"{urlPath}?orderBy={orderBy}&isAssending={isAscending}&pageNumber={pageNumber}&pageSize={pageSize}&includeDeclined={includeDeclined}";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Client.BaseAddress + url),
            };

            using var response = await Client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ReferralClientServiceException(response, await response.Content.ReadAsStringAsync(cancellationToken));
            }

            var referrals = await JsonSerializer.DeserializeAsync<PaginatedList<ReferralDto>>(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            if (referrals is null)
            {
                // the only time it'll be null, is if the API returns "null"
                // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
                // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
                // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
                throw new ReferralClientServiceException(response, "null");
            }

            return referrals;
        }

    public Task<PaginatedList<ReferralDto>> GetRequestsByLaProfessional(
        string accountId,
        ReferralOrderBy? orderBy,
        bool? isAscending,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return GetRequests($"api/referralsByReferrer/{accountId}", orderBy, isAscending, pageNumber, pageSize, true, cancellationToken);
    }

    public Task<PaginatedList<ReferralDto>> GetRequestsForConnectionByOrganisationId(
        string organisationId,
        ReferralOrderBy? orderBy,
        bool? isAscending,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return GetRequests($"api/organisationreferrals/{organisationId}",
            orderBy, isAscending, pageNumber, pageSize, false, cancellationToken);
    }

    public async Task<ReferralDto> GetReferralById(long referralId, CancellationToken cancellationToken = default)
    {
        var url = $"api/referral/{referralId}";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(Client.BaseAddress + url),
        };

        using var response = await Client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ReferralClientServiceException(response, await response.Content.ReadAsStringAsync(cancellationToken));
        }

        var referral = await JsonSerializer.DeserializeAsync<ReferralDto>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        if (referral is null)
        {
            // the only time it'll be null, is if the API returns "null"
            // (see https://stackoverflow.com/questions/71162382/why-are-the-return-types-of-nets-system-text-json-jsonserializer-deserialize-m)
            // unlikely, but possibly (pass new MemoryStream(Encoding.UTF8.GetBytes("null")) to see it actually return null)
            // note we hard-code passing "null", rather than messing about trying to rewind the stream, as this is such a corner case and we want to let the deserializer take advantage of the async stream (in the happy case)
            throw new ReferralClientServiceException(response, "null");
        }

        return referral;
        
    }

    public async Task<string> UpdateReferralStatus(long referralId, ReferralStatus referralStatus, string? reason = null)
    {
        var request = new HttpRequestMessage
        {
            //todo: this would be more restful
            //Method = HttpMethod.Put,
            //RequestUri = new Uri(Client.BaseAddress + $"api/referrals/{referralId}/status/{referralStatusId}"),
            Method = HttpMethod.Post,
            RequestUri = new Uri(Client.BaseAddress + $"api/status/{referralId}/{referralStatus}/{reason}"),
        };

        using var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new ReferralClientServiceException(response, await response.Content.ReadAsStringAsync());
        }

        return await response.Content.ReadAsStringAsync();
    }
}
