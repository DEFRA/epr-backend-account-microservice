using BackendAccountService.Data.Entities;

namespace BackendAccountService.Data.Repositories;

public interface IReprocessorExporterRepository
{
    Task<Nation> GetNationDetailsByNationId(int nationId);
    Task<Organisation> GetOrganisationDetailsByOrgId(Guid organisationId);
    Task<List<PersonOrganisationConnection>> GetPersonDetailsByIds(Guid? orgId, List<Guid> userIds);
}
