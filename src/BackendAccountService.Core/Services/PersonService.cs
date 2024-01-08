using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Services;

public class PersonService : IPersonService
{
    private readonly AccountsDbContext _accountsDbContext;

    public PersonService(AccountsDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }

    public async Task<PersonResponseModel?> GetPersonByUserIdAsync(Guid userId)
    {
        var person = await _accountsDbContext.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.User!.UserId == userId);

        return PersonMappings.GetPersonModelFromPerson(person);
    }
    
    public async Task<PersonResponseModel?> GetPersonByExternalIdAsync(Guid externalId)
    { 
        var person = await _accountsDbContext.Persons
          .Include(p => p.User)
          .FirstOrDefaultAsync(p => p.ExternalId == externalId);

        return PersonMappings.GetPersonModelFromPerson(person);
    }
    
    public async Task<InviteApprovedUserModel> GetPersonServiceRoleByInviteTokenAsync(string token)
    {
        try
        {
            var inviteApprovedUserModel = await (from person in _accountsDbContext.Persons
                join poc in _accountsDbContext.PersonOrganisationConnections on person.Id equals poc.PersonId
                join org in _accountsDbContext.Organisations on poc.OrganisationId equals org.Id
                join user in _accountsDbContext.Users.Where(u => token == u.InviteToken) on person.UserId equals user.Id
                join enrolment in _accountsDbContext.Enrolments.Where(e => e.EnrolmentStatus.Id == 5) on poc.Id equals enrolment.ConnectionId
                select new InviteApprovedUserModel
                {
                    ServiceRoleId = enrolment.ServiceRoleId.ToString(),
                    CompanyHouseNumber = org.CompaniesHouseNumber,
                    OrganisationId = org.ReferenceNumber,
                    Email = user.Email
                }).ToListAsync();
            

            return inviteApprovedUserModel.FirstOrDefault();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
