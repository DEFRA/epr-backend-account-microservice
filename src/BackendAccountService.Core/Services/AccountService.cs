using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace BackendAccountService.Core.Services;

public class AccountService(
    AccountsDbContext accountsDbContext,
    ITokenService tokenService,
    IReExEnrolmentMaps reExEnrolmentMaps
    ) : IAccountService
{
    public async Task<ServiceRole?> GetServiceRoleAsync(string serviceRoleKey)
    {
        return await accountsDbContext
            .ServiceRoles
            .FirstOrDefaultAsync(role => role.Key == serviceRoleKey);
    }

    public async Task<Enrolment> AddAccountAsync(AccountModel account, ServiceRole serviceRole)
    {
        var enrolment = EnrolmentMappings.GetEnrolmentFromAccountModel(account, serviceRole.Id);
        enrolment.Connection.Organisation.ExternalId = Guid.NewGuid();

        accountsDbContext.Add(enrolment);

        await accountsDbContext.SaveChangesAsync(account.User.UserId!.Value,
            enrolment.Connection.Organisation.ExternalId);

        return enrolment;
    }

    public async Task<Person> AddReprocessorExporterAccountAsync(
        ReprocessorExporterAccount account,
        string serviceKey,
        Guid userId)
    {
        var person = PersonMappings.GetPersonModel(account);

        accountsDbContext.Add(person);

        await accountsDbContext.SaveChangesAsync(userId, serviceKey);

        return person;
    }

    public async Task<ReExAddOrganisationResponse> AddReprocessorExporterOrganisationAsync(
        ReprocessorExporterAddOrganisation account,
        Person person,
        IImmutableDictionary<string, PartnerRole> partnerRoles,
        string serviceKey,
        Guid userId)
    {
        var adminEnrolment = reExEnrolmentMaps.GetAdminEnrolmentForCurrentUser(account, person);
        await accountsDbContext.AddAsync(adminEnrolment);

        List<ServiceRoleResponse> serviceRoles =
        [
            await GetServiceRoleResponse(
                Data.DbConstants.ServiceRole.ReprocessorExporter.AdminUser.Id)
        ];

        var reExApprovedUserServiceRole = await GetServiceRoleResponse(
            Data.DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id);

        if (account.User.IsApprovedUser)
        {
            var approvedUserEnrolment = reExEnrolmentMaps.GetApprovedPersonEnrolmentForCurrentUser(
                account, person, adminEnrolment.Connection);

            await accountsDbContext.AddAsync(approvedUserEnrolment);

            serviceRoles.Add(reExApprovedUserServiceRole);
        }

        await AddPartners(account.Partners, partnerRoles, adminEnrolment.Connection.Organisation);

        var invitedApprovedUserEnrolments = await AddInvitedApprovedUsers(
            account,
            adminEnrolment,
            reExApprovedUserServiceRole);

        await accountsDbContext.SaveChangesAsync(
            account.User.UserId,
            adminEnrolment.Connection.Organisation.ExternalId,
            serviceKey);

        // the account service shouldn't really know about the endpoint response,
        // but this keeps things simple
        return new ReExAddOrganisationResponse
        {
            UserFirstName = person!.FirstName,
            UserLastName = person.LastName,
            UserServiceRoles = serviceRoles,
            ReferenceNumber = adminEnrolment.Connection.Organisation.ReferenceNumber!,
            OrganisationId = adminEnrolment.Connection.Organisation.ExternalId,
            InvitedApprovedUsers = invitedApprovedUserEnrolments
        };
    }

    private async Task AddPartners(
        IEnumerable<PartnerModel> partners,
        IImmutableDictionary<string, PartnerRole> partnerRoles,
        Organisation organisation)
    {
        foreach (var partner in partners)
        {
            await accountsDbContext.AddAsync(new OrganisationToPartnerRole
            {
                Name = partner.Name,
                Organisation = organisation,
                PartnerRoleId = partnerRoles[partner.PartnerRole].Id
            });
        }
    }

    private async Task<List<InvitedApprovedUserResponse>> AddInvitedApprovedUsers(
        ReprocessorExporterAddOrganisation account,
        Enrolment enrolment,
        ServiceRoleResponse reExApprovedUserServiceRole)
    {
        var invitedApprovedUsers = new List<InvitedApprovedUserResponse>();
        foreach (var approvedUser in account.InvitedApprovedUsers)
        {
            invitedApprovedUsers.Add(
                await AddInvitedApprovedUser(enrolment, approvedUser, reExApprovedUserServiceRole));
        }

        return invitedApprovedUsers;
    }

    private async Task<ServiceRoleResponse> GetServiceRoleResponse(int serviceRoleId)
    {
        var reExApprovedUserServiceRole = await accountsDbContext.ServiceRoles
            .Where(r => r.Id == serviceRoleId)
            .FirstOrDefaultAsync();

        if (reExApprovedUserServiceRole == null)
        {
            throw new InvalidOperationException(
                $"Service role with ID {serviceRoleId} not found.");
        }
        
        return new ServiceRoleResponse
        {
            Key = reExApprovedUserServiceRole.Key,
            Name = reExApprovedUserServiceRole.Name,
            Description = reExApprovedUserServiceRole.Description
        };
    }

    private async Task<InvitedApprovedUserResponse> AddInvitedApprovedUser(
        Enrolment enrolment,
        InvitedApprovedUserModel invitedApprovedUser,
        ServiceRoleResponse reExApprovedUserServiceRole)
    {
        // look for an existing person for the invited user else create a new one.
        // if we find an existing user, we leave their existing details alone (KISS),
        // we could alternatively update their details, or check for differences and fail if different
        Person personToInvite = await FindPersonByEmail(invitedApprovedUser.Person.ContactEmail) ??
                                CreatePersonForInvitee(invitedApprovedUser);

        var invitedUserEnrolment = reExEnrolmentMaps.GetEnrolmentForInvitedApprovedUser(
                enrolment.Connection.Organisation,
                invitedApprovedUser,
                enrolment.Connection.Person.Email,
                personToInvite);

        await accountsDbContext.AddAsync(invitedUserEnrolment);

        var invite = CreatePersonOrganisationConnectionInvite(enrolment, invitedUserEnrolment);
        
        await accountsDbContext.AddAsync(invite);
        
        return new InvitedApprovedUserResponse
        {
            Email = invitedUserEnrolment.Connection.Person.Email,
            InviteToken = invite.InviteToken,
            ServiceRole = reExApprovedUserServiceRole
        };
    }

    private PersonOrganisationConnectionInvite CreatePersonOrganisationConnectionInvite(
        Enrolment enrolment,
        Enrolment invitedUserEnrolment)
    {
        return new PersonOrganisationConnectionInvite
        {
            // the invitee
            Person = invitedUserEnrolment.Connection.Person,
            Organisation = invitedUserEnrolment.Connection.Organisation,
            // the inviter
            User = enrolment.Connection.Person.User!,
            InviteToken = tokenService.GenerateInviteToken(),
            ServiceId = Data.DbConstants.Service.ReprocessorExporter
        };
    }

    private static Person CreatePersonForInvitee(InvitedApprovedUserModel invitedApprovedUser)
    {
        return new Person
        {
            Email = invitedApprovedUser.Person.ContactEmail,
            FirstName = invitedApprovedUser.Person.FirstName,
            LastName = invitedApprovedUser.Person.LastName,
            Telephone = invitedApprovedUser.Person.TelephoneNumber,
            User = new User
            {
                UserId = Guid.Empty,
                Email = invitedApprovedUser.Person.ContactEmail
            }
        };
    }

    private async Task<Person?> FindPersonByEmail(string email)
    {
        return await accountsDbContext.Persons
            .Where(p => p.Email == email)
            .Include(p => p.User)
            .FirstOrDefaultAsync();
    }

    public async Task AddApprovedUserAccountAsync(ApprovedUserAccountModel account, ServiceRole serviceRole,
        UserModel user)
    {
        var approvedUser = await accountsDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == user.Id);
        var approvedUserPerson = await accountsDbContext.Persons
            .FirstOrDefaultAsync(x => x.UserId == approvedUser.Id);
        //to-do: this assumes a person can only have 1 role in an organisation
        var approvedPersonOrganisationConnection = await accountsDbContext.PersonOrganisationConnections
            .FirstOrDefaultAsync(x => x.PersonId == approvedUserPerson.Id);
        var approvedUserEnrolment = await accountsDbContext.Enrolments
            .FirstOrDefaultAsync(x => x.ConnectionId == approvedPersonOrganisationConnection.Id);

        //enrolments
        approvedUserEnrolment.EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Pending;
        accountsDbContext.Update(approvedUserEnrolment);

        //ApprovedPersonEnrolment
        var approvedPersonEnrolment = new ApprovedPersonEnrolment
        {
            NomineeDeclaration = account.DeclarationFullName,
            NomineeDeclarationTime = (DateTimeOffset)account.DeclarationTimeStamp,
            EnrolmentId = approvedUserEnrolment.Id
        };
        await accountsDbContext.AddAsync(approvedPersonEnrolment);

        //connection
        approvedPersonOrganisationConnection.JobTitle = account.Connection.JobTitle;
        approvedPersonOrganisationConnection.OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer;
        approvedPersonOrganisationConnection.PersonRoleId = Data.DbConstants.PersonRole.Admin;
        accountsDbContext.Update(approvedPersonOrganisationConnection);

        //person
        approvedUserPerson.FirstName = account.Person.FirstName;
        approvedUserPerson.LastName = account.Person.LastName;
        approvedUserPerson.Email = account.Person.ContactEmail;
        approvedUserPerson.Telephone = account.Person.TelephoneNumber;
        accountsDbContext.Update(approvedUserPerson);

        //user
        approvedUser.ExternalIdpId = user.ExternalIdpId;
        approvedUser.ExternalIdpUserId = user.ExternalIdpUserId;
        approvedUser.UserId = account.UserId;
        approvedUser.InviteToken = null;
        accountsDbContext.Update(approvedUser);

        await accountsDbContext.SaveChangesAsync(approvedUserEnrolment.ServiceRoleId.ToString());
    }
}
