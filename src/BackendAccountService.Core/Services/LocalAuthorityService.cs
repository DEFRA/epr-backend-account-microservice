using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

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
                await _accountsDbContext.SaveChangesAsync(request.UserId, Guid.Empty);

                localAuthority.OrganisationId = organisation.Id;
                _accountsDbContext.Add(localAuthority);
                await _accountsDbContext.SaveChangesAsync(request.UserId, Guid.Empty);

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

        public async Task<IList<LocalAuthorityResponseModel>> GetLocalAuthorityOrganisationByOrganisationTypeIdAsync(int id)
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

                _logger.LogError("Selected local authority with external id: {Id}, was not found", id);
                return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with external id: {id}, was not found", HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError("Selected local authority with external id: {Id} was not found, {Message}", id, e.Message);
                return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with external id: {id} was not found, {e.Message}", HttpStatusCode.InternalServerError);
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

                _logger.LogError("Selected local authority with district code: {DistrictCode}, was not found", districtCode);
                return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with district code: {districtCode}, was not found", HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError("Selected local authority with district code: {DistrictCode} was not found, {Message}", districtCode, e.Message);
                return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with district code: {districtCode} was not found, {e.Message}", HttpStatusCode.InternalServerError);
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

                _logger.LogError("Selected local authority with name: {Name}, was not found", name);
                return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with name: {name}, was not found", HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError("Selected local authority with name: {Name} was not found, {Message}", name, e.Message);
                return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with name: {name} was not found, {e.Message}", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityByDistrictCodeAsync(UpdateLocalAuthorityRequest request)
        {
            var organisationResult = await _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => org.a.DistrictCode == request.DistrictCode && !org.o.IsDeleted && !org.a.IsDeleted).FirstOrDefaultAsync();

            if (organisationResult != null)
            {
                return await UpdateLocalAuthorityAsync(organisationResult.o, organisationResult.a, request);
            }

            _logger.LogError("Selected local authority with District Code: {DistrictCode}, was not found", request.DistrictCode);
            return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with District Code: {request.DistrictCode}, was not found", HttpStatusCode.NotFound);
        }

        public async Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityByExternalIdAsync(UpdateLocalAuthorityRequest request)
        {
            var organisationResult = await _accountsDbContext.Organisations
                .Join(_accountsDbContext.LaOrganisations, o => o.Id, a => a.OrganisationId, (o, a) => new { o, a })
                .Where(org => org.o.ExternalId == new Guid(request.ExternalId) && !org.o.IsDeleted && !org.a.IsDeleted)
                .FirstOrDefaultAsync();

            if (organisationResult != null)
            {
                return await UpdateLocalAuthorityAsync(organisationResult.o, organisationResult.a, request);
            }

            _logger.LogError("Selected local authority with External Id: {ExternalId}, was not found", request.ExternalId);
            return Result<LocalAuthorityResponseModel>.FailedResult($"Selected local authority with External Id: {request.ExternalId}, was not found", HttpStatusCode.NotFound);
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

            _logger.LogError("Selected local authority with District Code: {DistrictCode}, was not found", request.DistrictCode);
            return Result.FailedResult($"Selected local authority with District Code: {request.DistrictCode}, was not found", HttpStatusCode.NotFound);
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

            _logger.LogError("Selected local authority with External Id: {ExternalId}, was not found", request.ExternalId);
            return Result.FailedResult($"Selected local authority with External Id: {request.ExternalId}, was not found", HttpStatusCode.NotFound);
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

                _logger.LogError("Nation with name: {NationName}, was not found", nationName);
                return Result.FailedResult($"Nation with name: {nationName}, was not found", HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError("Nation with name: {NationName}, was not found", nationName);
                return Result.FailedResult($"Nation with name: {nationName}, was not found", HttpStatusCode.InternalServerError);
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

                _logger.LogError("Organisation Type with name: {OrganisationTypeName}, was not found", organisationTypeName);
                return Result.FailedResult($"Organisation Type with name: {organisationTypeName}, was not found", HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.LogError("Organisation Type with name: {OrganisationTypeName}, was not found", organisationTypeName);
                return Result.FailedResult($"Organisation Type with name: {organisationTypeName}, was not found", HttpStatusCode.InternalServerError);
            }
        }

        private async Task<Result> RemoveLocalAuthorityAsync(Organisation organisation, LaOrganisation laOrganisation,
            RemoveLocalAuthorityRequest request)
        {
            await RemoveSelectedLocalAuthorityAsync(laOrganisation);
            await RemoveSelectedOrganisationAsync(organisation);
            await _accountsDbContext.SaveChangesAsync(request.UserId, Guid.Empty);
            return Result.SuccessResult();
        }

        private async Task<Result<LocalAuthorityResponseModel>> UpdateLocalAuthorityAsync(
            Organisation organisation, LaOrganisation laOrganisations, UpdateLocalAuthorityRequest request)
        {
            await UpdateSelectedLocalAuthorityAsync(laOrganisations, request);
            await UpdateSelectedOrganisationAsync(organisation, request);
            await _accountsDbContext.SaveChangesAsync(request.UserId, Guid.Empty);

            var response = LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(organisation, laOrganisations);

            return Result<LocalAuthorityResponseModel>.SuccessResult(response);
        }

        private static async Task RemoveSelectedLocalAuthorityAsync(LaOrganisation laOrganisations)
        {
            laOrganisations.IsDeleted = true;
        }

        private static async Task RemoveSelectedOrganisationAsync(Organisation organisation)
        {
            organisation.IsDeleted = true;
        }

        private static async Task UpdateSelectedLocalAuthorityAsync(LaOrganisation laOrganisations, UpdateLocalAuthorityRequest request)
        {
            if (request.DistrictCode != string.Empty && request.DistrictCode.Length > 0)
            {
                laOrganisations.DistrictCode = request.DistrictCode;
            }
        }

        private async Task UpdateSelectedOrganisationAsync(Organisation organisation, UpdateLocalAuthorityRequest request)
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

        private static LaOrganisation BuildLocalAuthority(CreateLocalAuthorityRequest request)
        {
            return new LaOrganisation
            {
                DistrictCode = request.DistrictCode
            };
        }
    }
}