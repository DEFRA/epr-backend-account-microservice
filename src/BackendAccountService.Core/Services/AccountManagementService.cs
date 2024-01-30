using System.ComponentModel.DataAnnotations;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Core.Services;

public class AccountManagementService : IAccountManagementService
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AccountManagementService> _logger;
    private readonly AccountsDbContext _accountsDbContext;

    public AccountManagementService(
        ITokenService tokenService,
        ILogger<AccountManagementService> logger,
        AccountsDbContext accountsDbContext)
    {
        _tokenService = tokenService;
        _logger = logger;
        _accountsDbContext = accountsDbContext;
    }

    public async Task<string> CreateInviteeAccountAsync(AddInviteUserRequest request)
    {
        _logger.LogInformation("Generating invite token");
        var inviteToken = _tokenService.GenerateInviteToken();

        var organisation = await _accountsDbContext
            .Organisations
            .SingleAsync(x => x.ExternalId == request.InvitedUser.OrganisationId);

        var newEnrolment = CreateEnrolmentForInvitee(request, organisation.Id, inviteToken);

        await _accountsDbContext.AddAsync(newEnrolment);

        await _accountsDbContext.SaveChangesAsync(request.InvitingUser.UserId, request.InvitedUser.OrganisationId);

        return inviteToken;
    }

    public async Task<bool> EnrolReInvitedUserAsync(User user)
    {
        var enrolments = await _accountsDbContext
            .Enrolments
            .Include(x => x.ServiceRole)
            .Include(x => x.Connection.Organisation)
            .Where(enrolment =>
                enrolment.EnrolmentStatusId == Data.DbConstants.EnrolmentStatus.Invited
                && enrolment.Connection.Person.Id == user.Person.Id)
            .ToListAsync();

        if (!enrolments.Any())
        {
            return false;
        }

        user.InviteToken = null;
        enrolments.ForEach(x => x.EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled);

        await _accountsDbContext.SaveChangesAsync(user.UserId.Value, enrolments.First().Connection.Organisation.ExternalId);
        return true;
    }

    public async Task<bool> EnrolInvitedUserAsync(User user, EnrolInvitedUserRequest request)
    {
        var enrolments = await _accountsDbContext
            .Enrolments
            .Include(x => x.ServiceRole)
            .Include(x => x.Connection.Organisation)
            .Where(enrolment =>
                enrolment.EnrolmentStatusId == Data.DbConstants.EnrolmentStatus.Invited
                && enrolment.Connection.Person.Id == user.Person.Id)
            .ToListAsync();

        if (!enrolments.Any())
        {
            return false;
        }

        user.InviteToken = null;
        user.Person.FirstName = request.FirstName;
        user.Person.LastName = request.LastName;
        user.UserId = request.UserId;

        var validationContext = new ValidationContext(user.Person, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(user.Person, validationContext, validationResults, true);
        if (!isValid)
        {
            throw new ValidationException(validationResults.First().ErrorMessage);
        }

        enrolments.ForEach(x => x.EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled);

        await _accountsDbContext.SaveChangesAsync(user.UserId.Value, enrolments.First().Connection.Organisation.ExternalId);

        return true;
    }

    public async Task<string> ReInviteUserAsync(InvitedUser invitedUser, InvitingUser invitingUser)
    {
        _logger.LogInformation("Generating re-invite token");
        var invited = _accountsDbContext.Users.Single(x => x.Email == invitedUser.Email);
        var inviteToken = _tokenService.GenerateInviteToken();
        invited.InviteToken = inviteToken;

        var invitingUserOrganisationId = _accountsDbContext.PersonOrganisationConnections.First(x =>
            x.Person.User.UserId == invitingUser.UserId).Organisation.ExternalId;

        var invitedUserOrganisationConnection = _accountsDbContext.PersonOrganisationConnections.FirstOrDefault(x =>
            x.Person.User.UserId == invited.UserId && x.Organisation.ExternalId == invitedUser.OrganisationId);

        if (invitedUserOrganisationConnection is null)
        {
            throw new ValidationException($"Invited user '{invitedUser.Email}' doesn't belong to the same organisation.");
        }

        invitedUserOrganisationConnection.PersonRoleId = invitedUser.PersonRoleId;
        var enrolment = new Enrolment
        {
            ServiceRoleId = invitedUser.ServiceRoleId,
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Invited,
            IsDeleted = false,
            Connection = invitedUserOrganisationConnection
        };

        _accountsDbContext.Add(enrolment);

        await _accountsDbContext.SaveChangesAsync(invitingUser.UserId, invitingUserOrganisationId);

        return inviteToken;
    }

    public static Enrolment CreateEnrolmentForInvitee(AddInviteUserRequest request, int organisationDatabaseId, string token)
    {
        return new Enrolment
        {
            ServiceRoleId = request.InvitedUser.ServiceRoleId,
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Invited,
            IsDeleted = false,
            Connection = new PersonOrganisationConnection
            {
                OrganisationId = organisationDatabaseId,
                OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer,
                PersonRoleId = request.InvitedUser.PersonRoleId,
                IsDeleted = false,
                Person = new Person
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = request.InvitedUser.Email,
                    Telephone = string.Empty,
                    IsDeleted = false,
                    User = new User
                    {
                        UserId = request.InvitedUser.UserId != null ? request.InvitedUser.UserId : Guid.Empty,
                        Email = request.InvitedUser.Email,
                        IsDeleted = false,
                        InviteToken = token,
                        InvitedBy = request.InvitingUser.Email
                    }
                }
            }
        };
    }
}