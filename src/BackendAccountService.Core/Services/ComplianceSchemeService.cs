using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Data.Extensions;

namespace BackendAccountService.Core.Services;

using System.Net;
using Models.Responses;
using Data.Entities;
using Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Models.Request;
using Models.Result;
using InterOrganisationRole = Data.DbConstants.InterOrganisationRole;
using Extensions;
using System.Linq;

public class ComplianceSchemeService : IComplianceSchemeService
{
    private readonly AccountsDbContext _accountsDbContext;
    private readonly ILogger<ComplianceSchemeService> _logger;
    private const int MaxPageSize = 100;
    private const int MaxQueryLength = 160;

    public ComplianceSchemeService(AccountsDbContext accountsDbContext, ILogger<ComplianceSchemeService> logger)
    {
        _accountsDbContext = accountsDbContext;
        _logger = logger;
    }
    public async Task<Result<ComplianceSchemeMembershipResponse>> GetComplianceSchemeMembersAsync(Guid organisationId, Guid complianceSchemeId, string? query, int pageSize, int page)
    {
        query ??= string.Empty;

        if (query.Length > MaxQueryLength)
        {
            var message = $"Length {query.Length} of parameter 'query' exceeds max length {MaxQueryLength}";
            return Result<ComplianceSchemeMembershipResponse>.FailedResult(message, HttpStatusCode.BadRequest);
        }
        
        if(pageSize > MaxPageSize) 
        {
            pageSize = MaxPageSize;
        }

        query = query.Trim().ToLower();
        
        string? referenceNumber = query.TryExtractReferenceNumberFromQuery();
        var searchReferenceNumberNotRequired = query.Length == 0 || referenceNumber is null;
        var searchOrganisationNameNotRequired = query.Length == 0 || referenceNumber is not null;

        try
        {
            var complianceScheme = _accountsDbContext.ComplianceSchemes.FirstOrDefault(complianceScheme => complianceScheme.ExternalId == complianceSchemeId);
            if (complianceScheme == null)
            {
                _logger.LogError("Compliance scheme id {complianceSchemeId} is not found for the organisation Id {organisationId}}", complianceSchemeId, organisationId);
                return Result<ComplianceSchemeMembershipResponse>.FailedResult($"Compliance scheme id {complianceSchemeId} is not found for the organisation Id {organisationId}", HttpStatusCode.NotFound);
            }

            var isOperatorForThisScheme = await _accountsDbContext.Organisations.Where(o =>
                o.ExternalId == organisationId && o.CompaniesHouseNumber == complianceScheme.CompaniesHouseNumber).AnyAsync();
            if(!isOperatorForThisScheme)
            {
                _logger.LogError("Compliance scheme id {complianceSchemeId} is not the operator for the organisation Id {organisationId}}", complianceSchemeId, organisationId);
                return Result<ComplianceSchemeMembershipResponse>.FailedResult($"Compliance scheme id {complianceSchemeId} is not found for the organisation Id {organisationId}", HttpStatusCode.NotFound);
            }
            
            var fullProducerQuery = _accountsDbContext.SelectedSchemes
            .Where(selectedScheme =>
                selectedScheme.ComplianceSchemeId == complianceScheme.Id &&
                selectedScheme.OrganisationConnection.FromOrganisationRoleId == InterOrganisationRole.Producer &&
                selectedScheme.OrganisationConnection.ToOrganisation.ExternalId == organisationId &&
                selectedScheme.OrganisationConnection.ToOrganisationRoleId == InterOrganisationRole.ComplianceScheme);
            
            var linkedOrganisationCount = await fullProducerQuery.CountAsync();
            if (linkedOrganisationCount == 0)
            {
                var complianceSchemeMembershipEmptyResponse = new ComplianceSchemeMembershipResponse
                {
                    SchemeName = complianceScheme.Name,
                    LinkedOrganisationCount = linkedOrganisationCount
                };
                _logger.LogInformation("Compliance scheme id {complianceSchemeId} found which has 0 members for the organisation Id {organisationId}", complianceSchemeId, organisationId);
                return Result<ComplianceSchemeMembershipResponse>.SuccessResult(complianceSchemeMembershipEmptyResponse);
            }
                    
            var lastUpdatedDate = await fullProducerQuery.IgnoreQueryFilters()
                .MaxAsync(selectedScheme => selectedScheme.OrganisationConnection.LastUpdatedOn);

            var producerList = fullProducerQuery
                .Where(selectedScheme =>
                    (searchReferenceNumberNotRequired ||
                        selectedScheme.OrganisationConnection.FromOrganisation.ReferenceNumber == referenceNumber ||
                        selectedScheme.OrganisationConnection.FromOrganisation.Name.ToLower().Contains(query)) &&
                    (searchOrganisationNameNotRequired ||
                        selectedScheme.OrganisationConnection.FromOrganisation.Name.ToLower().Contains(query)))
                .OrderBy(selectedScheme => selectedScheme.OrganisationConnection.FromOrganisation.Name)
                .Select(selectedScheme => new ComplianceSchemeMemberDto
                {
                    SelectedSchemeId = selectedScheme.ExternalId,
                    OrganisationName = selectedScheme.OrganisationConnection.FromOrganisation.Name,
                    OrganisationNumber = selectedScheme.OrganisationConnection.FromOrganisation.ReferenceNumber

                });

            var pagedResults = await PaginatedResponse<ComplianceSchemeMemberDto>.CreateAsync(producerList, page, pageSize);
            var complianceSchemeMembershipResponse = new ComplianceSchemeMembershipResponse
            {
                PagedResult = pagedResults,
                SchemeName = complianceScheme.Name,
                LastUpdated = lastUpdatedDate,
                LinkedOrganisationCount = linkedOrganisationCount
            };
            
            return Result<ComplianceSchemeMembershipResponse>.SuccessResult(complianceSchemeMembershipResponse);
        }
        catch (Exception e)
        {
            var message = $"Fetching the compliance scheme members failed for the compliance scheme id {complianceSchemeId}";
            _logger.LogError(e, "Fetching the compliance scheme members failed for the compliance scheme id {complianceSchemeId}", complianceSchemeId);
            return Result<ComplianceSchemeMembershipResponse>.FailedResult(message, HttpStatusCode.InternalServerError);
        }
    }


