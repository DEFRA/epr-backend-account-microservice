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

        await _accountsDbContext.SaveChangesAsync(account.User.UserId!.Value,
            enrolment.Connection.Organisation.ExternalId);

        return enrolment;
    }

    public async Task<Enrolment> AddApprovedUserAccountAsync(ApprovedUserAccountModel account, ServiceRole serviceRole,
        UserModel user)
    {
        var enrolment = EnrolmentMappings.GetEnrolmentFromApprovedUserAccountModel(account, serviceRole.Id, user);
        var approvedUser = await _accountsDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == user.Id);
        var approvedUserPerson = await _accountsDbContext.Persons
            .FirstOrDefaultAsync(x => x.UserId == approvedUser.Id);
        var approvedPersonOrganisationConnection = await _accountsDbContext.PersonOrganisationConnections
            .FirstOrDefaultAsync(x => x.PersonId == approvedUserPerson.Id);
        var approvedUserEnrolment = await _accountsDbContext.Enrolments
            .FirstOrDefaultAsync(x => x.ConnectionId == approvedPersonOrganisationConnection.Id);

        //enrolments
        approvedUserEnrolment.EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Pending;
        _accountsDbContext.Update(approvedUserEnrolment);

        //connection
        approvedPersonOrganisationConnection.JobTitle = account.Connection.JobTitle;
        approvedPersonOrganisationConnection.OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer;
        approvedPersonOrganisationConnection.PersonRoleId = Data.DbConstants.PersonRole.Admin;
        _accountsDbContext.Update(approvedPersonOrganisationConnection);

        //person
        approvedUserPerson.FirstName = account.Person.FirstName;
        approvedUserPerson.LastName = account.Person.LastName;
        approvedUserPerson.Email = account.Person.ContactEmail;
        approvedUserPerson.Telephone = account.Person.TelephoneNumber;
        _accountsDbContext.Update(approvedUserPerson);

        //user
        approvedUser.ExternalIdpId = user.ExternalIdpId;
        approvedUser.ExternalIdpUserId = user.ExternalIdpUserId;
        approvedUser.UserId = account.UserId;
        approvedUser.InviteToken = null;
        _accountsDbContext.Update(approvedUser);

        await _accountsDbContext.SaveChangesAsync(approvedUserEnrolment.ServiceRole.ToString());
        return enrolment;
    }
}
