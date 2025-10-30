using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Services;

public class PersonService(AccountsDbContext accountsDbContext) : IPersonService
{
    public async Task<Person?> GetPersonByUserId(Guid userId)
    {
        return await accountsDbContext.Persons
            .Include(p => p.User)
            .Where(p => !p.IsDeleted && (p.User != null && !p.User.IsDeleted))
            .FirstOrDefaultAsync(p => p.User!.UserId == userId);
    }

    public async Task<PersonResponseModel?> GetPersonResponseByUserId(Guid userId)
    {
        var person = await GetPersonByUserId(userId);

        return PersonMappings.GetPersonModelFromPerson(person);
    }

    public async Task<PersonResponseModel?> GetAllPersonByUserIdAsync(Guid userId)
    {
        var person = await accountsDbContext.Persons
            .Include(p => p.User)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.User!.UserId == userId);

        return PersonMappings.GetPersonModelFromPerson(person);
    }

    public async Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId)
    {
        var person = await accountsDbContext.Persons
          .Include(p => p.User)
          .FirstOrDefaultAsync(p => p.ExternalId == externalId);

        return PersonMappings.GetPersonModelFromPerson(person);
    }

    public async Task<InviteApprovedUserModel> GetPersonServiceRoleByInviteTokenAsync(string token)
    {
        var inviteApprovedUserModel = await (from person in accountsDbContext.Persons
                                             join poc in accountsDbContext.PersonOrganisationConnections on person.Id equals poc.PersonId
                                             join org in accountsDbContext.Organisations on poc.OrganisationId equals org.Id
                                             join user in accountsDbContext.Users.Where(u => token == u.InviteToken) on person.UserId equals user.Id
                                             join enrolment in accountsDbContext.Enrolments.Where(e => e.EnrolmentStatus.Id == 5) on poc.Id equals enrolment.ConnectionId
                                             select new InviteApprovedUserModel
                                             {
                                                 ServiceRoleId = enrolment.ServiceRoleId.ToString(),
                                                 CompanyHouseNumber = org.CompaniesHouseNumber,
                                                 OrganisationId = org.ReferenceNumber,
                                                 Email = user.Email
                                             }).ToListAsync();


        return inviteApprovedUserModel.FirstOrDefault();
    }
}
