using System.Net;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BackendAccountService.Core.Services;

public class UserService : IUserService
{
    private readonly AccountsDbContext _accountsDbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AccountsDbContext accountsDbContext, ILogger<UserService> logger)
    {
        _accountsDbContext = accountsDbContext;
        _logger = logger;
    }

    public async Task<User?> GetUserByInviteAsync(string email, string inviteToken)
    {
        return await _accountsDbContext
            .Users
            .Include(user => user.Person)
            .FirstOrDefaultAsync(user => user.Email == email && user.InviteToken == inviteToken);
    }
    
    public async Task<Result<UserOrganisationsListModel>> GetUserOrganisationAsync(Guid userId)
    {
        try
        {
            UserOrganisationsListModel details = new UserOrganisationsListModel();
            var person = _accountsDbContext.Persons.Include(user=>user.User).FirstOrDefault(person => person.User.UserId == userId && !person.IsDeleted);
            if (person == null)
            {
                return Result<UserOrganisationsListModel>.FailedResult($"No user found with the user id {userId}", HttpStatusCode.NotFound);
            }

            UserDetailsModel userOrganisationModel = new UserDetailsModel
            {
                Id = person.User.UserId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
            };

            var personOrganisationConnections = GetPersonOrganisationConnectionAsync(person.Id);

            if (personOrganisationConnections.Length == 0)
            {
                details.User = userOrganisationModel;
                return Result<UserOrganisationsListModel>.SuccessResult(details);
            }

            userOrganisationModel.Organisations = GetOrganisationDetails(personOrganisationConnections);

            var role = _accountsDbContext.PersonInOrganisationRoles
                .First(role => role.Id == personOrganisationConnections[0].PersonRoleId);

            if (role != null)
                userOrganisationModel.RoleInOrganisation = role.Name;

            var enrolment = await GetEnrolmentDetailsAsync(personOrganisationConnections[0].Id);

            if (enrolment != null)
            {
                userOrganisationModel.EnrolmentStatus = enrolment?.EnrolmentStatus.Name;
                userOrganisationModel.ServiceRole = enrolment?.ServiceRole.Name;
                userOrganisationModel.Service = enrolment?.ServiceRole.Service.Name;
                userOrganisationModel.ServiceRoleId = enrolment?.ServiceRoleId;
            }

            details.User = userOrganisationModel;

            return Result<UserOrganisationsListModel>.SuccessResult(details);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fetching list of organisations failed for the user {userId}", userId);
            return Result<UserOrganisationsListModel>.FailedResult($"Error fetching list of organisations for the user {userId}", HttpStatusCode.InternalServerError);
        }
    }

    private PersonOrganisationConnection[] GetPersonOrganisationConnectionAsync(int personId)
    {
        return _accountsDbContext.PersonOrganisationConnections
            .Include(org => org.Organisation)
            .Include(type => type.Organisation.OrganisationType)
            .Include(type => type.Organisation.ProducerType)
            .Where(con =>
                con.PersonId == personId && !con.Organisation.IsDeleted).ToArray();
    }

    private async Task<Enrolment> GetEnrolmentDetailsAsync(int personOrganisationConnectionId)
    {
        return await _accountsDbContext.Enrolments
            .Include(service => service.ServiceRole)
            .Include(service => service.ServiceRole.Service)
            .Include(status => status.EnrolmentStatus)
            .FirstOrDefaultAsync(en => en.ConnectionId == personOrganisationConnectionId && !en.IsDeleted);
    }

    private static List<OrganisationDetailModel> GetOrganisationDetails(PersonOrganisationConnection[] personOrganisationConnections)
    {
        var organisations = new List<OrganisationDetailModel>();

        foreach (var personOrganisationConnection in personOrganisationConnections)
        {
            organisations.Add(new OrganisationDetailModel
            {
                Id = personOrganisationConnection.Organisation.ExternalId,
                Name = personOrganisationConnection.Organisation.Name,
                OrganisationRole = personOrganisationConnection.Organisation.IsComplianceScheme
                    ? "Compliance Scheme"
                    : "Producer",
                OrganisationType = personOrganisationConnection.Organisation.OrganisationType.Name,
                ProducerType = personOrganisationConnection.Organisation.ProducerType?.Name,
                OrganisationNumber = personOrganisationConnection.Organisation.ReferenceNumber,
                NationId = personOrganisationConnection.Organisation.NationId
            });
        }
        return organisations;
    }

    public async Task<Result<PersonWithOrganisationsResponse?>> GetPersonOrganisationsWithEnrolmentsForServiceAsync(Guid userId, string serviceKey)
    {
        try
        {
            if (serviceKey.IsNullOrEmpty() || userId == Guid.Empty)
            {
                return null;
            }

            var personOrganisationResponse = await GetPersonsWithOrganisationsResponse(userId, serviceKey);

            if (personOrganisationResponse != null)
            {
                _logger.LogInformation("User details fetched successfully for the user {userId}", userId);
                return Result<PersonWithOrganisationsResponse>.SuccessResult(personOrganisationResponse);
            }

            var message = $"No user details found for user {userId}";
            _logger.LogError(message);
            return Result<PersonWithOrganisationsResponse>.FailedResult(message, HttpStatusCode.NotFound);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user details for the user {userId}", userId);

            return Result<PersonWithOrganisationsResponse>.FailedResult($"Error fetching user details for the user {userId}", HttpStatusCode.InternalServerError);
        }

    }

    private Task<PersonWithOrganisationsResponse?> GetPersonsWithOrganisationsResponse(Guid userId, string serviceKey)
    {
        return _accountsDbContext.Persons
            .Where(person => person.User.ExternalIdpUserId == userId.ToString())
            .Select(person => new PersonWithOrganisationsResponse
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                Organisations = person.OrganisationConnections
                    .Select(connection => new OrganisationWithEnrolmentsResponse
                    {
                        Id = connection.Organisation.ExternalId,
                        Name = connection.Organisation.Name,
                        Type = OrganisationTypeMappings.GetOrganisationType(connection.Organisation.OrganisationTypeId),
                        IsComplianceScheme = connection.Organisation.IsComplianceScheme,
                        PersonRole = PersonRoleMappings.MapPersonRole(connection.PersonRoleId),
                        Enrolments = connection.Enrolments
                            .Where(enrolment => enrolment.ServiceRole.Service.Key == serviceKey)
                            .Select(enrolment => new EnrolmentResponse
                            {
                                ServiceRole = enrolment.ServiceRole.Key,
                                EnrolmentStatus = EnrolmentStatusMappings.MapEnrolmentStatus(enrolment.EnrolmentStatusId)
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }
    
    
    public async Task<UserModel?> GetApprovedUserUserByEmailAsync(string email)
    {
        var user =  await _accountsDbContext.Users.Where(p => p.Email == email).FirstOrDefaultAsync();
        var userModel = new UserModel()
        {
            Id = user.Id,
            UserId = user.UserId,
            ExternalIdpId = user.ExternalIdpId,
            ExternalIdpUserId = user.ExternalIdpUserId,
            Email = user.Email
        };
        
        return userModel;
    }
}