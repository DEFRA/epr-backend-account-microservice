using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Services;

public class AccountService : IAccountService
{
    private readonly AccountsDbContext _accountsDbContext;

    public AccountService(AccountsDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }

    public async Task<ServiceRole?> GetServiceRoleAsync(string serviceRoleKey)
    {
        return await _accountsDbContext
            .ServiceRoles
            .FirstOrDefaultAsync(role => role.Key == serviceRoleKey);
    }
    
    public async Task<Enrolment> AddAccountAsync(AccountModel account, ServiceRole serviceRole)
    {
        var enrolment = EnrolmentMappings.GetEnrolmentFromAccountModel(account, serviceRole.Id);
        enrolment.Connection.Organisation.ExternalId = Guid.NewGuid();

        _accountsDbContext.Add(enrolment);

        await _accountsDbContext.SaveChangesAsync(account.User.UserId!.Value, enrolment.Connection.Organisation.ExternalId);

        return enrolment;
    }
}
