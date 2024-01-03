using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using BackendAccountService.Core.Models.Mappings;
using Microsoft.Data.SqlClient;

namespace BackendAccountService.Core.Services
{
    public class LocalAuthorityService : ILocalAuthorityService
    {
        private readonly AccountsDbContext _accountsDbContext;
        private readonly ILogger<ComplianceSchemeService> _logger;

        public LocalAuthorityService(AccountsDbContext accountsDbContext, ILogger<ComplianceSchemeService> logger)
        {
            _accountsDbContext = accountsDbContext;
            _logger = logger;
        }

        public async Task<Result<LocalAuthorityResponseModel>> CreateNewLocalAuthorityOrganisationAsync(
            CreateLocalAuthorityRequest request)
        {
            try
            {
                var organisation = await BuildNewOrganisation(request);
                var localAuthority = BuildLocalAuthority(request);

                _accountsDbContext.Add(organisation);
                await _accountsDbContext.SaveChangesAsync(request.UserId,Guid.Empty);

                localAuthority.OrganisationId = organisation.Id;
                _accountsDbContext.Add(localAuthority);
                await _accountsDbContext.SaveChangesAsync(request.UserId,Guid.Empty);

                var response = LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(organisation, localAuthority);
                return Result<LocalAuthorityResponseModel>.SuccessResult(response);
            }
            catch (DbUpdateException e)
            {
                if (e.InnerException is SqlException { Number: 2627 or 2601 } innerException)
                {
                    return Result<LocalAuthorityResponseModel>.FailedResult(
                        $"Error inserting new data into the database, SQL Error: {innerException.Number}.",
                        HttpStatusCode.BadRequest);
                }

                return Result<LocalAuthorityResponseModel>.FailedResult(
                    "SQL Error inserting new data into the database.", HttpStatusCode.BadRequest);
            }
            catch
            {
                return Result<LocalAuthorityResponseModel>.FailedResult(
                    "Error creating new Local Authority / Organisation", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<IList<LocalAuthorityResponseModel>> GetLocalAuthorityOrganisationAsync()
        {
            var organisationList = _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => !org.o.IsDeleted && !org.a.IsDeleted);

            var organisationResponseList = new List<LocalAuthorityResponseModel>();

            foreach (var org in organisationList)
            {
                organisationResponseList.Add(LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(org.o, org.a));
            }

            return organisationResponseList;
        }

        public async Task<IList<LocalAuthorityResponseModel>>
            GetLocalAuthorityOrganisationByOrganisationTypeIdAsync(int id)
        {
            var organisationList = _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => org.o.OrganisationTypeId == id && !org.o.IsDeleted && !org.a.IsDeleted);

            var organisationResponseList = new List<LocalAuthorityResponseModel>();

            foreach (var org in organisationList)
            {
                organisationResponseList.Add(LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(org.o, org.a));
            }

            return organisationResponseList;
        }

        public async Task<Result<LocalAuthorityResponseModel>> GetLocalAuthorityOrganisationByExternalIdAsync(string id)
        {
            try
            {
                var organisationResult = await _accountsDbContext.Organisations
                    .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                    .Where(org => org.o.ExternalId == new Guid(id) && !org.o.IsDeleted && !org.a.IsDeleted).FirstOrDefaultAsync();

                if (organisationResult != null)
                {
                    var response = LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(organisationResult.o, organisationResult.a);
                    return Result<LocalAuthorityResponseModel>.SuccessResult(response);
                }

                var message = $"selected local authority with external id: {id}, was not found";
                _logger.LogError(message);
                return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = $"selected local authority with external id: {id} was not found, {e.Message}";
                _logger.LogError(message);
                return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<LocalAuthorityResponseModel>> GetLocalAuthorityByDistrictCodeAsync(string districtCode)
        {
            try
            {
                var organisationResult = await _accountsDbContext.Organisations
                    .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                    .Where(org => org.a.DistrictCode == districtCode && !org.o.IsDeleted && !org.a.IsDeleted).FirstOrDefaultAsync();

                if (organisationResult != null)
                {
                    var response = LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(organisationResult.o, organisationResult.a);
                    return Result<LocalAuthorityResponseModel>.SuccessResult(response);
                }
             
                var message = $"selected local authority with district code: {districtCode}, was not found";
                _logger.LogError(message);
                return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = $"selected local authority with district code: {districtCode} was not found, {e.Message}";
                _logger.LogError(message);
                return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<LocalAuthorityResponseModel>> GetLocalAuthorityByOrganisationNameAsync(string name)
        {
            try
            {
                var organisationResult = await _accountsDbContext.Organisations
                    .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                    .Where(org => org.o.Name == name && !org.o.IsDeleted && !org.a.IsDeleted).FirstOrDefaultAsync();

                if (organisationResult != null)
                {
                    var response =
                        LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(organisationResult.o,
                            organisationResult.a);
                    return Result<LocalAuthorityResponseModel>.SuccessResult(response);
                }

                var message = $"selected local authority with name: {name}, was not found";
                _logger.LogError(message);
                return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = $"selected local authority with name: {name} was not found, {e.Message}";
                _logger.LogError(message);
                return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityByDistrictCodeAsync(
            UpdateLocalAuthorityRequest request)
        {
            var organisationResult = await _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => org.a.DistrictCode == request.DistrictCode && !org.o.IsDeleted && !org.a.IsDeleted).FirstOrDefaultAsync();

            if (organisationResult != null)
            {
                return await UpdateLocalAuthorityAsync(organisationResult.o, organisationResult.a, request);
            }

            var message = $"selected local authority with District Code: {request.DistrictCode}, was not found";
            _logger.LogError(message);
            return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.NotFound);
        }

        public async Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityByExternalIdAsync(
            UpdateLocalAuthorityRequest request)
        {
            var organisationResult = await _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => org.o.ExternalId == new Guid(request.ExternalId) && !org.o.IsDeleted && !org.a.IsDeleted)
                .FirstOrDefaultAsync();

            if (organisationResult != null)
            {
                return await UpdateLocalAuthorityAsync(organisationResult.o, organisationResult.a, request);
            }

            var message = $"selected local authority with External Id: {request.ExternalId}, was not found";
            _logger.LogError(message);
            return Result<LocalAuthorityResponseModel>.FailedResult(message, HttpStatusCode.NotFound);
        }

        public async Task<Result> RemoveLocalAuthorityByDistrictCodeAsync(RemoveLocalAuthorityRequest request)
        {
            var organisationResult = await _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => org.a.DistrictCode == request.DistrictCode && !org.o.IsDeleted && !org.a.IsDeleted)
                .FirstOrDefaultAsync();

            if (organisationResult != null)
            {
                return await RemoveLocalAuthorityAsync(organisationResult.o, organisationResult.a, request);
            }

            var message = $"selected local authority with District Code: {request.DistrictCode}, was not found";
            _logger.LogError(message);
            return Result.FailedResult(message, HttpStatusCode.NotFound);
        }

        public async Task<Result> RemoveLocalAuthorityByExternalIdAsync(RemoveLocalAuthorityRequest request)
        {
            var organisationResult = await _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => org.o.ExternalId == new Guid(request.ExternalId) && !org.o.IsDeleted && !org.a.IsDeleted)
                .FirstOrDefaultAsync();

            if (organisationResult != null)
            {
                return await RemoveLocalAuthorityAsync(organisationResult.o, organisationResult.a, request);
            }

            var message = $"selected local authority with External Id: {request.ExternalId}, was not found";
            _logger.LogError(message);
            return Result.FailedResult(message, HttpStatusCode.NotFound);
        }

        public async Task<Result> GetLocalAuthorityOrganisationNationAsync(string nationName)
        {
            try
            {
                var nationResult = await _accountsDbContext.Nations.Where(n => n.Name == nationName).FirstOrDefaultAsync();

                if (nationResult != null)
                {
                    return Result.SuccessResult();
                }

                var message = $"Nation with name: {nationName}, was not found";
                _logger.LogError(message);
                return Result.FailedResult(message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = $"Nation with name: {nationName}, was not found";
                _logger.LogError(message);
                return Result.FailedResult(message, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result> GetLocalAuthorityOrganisationByOrganisationTypeNameAsync(string organisationTypeName)
        {
            try
            {
                var organisationTypeResult = await _accountsDbContext.OrganisationTypes.Where(t => t.Name == organisationTypeName).FirstOrDefaultAsync();

                if (organisationTypeResult != null)
                {
                    return Result.SuccessResult();
                }

                var message = $"Organisation Type with name: {organisationTypeName}, was not found";
                _logger.LogError(message);
                return Result.FailedResult(message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                var message = $"Organisation Type with name: {organisationTypeName}, was not found";
                _logger.LogError(message);
                return Result.FailedResult(message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<Result> RemoveLocalAuthorityAsync(Organisation organisation, LaOrganisation laOrganisation,
            RemoveLocalAuthorityRequest request)
        {
            await RemoveSelectedLocalAuthorityAsync(laOrganisation);
            await RemoveSelectedOrganisationAsync(organisation);
            await _accountsDbContext.SaveChangesAsync(request.UserId,Guid.Empty);
            return Result.SuccessResult();
        }

        private async Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityAsync(
            Organisation organisation, LaOrganisation laOrganisations, UpdateLocalAuthorityRequest request)
        {
            await UpdateSelectedLocalAuthorityAsync(laOrganisations, request);
            await UpdateSelectedOrganisationAsync(organisation, request);
            await _accountsDbContext.SaveChangesAsync(request.UserId,Guid.Empty);
                
            var response = LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(organisation, laOrganisations);

            return Result<LocalAuthorityResponseModel>.SuccessResult(response);
        }

        private async Task RemoveSelectedLocalAuthorityAsync(LaOrganisation laOrganisations)
        {
            laOrganisations.IsDeleted = true;
        }

        private async Task RemoveSelectedOrganisationAsync(Organisation organisation)
        {
            organisation.IsDeleted = true;
        }

        private async Task UpdateSelectedLocalAuthorityAsync(LaOrganisation laOrganisations,
            UpdateLocalAuthorityRequest request)
        {
            if (request.DistrictCode != string.Empty && request.DistrictCode.Length > 0)
            {
                laOrganisations.DistrictCode = request.DistrictCode;
            }
        }

        private async Task UpdateSelectedOrganisationAsync(Organisation organisation,
            UpdateLocalAuthorityRequest request)
        {
            var nation = await _accountsDbContext.Nations
                .Where(n => n.Name == request.Nation).FirstOrDefaultAsync();
            var organisationType = await _accountsDbContext.OrganisationTypes
                .Where(t => t.Name == request.WasteAuthorityType).FirstOrDefaultAsync();

            nation ??= await _accountsDbContext.Nations
                .Where(n => n.Id == 0).FirstOrDefaultAsync();

            organisationType ??= await _accountsDbContext.OrganisationTypes
                .Where(t => t.Id == 0).FirstOrDefaultAsync();

            if (request.Name != string.Empty && request.Name.Length > 0)
            {
                organisation.Name = request.Name;
            }

            organisation.Nation = nation;
            organisation.OrganisationType = organisationType;
        }

        private async Task<Organisation> BuildNewOrganisation(CreateLocalAuthorityRequest request)
        {
            var nation = await _accountsDbContext.Nations
                .Where(n => n.Name == request.Nation).FirstOrDefaultAsync();
            var organisationType = await _accountsDbContext.OrganisationTypes
                .Where(t => t.Name == request.WasteAuthorityType).FirstOrDefaultAsync();

            nation ??= await _accountsDbContext.Nations
                .Where(n => n.Id == 0).FirstOrDefaultAsync();

            organisationType ??= await _accountsDbContext.OrganisationTypes
                .Where(t => t.Id == 0).FirstOrDefaultAsync();

            return new Organisation
            {
                OrganisationTypeId = organisationType.Id,
                CompaniesHouseNumber = request.CompaniesHouseNumber,
                Name = request.Name,
                TradingName = request.TradingName,
                ReferenceNumber = request.ReferenceNumber,
                BuildingName = request.BuildingName,
                BuildingNumber = request.BuildingNumber,
                Street = request.Street,
                Locality = request.Locality,
                DependentLocality = request.DependentLocality,
                Town = request.Town,
                County = request.County,
                Country = request.Country,
                Postcode = request.Postcode,
                ValidatedWithCompaniesHouse = request.ValidatedWithCompaniesHouse,
                IsComplianceScheme = request.IsComplianceScheme,
                NationId = nation?.Id,
                IsDeleted = false
            };
        }

        private LaOrganisation BuildLocalAuthority(CreateLocalAuthorityRequest request)
        {
            return new LaOrganisation
            {
                DistrictCode = request.DistrictCode
            };
        }
    }
}
