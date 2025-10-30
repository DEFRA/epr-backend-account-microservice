using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAccountService.Data.Repositories;

public class ReprocessorExporterRepository(AccountsDbContext accountsDbContext) : IReprocessorExporterRepository
{
    public async Task<Nation> GetNationDetailsByNationId(int nationId)
    {
        return await accountsDbContext.Nations.SingleOrDefaultAsync(t => t.Id == nationId)
                ?? throw new KeyNotFoundException("Nation not found.");
    }

    public async Task<Organisation> GetOrganisationDetailsByOrgId(Guid organisationId)
    {
        return await accountsDbContext.Organisations
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.PersonOrganisationConnections.Where(k => !k.IsDeleted && !k.Person.IsDeleted))
                  .ThenInclude(pc => pc.Person)
                     .ThenInclude(p => p.User)
                .Include(x => x.PersonOrganisationConnections.Where(k => !k.IsDeleted && !k.Person.IsDeleted))
                  .ThenInclude(pc => pc.Enrolments.Where(k => !k.IsDeleted))
                    .ThenInclude(e => e.ServiceRole)
                .Include(x => x.OrganisationType)
                .SingleOrDefaultAsync(a => a.ExternalId == organisationId && !a.IsDeleted)
                ?? throw new KeyNotFoundException("Organisation not found.");
    }

    public async Task<List<PersonOrganisationConnection>> GetPersonDetailsByIds(Guid? orgId, List<Guid> userIds)
    {
        return await accountsDbContext.PersonOrganisationConnections
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Person)
                      .ThenInclude(p => p.User)
                .Include(x => x.Organisation)
                .Include(x => x.Enrolments.Where(k => !k.IsDeleted))
                    .ThenInclude(e => e.ServiceRole)
                .Where(x => userIds.Contains(x.Person!.User!.UserId!.Value) && (orgId == null || x.Organisation.ExternalId == orgId)
                && !x.IsDeleted && !x.Person.IsDeleted && !x.Person!.User!.IsDeleted && !x.Organisation.IsDeleted)
                .ToListAsync();
    }
}
