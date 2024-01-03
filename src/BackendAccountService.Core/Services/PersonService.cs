using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Responses;
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
}
