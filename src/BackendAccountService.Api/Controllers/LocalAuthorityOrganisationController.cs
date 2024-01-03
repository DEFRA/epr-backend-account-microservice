using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Helpers;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace BackendAccountService.Api.Controllers
{
    [ApiController]
    [Route("api/laorganisations")]
    public class LocalAuthorityOrganisationController : ApiControllerBase
    {
        private readonly ILogger<LocalAuthorityOrganisationController> _logger;
        private readonly ILocalAuthorityService _localAuthorityService;

        public LocalAuthorityOrganisationController(
            ILocalAuthorityService localAuthorityService,
            IOptions<ApiConfig> baseApiConfigOptions,
            ILogger<LocalAuthorityOrganisationController> logger)
            : base(baseApiConfigOptions)
        {
            _localAuthorityService = localAuthorityService;
            _logger = logger;
        }

        [HttpGet("retrieve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocalAuthorityOrganisation()
        {
            _logger.LogInformation($"Getting all local authority");
            var result = await _localAuthorityService.GetLocalAuthorityOrganisationAsync();
            
            if (result.Count > 0) return Ok(result);
            
            _logger.LogInformation($"Error getting all local authority");
            return NotFound(result);
        }

        [HttpGet("district-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocalAuthorityOrganisationByDistrictCode(string districtCode)
        {
            _logger.LogInformation($"Getting all local authority with district code {districtCode}");
            if (string.IsNullOrWhiteSpace(districtCode))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            
            var result = await _localAuthorityService.GetLocalAuthorityByDistrictCodeAsync(districtCode);
            
            if (result.IsSuccess) return Ok(result);
            
            _logger.LogInformation($"Error getting all local authority with district code {districtCode}");
            return result.BuildErrorResponse();
        }

        [HttpGet("organisation-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocalAuthorityOrganisationByOrganisationName(string name)
        {
            _logger.LogInformation($"Getting all local authority with name {name}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            
            var result = await _localAuthorityService.GetLocalAuthorityByOrganisationNameAsync(name);
            
            if (result.IsSuccess) return Ok(result);
            
            _logger.LogInformation($"Error getting all local authority with name {name}");
            return result.BuildErrorResponse();
        }

        [HttpGet("organisation-type-id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocalAuthorityOrganisationByOrganisationTypeId(int id)
        {
            _logger.LogInformation($"Getting all local authority with organisation type {id}");
            if (id < 0)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            
            var result = await _localAuthorityService.GetLocalAuthorityOrganisationByOrganisationTypeIdAsync(id);
            
            if (result.Count > 0) return Ok(result);
            
            _logger.LogInformation($"Error getting all local authority with organisation type {id}");
            return NotFound(result);
        }

        [HttpGet("external-id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocalAuthorityOrganisationByExternalId(string id)
        {
            _logger.LogInformation($"Getting all local authority with external id {id}");
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var localAuthorityResult = await _localAuthorityService.GetLocalAuthorityOrganisationByExternalIdAsync(id);
            
            if (localAuthorityResult.IsSuccess) return Ok(localAuthorityResult);
            
            _logger.LogInformation($"Error getting all local authority with external id {id}");
            return NotFound(localAuthorityResult);
        }

        [Route("create")]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNewLocalAuthorityOrganisation(CreateLocalAuthorityRequest request)
        {
            _logger.LogInformation($"Creating the selected local authority {request.DistrictCode}");
            var result = await _localAuthorityService.CreateNewLocalAuthorityOrganisationAsync(request);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation($"Created the selected local authority Id: {request.DistrictCode}");
                return Ok(result);
            }
            
            _logger.LogInformation($"Failed to create the selected local authority {request.DistrictCode}");
            return result.BuildErrorResponse();
        }

        [Route("update/external-id")]
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLocalAuthorityOrganisationByExternalId(
            UpdateLocalAuthorityRequest request)
        {
            _logger.LogInformation($"Updating the selected local authority ExternalId:{request.ExternalId}");
            if (string.IsNullOrWhiteSpace(request.ExternalId) || !Guid.TryParse(request.ExternalId, out _))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var result = await _localAuthorityService.UpdateLocalAuthorityByExternalIdAsync(request);
            
            if (result.IsSuccess) return NoContent();
            
            _logger.LogInformation($"Failed to update the selected local authority ExternalId:{request.ExternalId}");
            return result.BuildErrorResponse();
        }

        [Route("update/district-code")]
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLocalAuthorityOrganisationByDistrictCode(
            UpdateLocalAuthorityRequest request)
        {
            _logger.LogInformation($"Updating the selected local authority DistrictCode:{request.DistrictCode}");
            if (string.IsNullOrWhiteSpace(request.DistrictCode))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var result = await _localAuthorityService.UpdateLocalAuthorityByDistrictCodeAsync(request);
            
            if (result.IsSuccess) return NoContent();
            
            _logger.LogInformation(
                $"Failed to update the selected local authority DistrictCode:{request.DistrictCode}");
            return result.BuildErrorResponse();
        }

        [Route("remove/external-id")]
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveAuthorityOrganisationByExternalId(RemoveLocalAuthorityRequest request)
        {
            _logger.LogInformation($"Removing the selected local authority ExternalId:{request.ExternalId}");
            if (string.IsNullOrWhiteSpace(request.ExternalId) || !Guid.TryParse(request.ExternalId, out _))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var result = await _localAuthorityService.RemoveLocalAuthorityByExternalIdAsync(request);
            
            if (result.IsSuccess) return NoContent();
            
            _logger.LogError($"Failed to remove selected local authority ExternalId:{request.ExternalId}");
            return result.BuildErrorResponse();
        }

        [Route("remove/district-code")]
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveAuthorityOrganisationByDistrictCode(RemoveLocalAuthorityRequest request)
        {
            _logger.LogInformation($"Removing the selected local authority DistrictCode:{request.DistrictCode}");
            if (string.IsNullOrWhiteSpace(request.DistrictCode))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            
            var result = await _localAuthorityService.RemoveLocalAuthorityByDistrictCodeAsync(request);
            
            if (result.IsSuccess) return NoContent();
            
            _logger.LogError($"Failed to remove selected local authority DistrictCode:{request.DistrictCode}");
            return result.BuildErrorResponse();
        }
        
        [HttpGet("nation-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNation(string nationName)
        {
            _logger.LogInformation($"Getting nation with name {nationName}");
            if (string.IsNullOrWhiteSpace(nationName))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            
            var result = await _localAuthorityService.GetLocalAuthorityOrganisationNationAsync(nationName);
            
            if (result.IsSuccess) return Ok(result);
            
            _logger.LogInformation($"Error getting nation with name {nationName}");
            return result.BuildErrorResponse();
        }
        
        [HttpGet("organisation-type-name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OrganisationType(string organisationTypeName)
        {
            _logger.LogInformation($"Getting organisation type with name {organisationTypeName}");
            if (string.IsNullOrWhiteSpace(organisationTypeName))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            
            var result = await _localAuthorityService.GetLocalAuthorityOrganisationByOrganisationTypeNameAsync(organisationTypeName);
            
            if (result.IsSuccess) return Ok(result);
            
            _logger.LogInformation($"Error organisation type with name {organisationTypeName}");
            return result.BuildErrorResponse();
        }
    }
}