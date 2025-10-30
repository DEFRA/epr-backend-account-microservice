using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Core.Services;

public interface IReprocessorExporterService
{
    Task<NationDetailsResponseDto> GetNationDetailsByNationId(int nationId);
    Task<OrganisationDetailsResponseDto> GetOrganisationDetailsByOrgId(Guid organisationId); 
    Task<List<OrganisationPersonDto>> GetPersonDetailsByIds(PersonsDetailsRequestDto request);
}
