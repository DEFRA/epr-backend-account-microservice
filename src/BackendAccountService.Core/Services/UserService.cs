using System;
using System.Net;
using System.Text.Json;
using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Core.Services;

public class UserService : IUserService
{
	private readonly AccountsDbContext _accountsDbContext;
	private readonly ILogger<UserService> _logger;
	private readonly IValidationService _validationService;

	public UserService(AccountsDbContext accountsDbContext, ILogger<UserService> logger, IValidationService validationService)
	{
		_accountsDbContext = accountsDbContext;
		_validationService = validationService;
		_logger = logger;
	}

	public async Task<User?> GetUserByInviteAsync(string email, string inviteToken)
	{
		return await _accountsDbContext
			.Users
			.Include(user => user.Person)
			.FirstOrDefaultAsync(user => user.Email == email && user.InviteToken == inviteToken);
	}

	public async Task<User?> GetUserByUserId(Guid userId)
	{
		return await _accountsDbContext
			.Users
			.Include(user => user.Person)
			.FirstOrDefaultAsync(user => user.UserId == userId);
	}

	public async Task<Result<UserOrganisationsListModel>> GetUserOrganisationAsync(Guid userId, bool includeDeleted = false)
	{
		try
		{
			var details = new UserOrganisationsListModel();
			Person person;

			if (includeDeleted)
			{
				person = await _accountsDbContext.Persons
					.Include(p => p.User)
					.AsNoTracking()
					.IgnoreQueryFilters()
					.FirstOrDefaultAsync(p => p.User.UserId == userId);
			}
			else
			{
				person = _accountsDbContext.Persons.Include(user => user.User).FirstOrDefault(person => person.User.UserId == userId && !person.IsDeleted);
			}

			if (person is null)
			{
				return Result<UserOrganisationsListModel>.FailedResult($"No user found with the user id {userId}", HttpStatusCode.NotFound);
			}

			var userOrganisationModel = new UserDetailsModel
			{
				Id = person.User.UserId,
				FirstName = person.FirstName,
				LastName = person.LastName,
				Email = person.Email,
				Telephone = person.Telephone,
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

			// add the job title
			userOrganisationModel.JobTitle = personOrganisationConnections[0].JobTitle;

			if (enrolment != null)
			{
				userOrganisationModel.EnrolmentStatus = enrolment.EnrolmentStatus.Name;
				userOrganisationModel.ServiceRole = enrolment.ServiceRole.Name;
				userOrganisationModel.Service = enrolment.ServiceRole.Service.Name;
				userOrganisationModel.ServiceRoleId = enrolment.ServiceRoleId;
			}

			userOrganisationModel.IsChangeRequestPending = await _accountsDbContext.ChangeHistory
				.AsNoTracking()
				.Where(c => c.Person.User.UserId == userId &&
							c.OrganisationId == personOrganisationConnections[0].OrganisationId
							&& c.IsActive && c.DecisionDate == null && !c.IsDeleted).AnyAsync();

			details.User = userOrganisationModel;

			return Result<UserOrganisationsListModel>.SuccessResult(details);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Fetching list of organisations failed for the user {UserId}", userId);
			return Result<UserOrganisationsListModel>.FailedResult($"Error fetching list of organisations for the user {userId}", HttpStatusCode.InternalServerError);
		}
	}

    public async Task<Result<IList<string>>> GetUserOrganisationIdListAsync(Guid userId)
    {
        var referenceNumbersQuery =
            from p in _accountsDbContext.Persons.AsNoTracking()
            join poc in _accountsDbContext.PersonOrganisationConnections.AsNoTracking()
            on p.Id equals poc.PersonId
            join o in _accountsDbContext.Organisations.AsNoTracking()
            on poc.OrganisationId equals o.Id
            where p.User.UserId == userId && !p.IsDeleted && poc != null && !poc.IsDeleted && o != null && !o.IsDeleted
            select o.ReferenceNumber;

        var referenceNumbers = await referenceNumbersQuery.ToListAsync();
        return Result<IList<string>>.SuccessResult(referenceNumbers);
    }

	public async Task<Result<UserOrganisation>> GetSystemUserAndOrganisationAsync(string appUser)
	{
		var systemUser = await _accountsDbContext
			.Users
			.Include(user => user.Person)
			.ThenInclude(person => person.OrganisationConnections)
			.ThenInclude(x => x.Organisation)
			.Where(x => x.Email == appUser)
			.Select(s => new UserOrganisation
			{
				UserId = s.UserId,
				OrganisationId = s.Person.OrganisationConnections.Count > 0 ? s.Person.OrganisationConnections.First().Organisation.ExternalId : null
			})
			.FirstOrDefaultAsync();

		if (systemUser == null)
		{
			return Result<UserOrganisation>.FailedResult($"Error fetching system user for {appUser}", HttpStatusCode.NotFound);
		}

		return Result<UserOrganisation>.SuccessResult(systemUser);
	}

	public async Task<Result<UserOrganisationsListModel>> GetUserOrganisationsWithEnrolmentsAsync(Guid userId, string serviceKey)
	{
		var person = await _accountsDbContext.Persons
			.AsNoTracking()
			.Include(p => p.User)
			.Where(p => p.User.UserId == userId && !p.IsDeleted)
			.FirstOrDefaultAsync();

		if (person == null)
		{
			return Result<UserOrganisationsListModel>.FailedResult("User not found.", HttpStatusCode.NoContent);
		}

		var connections = await _accountsDbContext.PersonOrganisationConnections
			.AsNoTracking()
			.Include(conn => conn.Organisation)
				.ThenInclude(org => org.OrganisationType)
			.Include(conn => conn.OrganisationRole)
			.Include(conn => conn.Enrolments)
				.ThenInclude(enr => enr.ServiceRole)
					.ThenInclude(sr => sr.Service)
			.Include(conn => conn.Enrolments)
				.ThenInclude(enr => enr.EnrolmentStatus)
			.Where(conn => conn.PersonId == person.Id && !conn.IsDeleted)
			.ToListAsync();

		var organisations = connections
			.Select(conn =>
			{
				var serviceRoles = conn.Enrolments
					.Where(e => !e.IsDeleted &&
								(string.IsNullOrWhiteSpace(serviceKey) || e.ServiceRole.Service.Key == serviceKey))
					.Select(e => new EnrolmentModel
					{
						EnrolmentId = e.Id,
						EnrolmentStatus = e.EnrolmentStatus?.Name,
						ServiceRoleId = e.ServiceRoleId,
						ServiceRole = e.ServiceRole?.Name,
						ServiceRoleKey = e.ServiceRole?.Key,
						Service = e.ServiceRole?.Service?.Name,
						ServiceKey = e.ServiceRole?.Service?.Key
					}).ToList();

				return new OrganisationDetailModel
				{
					Id = conn.Organisation.ExternalId,
					Name = conn.Organisation.Name,
					TradingName = conn.Organisation.TradingName,
					OrganisationRole = GetOrganisationServiceRole(serviceKey, conn),
					OrganisationType = conn.Organisation.OrganisationType?.Name,
					OrganisationNumber = conn.Organisation.ReferenceNumber,
					CompaniesHouseNumber = conn.Organisation.CompaniesHouseNumber,
					NationId = conn.Organisation.NationId,
					OrganisationAddress = $"{conn.Organisation.Street}, {conn.Organisation.Town}, {conn.Organisation.Postcode}",
					JobTitle = conn.JobTitle,
					SubBuildingName = conn.Organisation.SubBuildingName,
					BuildingName = conn.Organisation.BuildingName,
					BuildingNumber = conn.Organisation.BuildingNumber,
					Street = conn.Organisation.Street,
					Locality = conn.Organisation.Locality,
					DependentLocality = conn.Organisation.DependentLocality,
					Town = conn.Organisation.Town,
					County = conn.Organisation.County,
					Country = conn.Organisation.Country,
					Postcode = conn.Organisation.Postcode,
					JoinerDate = conn.CreatedOn,
					PersonRoleInOrganisation = GetPersonInOrganisationRoleName(conn),
					IsChangeRequestPending = GetChangeHistoryStatus(userId, conn),
					Enrolments = serviceRoles
				};
			})
			.Where(org => org.Enrolments.Count > 0)
			.ToList();

		var firstOrganisation = organisations.FirstOrDefault();
		var firstEnrolment = firstOrganisation?.Enrolments[0];

		var model = new UserOrganisationsListModel
		{
			User = new UserDetailsModel
			{
				Id = userId,
				Email = person.Email,
				FirstName = person.FirstName,
				LastName = person.LastName,
				Telephone = person.Telephone,
				NumberOfOrganisations = organisations.Count,
				Organisations = organisations,

				// The below assignment is to avoid any issue for the services which access first org and its first enrolment from userdata level
				JobTitle = firstOrganisation?.JobTitle,
				IsChangeRequestPending = firstOrganisation?.IsChangeRequestPending ?? false,
				RoleInOrganisation = firstOrganisation?.PersonRoleInOrganisation,
				EnrolmentStatus = firstEnrolment?.EnrolmentStatus,
				ServiceRoleId = firstEnrolment?.ServiceRoleId ?? 0,
				ServiceRole = firstEnrolment?.ServiceRole,
				Service = firstEnrolment?.Service
			}
		};

		return Result<UserOrganisationsListModel>.SuccessResult(model);
	}

	private static string GetOrganisationServiceRole(string serviceKey, PersonOrganisationConnection conn)
	{
		if (serviceKey == ServiceKeys.Packaging)
		{
			return conn.Organisation.IsComplianceScheme ? OrganisationRoles.ComplianceScheme : OrganisationRoles.Producer;
		}

		if (serviceKey == ServiceKeys.ReprocessorExporter)
		{
			return OrganisationRoles.ReprocessorExporter;
		}

		return string.Empty;
	}

	private string GetPersonInOrganisationRoleName(PersonOrganisationConnection conn)
	{
		return _accountsDbContext.PersonInOrganisationRoles
						.FirstOrDefault(role => role.Id == conn.PersonRoleId)?.Name;
	}

	private bool GetChangeHistoryStatus(Guid userId, PersonOrganisationConnection conn)
	{
		return _accountsDbContext.ChangeHistory
								.AsNoTracking()
								.Where(c => c.Person.User.UserId == userId &&
											c.OrganisationId == conn.OrganisationId &&
											c.IsActive &&
											c.DecisionDate == null &&
											!c.IsDeleted)
								.Any();
	}

	private PersonOrganisationConnection[] GetPersonOrganisationConnectionAsync(int personId)
	{
		return _accountsDbContext.PersonOrganisationConnections
			.Include(org => org.Organisation)
			.Include(type => type.Organisation.OrganisationType)
			.Include(type => type.Organisation.ProducerType)
			.Where(con => con.PersonId == personId && !con.Organisation.IsDeleted).ToArray();
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
		return personOrganisationConnections.Select(po =>
			new OrganisationDetailModel
			{
				Id = po.Organisation.ExternalId,
				Name = po.Organisation.Name,
				TradingName = po.Organisation.TradingName,
				OrganisationRole = po.Organisation.IsComplianceScheme
					? "Compliance Scheme"
					: "Producer",
				OrganisationType = po.Organisation.OrganisationType.Name,
				ProducerType = po.Organisation.ProducerType?.Name,
				OrganisationNumber = po.Organisation.ReferenceNumber,
				CompaniesHouseNumber = po.Organisation.CompaniesHouseNumber,
				NationId = po.Organisation.NationId,
				SubBuildingName = po.Organisation.SubBuildingName,
				BuildingName = po.Organisation.BuildingName,
				BuildingNumber = po.Organisation.BuildingNumber,
				Street = po.Organisation.Street,
				Locality = po.Organisation.Locality,
				DependentLocality = po.Organisation.DependentLocality,
				Town = po.Organisation.Town,
				County = po.Organisation.County,
				Country = po.Organisation.Country,
				Postcode = po.Organisation.Postcode
			}
		).ToList();
	}

	public async Task<Result<PersonWithOrganisationsResponse?>> GetPersonOrganisationsWithEnrolmentsForServiceAsync(Guid userId, string serviceKey)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(serviceKey) || userId == Guid.Empty)
			{
				return null;
			}

