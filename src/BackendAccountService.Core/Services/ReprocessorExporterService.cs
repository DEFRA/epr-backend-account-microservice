using AutoMapper;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Repositories;
using System.Threading.Tasks;

namespace BackendAccountService.Core.Services;

public class ReprocessorExporterService(IReprocessorExporterRepository reprocessorExporterRepository,
    IMapper mapper) : IReprocessorExporterService
{
    public async Task<NationDetailsResponseDto> GetNationDetailsByNationId(int nationId)
    {
        var entity = await reprocessorExporterRepository.GetNationDetailsByNationId(nationId);
        var nationDetailsResponseDto = mapper.Map<NationDetailsResponseDto>(entity);
        return nationDetailsResponseDto;
    }

    public async Task<OrganisationDetailsResponseDto> GetOrganisationDetailsByOrgId(Guid organisationId)
    {
        var entity = await reprocessorExporterRepository.GetOrganisationDetailsByOrgId(organisationId);
        var organisationDetailsResponseDto = mapper.Map<OrganisationDetailsResponseDto>(entity);
        return organisationDetailsResponseDto;
    }

    public async Task<List<OrganisationPersonDto>> GetPersonDetailsByIds(PersonsDetailsRequestDto request)
    {
        var entity = await reprocessorExporterRepository.GetPersonDetailsByIds(request.OrgId, request.UserIds);
        var personDetailsResponseDto = mapper.Map<List<OrganisationPersonDto>>(entity);
        return personDetailsResponseDto;
    }
}