    public async Task<IEnumerable<ComplianceSchemeDto>> GetAllComplianceSchemesAsync()
    {
        var csOperatorCompaniesHouseNumbers = _accountsDbContext.Organisations
            .Where(x => x.IsComplianceScheme)
            .Select(x => x.CompaniesHouseNumber)
            .AsNoTracking();
        
        var complianceSchemes = await _accountsDbContext
            .ComplianceSchemes
            .Where(cs => csOperatorCompaniesHouseNumbers.Contains(cs.CompaniesHouseNumber))
            .AsNoTracking()
            .Select(cs => new ComplianceSchemeDto(cs.ExternalId, cs.Name, cs.CreatedOn))
            .ToListAsync();

        return complianceSchemes;
    }

    public async Task<Result> RemoveComplianceSchemeAsync(RemoveComplianceSchemeRequest removeComplianceScheme)
    {
        try
        {
            var selectedScheme =
                _accountsDbContext.SelectedSchemes
                    .FirstOrDefault(x => x.ExternalId == removeComplianceScheme.SelectedSchemeId);

            if (selectedScheme == null)
            {
                var message = $"selected scheme {removeComplianceScheme.SelectedSchemeId} is not found";
                _logger.LogError("selected scheme {schemeId} is not found", removeComplianceScheme.SelectedSchemeId);
                return Result.FailedResult(message, HttpStatusCode.NotFound);
            }

            await RemoveOrganisationsConnectionForSchemeAsync(selectedScheme);
            await RemoveSelectedSchemeAsync(selectedScheme);
            
            await _accountsDbContext.SaveChangesAsync(removeComplianceScheme.UserOId, removeComplianceScheme.OrganisationId);
            
            return Result.SuccessResult();
        }
        catch (Exception e)
        {
            var message = $"Error removing selected scheme id {removeComplianceScheme.SelectedSchemeId}";
            _logger.LogError(e, "Error removing selected scheme id {schemeId}", removeComplianceScheme.SelectedSchemeId);
            return Result.FailedResult(message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<InfoForSelectedSchemeRemoval>> GetInfoForSelectedSchemeRemoval(Guid organisationId, Guid selectedSchemeId, Guid userId)
    {
        try
        {
            var selectedSchemeToRemove = await _accountsDbContext.SelectedSchemes
                .AsNoTracking()
                .Where(selectedScheme =>
                    selectedScheme.OrganisationConnection.ToOrganisation.ExternalId == organisationId &&
                    selectedScheme.OrganisationConnection.FromOrganisationRoleId == InterOrganisationRole.Producer &&
                    selectedScheme.OrganisationConnection.ToOrganisationRoleId == InterOrganisationRole.ComplianceScheme)
                .Where(selectedScheme => selectedScheme.ExternalId == selectedSchemeId)
                .Select(selectedScheme => new
                {
                    ComplianceSchemeName = selectedScheme.OrganisationConnection.ToOrganisation.Name,
                    ComplianceSchemeNation = NationMappings.GetNation(selectedScheme.ComplianceScheme.NationId ?? selectedScheme.OrganisationConnection.ToOrganisation.NationId).Value,
                    ProducerOrganisationId = selectedScheme.OrganisationConnection.FromOrganisationId,
                    ProducerOrganisationName = selectedScheme.OrganisationConnection.FromOrganisation.Name,
                    ProducerOrganisationNation = NationMappings.GetNation(selectedScheme.OrganisationConnection.FromOrganisation.NationId).Value,
                    ProducerReferenceNumber = selectedScheme.OrganisationConnection.FromOrganisation.ReferenceNumber
                })
                .SingleOrDefaultAsync();

            if (selectedSchemeToRemove is null)
            {
                _logger.LogError("Compliance scheme membership is not found within organisation Id {OrganisationId} and selected scheme Id {SelectedSchemeId}", organisationId, selectedSchemeId);

                var memberDetailMessage = $"Compliance scheme membership is not found within organisation Id {organisationId} and selected scheme Id {selectedSchemeId}";
                return Result<InfoForSelectedSchemeRemoval>.FailedResult(memberDetailMessage, HttpStatusCode.NotFound);
            }

            var emailRecipients = await _accountsDbContext.Enrolments
                .AsNoTracking()
                .WhereServiceRoleIn(
                    Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key,
                    Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Key)
                .WhereEnrolmentStatusIn(
                    Data.DbConstants.EnrolmentStatus.Pending,
                    Data.DbConstants.EnrolmentStatus.Approved)
                // A DelegatedPerson must have EnrolmentStatus.Approved status:
                .Where(enrolment => 
                    enrolment.ServiceRole.Key == Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key || 
                    enrolment.EnrolmentStatusId == Data.DbConstants.EnrolmentStatus.Approved)
                .Where(enrolment =>
                    enrolment.Connection.OrganisationId == selectedSchemeToRemove.ProducerOrganisationId)
                .OrderBy(enrolment => enrolment.ServiceRoleId)
                .Select(enrolment => new EmailRecipient
                {
                    Email = enrolment.Connection.Person.Email,
                    FirstName = enrolment.Connection.Person.FirstName,
                    LastName = enrolment.Connection.Person.LastName
                })
                .Distinct()
                .ToListAsync();

            var result = new InfoForSelectedSchemeRemoval()
            {
                ComplianceSchemeName = selectedSchemeToRemove.ComplianceSchemeName,
                ComplianceSchemeNation = selectedSchemeToRemove.ComplianceSchemeNation,
                OrganisationName = selectedSchemeToRemove.ProducerOrganisationName,
                OrganisationNation = selectedSchemeToRemove.ProducerOrganisationNation,
                OrganisationNumber = selectedSchemeToRemove.ProducerReferenceNumber,
                RemovalNotificationRecipients = emailRecipients
            };

            return Result<InfoForSelectedSchemeRemoval>.SuccessResult(result);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error getting info for selected scheme id {SelectedSchemeId}", selectedSchemeId);

            var message = $"Error getting info for selected scheme id {selectedSchemeId}";
            return Result<InfoForSelectedSchemeRemoval>.FailedResult(message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RemoveComplianceSchemeMemberResponse>> RemoveComplianceSchemeMember(Guid organisationId, Guid selectedSchemeId, Guid userId,
        RemoveComplianceSchemeMemberRequest request)
    {
        try
        {
            var removalReason = await _accountsDbContext.ComplianceSchemeMemberRemovalReasons
                .SingleOrDefaultAsync(reason => reason.Code == request.Code);

            if (removalReason is null)
            {
                var removalReasonMessage = $"Removal reason not found when trying to remove selected scheme id {selectedSchemeId}";
                _logger.LogError("Removal reason not found when trying to remove selected scheme id {selectedSchemeId}", selectedSchemeId);
                return Result<RemoveComplianceSchemeMemberResponse>.FailedResult(removalReasonMessage, HttpStatusCode.BadRequest);
            }

            var selectedSchemeToRemove = await _accountsDbContext.SelectedSchemes
                .Where(selectedScheme =>
                    selectedScheme.OrganisationConnection.ToOrganisation.ExternalId == organisationId &&
                    selectedScheme.OrganisationConnection.FromOrganisationRoleId == InterOrganisationRole.Producer &&
                    selectedScheme.OrganisationConnection.ToOrganisationRoleId == InterOrganisationRole.ComplianceScheme)
                .Where(selectedScheme => selectedScheme.ExternalId == selectedSchemeId)
                .Include(selectedScheme => selectedScheme.OrganisationConnection)
                .Include(selectedScheme => selectedScheme.OrganisationConnection.FromOrganisation)
                .Include(selectedScheme => selectedScheme.OrganisationConnection.ToOrganisation)
                .SingleOrDefaultAsync();

            if (selectedSchemeToRemove is null)
            {
                var memberDetailMessage = $"Compliance scheme membership is not found within organisation Id {organisationId} and selected scheme Id {selectedSchemeId}";
                _logger.LogError("Compliance scheme membership is not found within organisation Id {OrganisationId} and selected scheme Id {SelectedSchemeId}", organisationId, selectedSchemeId);
                return Result<RemoveComplianceSchemeMemberResponse>.FailedResult(memberDetailMessage, HttpStatusCode.NotFound);
            }

            selectedSchemeToRemove.IsDeleted = true;
            selectedSchemeToRemove.OrganisationConnection.IsDeleted = true;

            _accountsDbContext.Add(new ComplianceSchemeMemberRemovalAuditLogsReason
            {
                AuditLog = new ComplianceSchemeMemberRemovalAuditLog
                {
                    SchemeOrganisationId = selectedSchemeToRemove.OrganisationConnection.ToOrganisationId,
                    MemberOrganisationId = selectedSchemeToRemove.OrganisationConnection.FromOrganisationId,
                    ComplianceSchemeId = selectedSchemeToRemove.ComplianceSchemeId,
                    RemovedBy = userId,
                    RemovedOn = DateTimeOffset.Now,
                    ReasonDescription = request.TellUsMore
                },
                Reason = removalReason
            });

            var response = new RemoveComplianceSchemeMemberResponse()
            {
                OrganisationName = selectedSchemeToRemove.OrganisationConnection.FromOrganisation.Name,
            };

            await _accountsDbContext.SaveChangesAsync(userId, organisationId);

            return Result<RemoveComplianceSchemeMemberResponse>.SuccessResult(response);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error removing selected scheme id {SelectedSchemeId}", selectedSchemeId);

            var message = $"Error removing selected scheme id {selectedSchemeId}";
            return Result<RemoveComplianceSchemeMemberResponse>.FailedResult(message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<SelectedScheme>> SelectComplianceSchemeAsync(SelectComplianceSchemeRequest request)
    {
        try
        {
            var currentComplianceScheme = await GetSelectedSchemeForOrganisation(request.ProducerOrganisationId);
            if (currentComplianceScheme != null)
            {
                await RemoveOrganisationsConnectionForSchemeAsync(currentComplianceScheme);
                var selectedScheme =
                    _accountsDbContext.SelectedSchemes.Single(x => x.ExternalId == currentComplianceScheme.ExternalId);
                await RemoveSelectedSchemeAsync(selectedScheme);
            
                await _accountsDbContext.SaveChangesAsync(request.UserOId, request.ProducerOrganisationId);
            }
            var result = await SelectComplianceSchemeAsync(request.ComplianceSchemeId, request.ProducerOrganisationId);
            
            await _accountsDbContext.SaveChangesAsync(request.UserOId, request.ProducerOrganisationId);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error selecting compliance scheme");
            return Result<SelectedScheme>.FailedResult("Failed", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<SelectedScheme>> UpdateSelectedComplianceSchemeAsync(
        UpdateSelectedComplianceSchemeRequest request)
    {
        try
        {
            var selectedScheme = await GetSelectedSchemeAndOrganisationConnectionAsync(request.SelectedSchemeId);

            if (selectedScheme == null)
            {
                return Result<SelectedScheme>.FailedResult("Existing selected scheme not found",
                    HttpStatusCode.NotFound);
            }

            await RemoveOrganisationsConnectionAsync(selectedScheme.OrganisationConnection);
            await RemoveSelectedSchemeAsync(selectedScheme);
            
            await _accountsDbContext.SaveChangesAsync(request.UserOid, request.ProducerOrganisationId);

            var result = await SelectComplianceSchemeAsync(request.ComplianceSchemeId, request.ProducerOrganisationId);

            if (!result.IsSuccess)
            {
                return result;
            }
            
            await _accountsDbContext.SaveChangesAsync(request.UserOid, request.ProducerOrganisationId);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating selected compliance scheme");
            return Result<SelectedScheme>.FailedResult("Failed", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<ProducerComplianceSchemeDto>> GetComplianceSchemeForProducer(Guid organisationId)
    {
        try
        {
            var selectedScheme = await GetSelectedSchemeForOrganisation(organisationId);

            if (selectedScheme == null)
            {
                _logger.LogError("No current selected compliance scheme found for organisationId {organisationId}", organisationId);
                return Result<ProducerComplianceSchemeDto>.FailedResult("Organisation does not currently have a compliance scheme selected", HttpStatusCode.NotFound);
            }
            
            var complianceSchemeModel = new ProducerComplianceSchemeDto
            {
                SelectedSchemeId = selectedScheme.ExternalId,
                ComplianceSchemeName = selectedScheme.ComplianceScheme.Name,
                ComplianceSchemeId = selectedScheme.ComplianceScheme.ExternalId,
                ComplianceSchemeOperatorId = selectedScheme.OrganisationConnection.ToOrganisation.ExternalId,
                ComplianceSchemeOperatorName = selectedScheme.OrganisationConnection.ToOrganisation.Name
            };
            
            return Result<ProducerComplianceSchemeDto>.SuccessResult(complianceSchemeModel);
        }
        catch (Exception e)
        {
            var message = $"Fetching the selected scheme failed for the organisation id {organisationId}";
            _logger.LogError(e, "Fetching the selected scheme failed for the organisation id {organisationId}", organisationId);
            return Result<ProducerComplianceSchemeDto>.FailedResult(message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<ComplianceSchemeMemberDetailDto>> GetComplianceSchemeMemberDetailsAsync(Guid organisationId, Guid selectedSchemeId)
    {
        try
        {
            var schemeMemberDetail = _accountsDbContext.SelectedSchemes
                .Where(selectedScheme =>
                    selectedScheme.OrganisationConnection.ToOrganisation.ExternalId == organisationId &&
                    selectedScheme.OrganisationConnection.FromOrganisationRoleId == InterOrganisationRole.Producer &&
                    selectedScheme.OrganisationConnection.ToOrganisationRoleId == InterOrganisationRole.ComplianceScheme)
                .Where(selectedScheme => selectedScheme.ExternalId == selectedSchemeId)
                .Select(selectedScheme => new ComplianceSchemeMemberDetailDto
                {
                    ProducerType =   OrganisationMappings.GetProducerType(selectedScheme.OrganisationConnection.FromOrganisation.ProducerTypeId),
                    OrganisationNumber = selectedScheme.OrganisationConnection.FromOrganisation.ReferenceNumber,
                    RegisteredNation = NationMappings.GetNation(selectedScheme.OrganisationConnection.FromOrganisation.NationId.Value),
                    ComplianceScheme = selectedScheme.ComplianceScheme.Name,
                    CompanyHouseNumber = selectedScheme.OrganisationConnection.FromOrganisation.CompaniesHouseNumber,
                    OrganisationName = selectedScheme.OrganisationConnection.FromOrganisation.Name
                }).SingleOrDefault();

            if (schemeMemberDetail is not null)
            {
                return Result<ComplianceSchemeMemberDetailDto>.SuccessResult(schemeMemberDetail);
            }
            
            var memberDetailMessage = $"Compliance scheme member detail is not found is not found with organisation Id {organisationId} and selected scheme Id {selectedSchemeId}";
            _logger.LogError("Compliance scheme member detail is not found is not found with organisation Id {organisationId} and selected scheme Id {selectedSchemeId}", organisationId, selectedSchemeId);
            return Result<ComplianceSchemeMemberDetailDto>.FailedResult(memberDetailMessage, HttpStatusCode.NotFound);
        }
        catch (Exception e)
        {
            var message = $"Fetching the compliance scheme member details failed for the organisation id {organisationId} and selected scheme is {selectedSchemeId}";
            _logger.LogError(e, "Fetching the compliance scheme member details failed for the organisation id {organisationId} and scheme id {selectedSchemeId}", organisationId, selectedSchemeId);
            return Result<ComplianceSchemeMemberDetailDto>.FailedResult(message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ComplianceSchemeDto>> GetComplianceSchemesForOperatorAsync(Guid organisationId)
    {
        var organisation = await _accountsDbContext.Organisations
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.ExternalId == organisationId);
    
        var complianceSchemes = await _accountsDbContext
            .ComplianceSchemes
            .AsNoTracking()
            .Where(x => x.CompaniesHouseNumber == organisation.CompaniesHouseNumber)
            .AsNoTracking()
            .Select(cs => new ComplianceSchemeDto(cs.ExternalId, cs.Name, cs.CreatedOn))
            .ToListAsync();

        return complianceSchemes;
    }    

    public async Task<ComplianceSchemeSummary?> GetComplianceSchemeSummary(Guid organisationId, Guid complianceSchemeId)
    {
        var complianceSchemeSummary =
            await _accountsDbContext.ComplianceSchemes
                .Join(_accountsDbContext.Organisations,
                    complianceScheme => complianceScheme.CompaniesHouseNumber,
                    organisation => organisation.CompaniesHouseNumber,
                    (complianceScheme, organisation) => new {
                        OrganisationId = organisation.ExternalId,
                        ComplianceSchemeId = complianceScheme.ExternalId,
                        complianceScheme.Name,
                        complianceScheme.CreatedOn,
                        complianceScheme.NationId
                    })
                .Where(complianceScheme => 
                    complianceScheme.OrganisationId == organisationId && 
                    complianceScheme.ComplianceSchemeId == complianceSchemeId)
                .Select(complianceScheme =>                         
                    new ComplianceSchemeSummary {
                        Name = complianceScheme.Name,
                        CreatedOn = complianceScheme.CreatedOn,
                        Nation = NationMappings.GetNation(complianceScheme.NationId),
                        MemberCount = 0,
                        MembersLastUpdatedOn = null
                })
                .FirstOrDefaultAsync();

        if (complianceSchemeSummary == null)
        {
            return null;
        }
  
        var memberStats =
            await _accountsDbContext.SelectedSchemes
                .IgnoreQueryFilters()
                .Where(selectedScheme =>
                    selectedScheme.ComplianceScheme.ExternalId == complianceSchemeId &&
                    selectedScheme.OrganisationConnection.ToOrganisation.ExternalId == organisationId &&
                    selectedScheme.OrganisationConnection.FromOrganisationRoleId == InterOrganisationRole.Producer &&
                    selectedScheme.OrganisationConnection.ToOrganisationRoleId == InterOrganisationRole.ComplianceScheme)
                .Where(selectedScheme =>
                    !selectedScheme.OrganisationConnection.ToOrganisation.IsDeleted &&
                    !selectedScheme.OrganisationConnection.FromOrganisation.IsDeleted &&
                    !selectedScheme.ComplianceScheme.IsDeleted)
                .GroupBy(selectedScheme => selectedScheme.ComplianceScheme.ExternalId)
                .Select(selectedSchemes => new
                {
                    MemberCount = selectedSchemes.Count(selectedScheme => !selectedScheme.IsDeleted),
                    MembersLastUpdatedOn = selectedSchemes.Max(selectedScheme => selectedScheme.LastUpdatedOn)
                })
                .FirstOrDefaultAsync();

        complianceSchemeSummary.MemberCount = memberStats?.MemberCount ?? 0;
        complianceSchemeSummary.MembersLastUpdatedOn = memberStats?.MembersLastUpdatedOn;

        return complianceSchemeSummary;
    }

    public async Task<IEnumerable<ComplianceSchemeRemovalReasonResponse>> GetComplianceSchemeReasonsForRemovalAsync()
    {
        var RemovalForReasons = await _accountsDbContext.ComplianceSchemeMemberRemovalReasons
            .Where(removalreason => removalreason.IsActive)
            .OrderBy(removalreason => removalreason.DisplayOrder)
            .Select(removalreason => new ComplianceSchemeRemovalReasonResponse(removalreason.Code, removalreason.RequiresReason))
            .ToListAsync();

        return RemovalForReasons;
    }

    private async Task<SelectedScheme?> GetSelectedSchemeAndOrganisationConnectionAsync(Guid selectedSchemeId)
    {
        return await _accountsDbContext.SelectedSchemes
            .Include(x => x.OrganisationConnection)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.ExternalId == selectedSchemeId);
    }

    private async Task RemoveOrganisationsConnectionForSchemeAsync(SelectedScheme selectedScheme)
    {
        var organisationConnection =
            _accountsDbContext.OrganisationsConnections.Single(
                x => !x.IsDeleted &&
                     x.Id == selectedScheme.OrganisationConnectionId);

        organisationConnection!.IsDeleted = true;
    }

    private async Task RemoveOrganisationsConnectionAsync(OrganisationsConnection organisationsConnection)
    {
        organisationsConnection.IsDeleted = true;
    }

    private static async Task RemoveSelectedSchemeAsync(SelectedScheme selectedScheme)
    {
        selectedScheme.IsDeleted = true;
    }

    private async Task<OrganisationsConnection?> FindOrganisationConnectionAsync(Organisation producerOrganisation,
        Organisation operatorOrganisation)
    {
        return await _accountsDbContext.OrganisationsConnections
            .FirstOrDefaultAsync(
                x => !x.IsDeleted &&
                     (x.ToOrganisationId == operatorOrganisation.Id &&
                      x.FromOrganisationId == producerOrganisation.Id) ||
                     (x.ToOrganisationId == producerOrganisation.Id &&
                      x.FromOrganisationId == operatorOrganisation.Id));
    }

    private async Task<Result<SelectedScheme>> SelectComplianceSchemeAsync(Guid complianceSchemeId,
        Guid producerOrganisationId)
    {
        var complianceScheme = _accountsDbContext.ComplianceSchemes
            .FirstOrDefault(x => !x.IsDeleted && x.ExternalId == complianceSchemeId);

        if (complianceScheme == null)
            return Result<SelectedScheme>.FailedResult("Compliance scheme not found", HttpStatusCode.NotFound);

        var operatorOrganisation = await _accountsDbContext.Organisations
            .FirstOrDefaultAsync(x =>
                x.CompaniesHouseNumber == complianceScheme.CompaniesHouseNumber && 
                x.IsComplianceScheme);
        var producerOrganisation = _accountsDbContext.Organisations.FirstOrDefault(x => x.ExternalId == producerOrganisationId);

        if (producerOrganisation == null)
            return Result<SelectedScheme>.FailedResult("Producer organisation not found", HttpStatusCode.NotFound);

        _logger.LogInformation("Adding organisation connection");
        var organisationsConnection =
            await FindOrganisationConnectionAsync(producerOrganisation, operatorOrganisation) ??
            await AddProducerOperatorConnection(producerOrganisation, operatorOrganisation);

        var selectedScheme = await AddSelectedSchemeAsync(complianceScheme, organisationsConnection);

        return Result<SelectedScheme>.SuccessResult(selectedScheme);
    }

    private async Task<SelectedScheme> AddSelectedSchemeAsync(ComplianceScheme complianceScheme, OrganisationsConnection orgConnection)
    {
        var selectedScheme = new SelectedScheme
        {
            ComplianceScheme = complianceScheme,
            OrganisationConnection = orgConnection
        };

        await _accountsDbContext.SelectedSchemes.AddAsync(selectedScheme);

        return selectedScheme;
    }

    private async Task<OrganisationsConnection> AddProducerOperatorConnection(Organisation producerOrganisation,
        Organisation operatorOrganisation)
    {
        var organisationConnection = new OrganisationsConnection
        {
            FromOrganisation = producerOrganisation,
            FromOrganisationRoleId = InterOrganisationRole.Producer,
            ToOrganisation = operatorOrganisation,
            ToOrganisationRoleId = InterOrganisationRole.ComplianceScheme
        };

        await _accountsDbContext.OrganisationsConnections.AddAsync(organisationConnection);
        return organisationConnection;
    }
    
    private async Task<SelectedScheme> GetSelectedSchemeForOrganisation(Guid organisationId)
    {
        return await _accountsDbContext.SelectedSchemes
            .Include(org => org.OrganisationConnection)
            .Include(org => org.OrganisationConnection.ToOrganisation)
            .Include(cs => cs.ComplianceScheme)
            .AsNoTracking()
            .SingleOrDefaultAsync(selectedScheme =>
                (selectedScheme.OrganisationConnection.FromOrganisation.ExternalId == organisationId
                 || selectedScheme.OrganisationConnection.ToOrganisation.ExternalId == organisationId)
                && !selectedScheme.IsDeleted && !selectedScheme.OrganisationConnection.IsDeleted);
    }    
}