			var personOrganisationResponse = await GetPersonsWithOrganisationsResponse(userId, serviceKey);

			if (personOrganisationResponse != null)
			{
				_logger.LogInformation("User details fetched successfully for the user {UserId}", userId);
				return Result<PersonWithOrganisationsResponse>.SuccessResult(personOrganisationResponse);
			}

			_logger.LogError("No user details found for user {UserId}", userId);
			return Result<PersonWithOrganisationsResponse>.FailedResult($"No user details found for user {userId}", HttpStatusCode.NotFound);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching user details for the user {UserId}", userId);
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
		var user = await _accountsDbContext.Users.Where(p => p.Email == email).FirstOrDefaultAsync();
		return new UserModel
		{
			Id = user.Id,
			UserId = user.UserId,
			ExternalIdpId = user.ExternalIdpId,
			ExternalIdpUserId = user.ExternalIdpUserId,
			Email = user.Email
		};
	}

	/// <inheritdoc />
	public Task<string[]> DoAnyUsersExist(IEnumerable<string> userEmails)
	{
		// we can forgive selecting the nullable Email, as we only filter for non-null userEmails
		return _accountsDbContext.Users
			.Where(u => userEmails.Contains(u.Email) && !u.IsDeleted)
			.Select(u => u.Email)
			.ToArrayAsync()!;
	}

	public async Task<bool> InvitationTokenExists(string inviteToken)
	{
		return await _accountsDbContext.Users.AnyAsync(u => u.InviteToken == inviteToken);
	}

	public async Task<Result<UpdateUserDetailsResponse>> UpdateUserDetailsRequest(Guid userId, Guid organisationExternalId, string serviceKey, UpdateUserDetailsRequest updateUserDetails)
	{
		try
		{
			var poc = await _accountsDbContext.PersonOrganisationConnections.AsNoTracking()
			   .Where(conn => conn.Organisation.ExternalId == organisationExternalId)
			   .Include(p => p.Person)
			   .SingleOrDefaultAsync(pp => pp.Person.User.UserId == userId);
			if (poc == null)
			{
				var message = $"person or their organisation connection not found to update user details for user id {userId} and organisation ExternalId {organisationExternalId}.";
				return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.NotFound);
			}
			var isApprovedOrDelegatedUserInEprPackaging = await _validationService.IsApprovedOrDelegatedUserInEprPackaging(userId: userId, organisationId: organisationExternalId, serviceKey: serviceKey);
			var isBasicUserInEprPackaging = await _validationService.IsBasicUserInEprPackaging(userId: userId, organisationId: organisationExternalId, serviceKey: serviceKey);

			if ((isBasicUserInEprPackaging || isApprovedOrDelegatedUserInEprPackaging)
				 && (poc.Person.FirstName == updateUserDetails.FirstName
				 && poc.Person.LastName == updateUserDetails.LastName
				 && poc.JobTitle == updateUserDetails.JobTitle
				 && poc.Person.Telephone != updateUserDetails.Telephone))
			{
				return await UpdatePersonTelephone(userId, organisationExternalId, updateUserDetails.Telephone);
			}
			else if (isBasicUserInEprPackaging)
			{
				return await UpdateBasicUserDetails(userId, organisationExternalId, updateUserDetails);
			}
			else if (isApprovedOrDelegatedUserInEprPackaging)
			{
				return await CreateChangeHistoryForRegulatorApproval(userId, organisationExternalId, updateUserDetails);
			}
			else
			{
				return Result<UpdateUserDetailsResponse>.FailedResult("not valid user to update personal details", HttpStatusCode.Forbidden);
			}
		}
		catch (Exception ex)
		{
			var message = $"failed to update user details change request for user id {userId} and organisation ExternalId {organisationExternalId}";
			_logger.LogError(ex, "failed to update user details change request for user id {UserId} and organisation ExternalId {OrganisationExternalId}", userId, organisationExternalId);
			return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.InternalServerError);
		}
	}

	private async Task<Result<UpdateUserDetailsResponse>> UpdatePersonTelephone(Guid userId, Guid organisationExternalId, string telephone)
	{
		var response = new UpdateUserDetailsResponse();
		try
		{
			var person = await _accountsDbContext.Persons.SingleOrDefaultAsync(person => person.User.UserId == userId && !person.IsDeleted);
			if (person == null)
			{
				var message = $"Person not found to update telephone details for user id {userId}.";
				return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.NotFound);
			}
			person.Telephone = telephone ?? string.Empty;
			await _accountsDbContext.SaveChangesAsync(userId, organisationExternalId);
			response.HasTelephoneOnlyUpdated = true;
			return Result<UpdateUserDetailsResponse>.SuccessResult(response);
		}
		catch (Exception ex)
		{
			var message = $"Error updating telephone number only for user id {userId} and organisation ExternalId {organisationExternalId}";
			_logger.LogError(ex, "Error updating telephone number only for user id {UserId} and organisation ExternalId {OrganisationExternalId}", userId, organisationExternalId);
			return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.InternalServerError);
		}
	}

	private async Task<Result<UpdateUserDetailsResponse>> UpdateBasicUserDetails(Guid userId, Guid organisationExternalId, UpdateUserDetailsRequest updateUserDetails)
	{
		var response = new UpdateUserDetailsResponse();
		try
		{
			var person = await _accountsDbContext.Persons.SingleOrDefaultAsync(person => person.User.UserId == userId && !person.IsDeleted);
			var poc = await _accountsDbContext.PersonOrganisationConnections
				.Where(con => con.PersonId == person.Id
				&& con.Organisation.ExternalId == organisationExternalId
				&& !con.Organisation.IsDeleted).SingleOrDefaultAsync();

			if (person == null || poc == null)
			{
				var message = $"Person or their Organisation connection not found to update user details for user id {userId} and organisation ExternalId {organisationExternalId}.";
				return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.NotFound);
			}

			person.FirstName = updateUserDetails.FirstName ?? string.Empty;
			person.LastName = updateUserDetails.LastName ?? string.Empty;
			person.Telephone = updateUserDetails.Telephone ?? string.Empty;
			poc.JobTitle = updateUserDetails.JobTitle ?? null;

			await _accountsDbContext.SaveChangesAsync(userId, organisationExternalId);
			response.HasBasicUserDetailsUpdated = true;
			return Result<UpdateUserDetailsResponse>.SuccessResult(response);
		}
		catch (Exception ex)
		{
			var message = $"Error updating telephone number only for user id {userId} and organisation ExternalId {organisationExternalId}";
			_logger.LogError(ex, "Error updating telephone number only for user id {UserId} and organisation ExternalId {OrganisationExternalId}", userId, organisationExternalId);
			return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.InternalServerError);
		}
	}

	private async Task<Result<UpdateUserDetailsResponse>> CreateChangeHistoryForRegulatorApproval(Guid userId, Guid organisationExternalId, UpdateUserDetailsRequest updateUserDetails)
	{
		var response = new UpdateUserDetailsResponse();
		try
		{
			var ischangeHistoryExists = await _accountsDbContext.ChangeHistory.AsNoTracking()
			  .Where(c => c.Person.User.UserId == userId && c.Organisation.ExternalId == organisationExternalId && c.IsActive && c.DecisionDate == null && !c.IsDeleted).AnyAsync();

			if (ischangeHistoryExists)
			{
				var message = $"update user details approval change request for user id  {userId} and organisation ExternalId {organisationExternalId} already exisits.";
				return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.BadRequest);
			}

			var person = await _accountsDbContext.Persons.SingleOrDefaultAsync(person => person.User.UserId == userId && !person.IsDeleted);
			var poc = await _accountsDbContext.PersonOrganisationConnections.AsNoTracking().Include(o => o.Organisation).ThenInclude(n => n.Nation)
				.Include(o => o.Organisation).ThenInclude(n => n.OrganisationType)
				.Include(o => o.Enrolments).ThenInclude(s => s.ServiceRole)
				.Where(con => con.PersonId == person.Id
				&& con.Organisation.ExternalId == organisationExternalId
				&& !con.Organisation.IsDeleted).SingleOrDefaultAsync();

			if (person == null || poc == null)
			{
				var message = $"Person or their Organisation connection not found to update user details for user id {userId} and organisation ExternalId {organisationExternalId}.";
				return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.NotFound);
			}

			var OldValues = JsonSerializer.Serialize<UserDetailsChangeModel>(new UserDetailsChangeModel
			{
				FirstName = person.FirstName,
				LastName = person.LastName,
				JobTitle = poc.JobTitle
			});
			var NewValues = JsonSerializer.Serialize<UserDetailsChangeModel>(new UserDetailsChangeModel
			{
				FirstName = updateUserDetails.FirstName,
				LastName = updateUserDetails.LastName,
				JobTitle = updateUserDetails.JobTitle
			});

			var changeHistory = new ChangeHistory()
			{
				IsActive = true,
				IsDeleted = false,
				PersonId = person.Id,
				OrganisationId = poc.OrganisationId,
				DeclarationDate = DateTime.UtcNow,
				OldValues = OldValues,
				NewValues = NewValues
			};

			if (person.Telephone != updateUserDetails.Telephone)
			{
				person.Telephone = updateUserDetails.Telephone ?? string.Empty;
			}

			var existingActiveRequests = await _accountsDbContext.ChangeHistory
				.Where(c => c.Person.User.UserId == userId
				&& c.Organisation.ExternalId == organisationExternalId
				&& c.IsActive
				&& c.DecisionDate != null && !c.IsDeleted)
				.Select(x => x).ToListAsync();

			if (existingActiveRequests.Count > 0)
			{
				existingActiveRequests.ForEach(x => x.IsActive = false);
			}

			await _accountsDbContext.ChangeHistory.AddAsync(changeHistory);
			await _accountsDbContext.SaveChangesAsync(userId, organisationExternalId);
			response.HasApprovedOrDelegatedUserDetailsSentForApproval = true;
			response.ChangeHistory = ChangeHistoryMappings.GetChangeHistoryModelFromChangeHistory(changeHistory, poc, person);
			return Result<UpdateUserDetailsResponse>.SuccessResult(response);
		}
		catch (Exception ex)
		{
			var message = $"Error updating telephone number only for user id {userId} and organisation ExternalId {organisationExternalId}";
			_logger.LogError(ex, "Error updating telephone number only for user id {UserId} and organisation ExternalId {OrganisationExternalId}", userId, organisationExternalId);
			return Result<UpdateUserDetailsResponse>.FailedResult(message, HttpStatusCode.InternalServerError);
		}
	}

	public async Task<Guid?> GetUserIdByPersonId(Guid personId)
	{
        return await _accountsDbContext.Persons
            .Include(p => p.User)
            .Where(p => p.ExternalId == personId && !p.IsDeleted && (p.User != null && !p.User.IsDeleted))
			.Select(u => u.User.UserId)
			.SingleOrDefaultAsync();
    }
}