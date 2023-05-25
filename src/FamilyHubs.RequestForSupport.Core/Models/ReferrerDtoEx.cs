using FamilyHubs.ReferralService.Shared.Dto;

namespace FamilyHubs.RequestForSupport.Core.Models
{
    public record ReferrerDtoEx : ReferrerDto
    {
        public bool ShowTeam { get; set; }
        public static ReferrerDtoEx CreateReferrerDtoEx(ReferrerDto referrerDto, bool showTeam)
        {
            return new ReferrerDtoEx
            {
                Id = referrerDto.Id,
                EmailAddress = referrerDto.EmailAddress,
                Name = referrerDto.Name,
                PhoneNumber = referrerDto.PhoneNumber,
                Role = referrerDto.Role,
                Team = referrerDto.Team,
                ShowTeam = showTeam
            };
        }
    }
}
