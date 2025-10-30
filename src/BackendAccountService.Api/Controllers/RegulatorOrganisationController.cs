using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Helpers;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers
{
    [ApiController]
    [Consumes("application/json")]
    [Route("api/regulator-organisation")]    
    public class RegulatorOrganisationController : ApiControllerBase
    {
        private readonly IRegulatorOrganisationService _regulatorOrganisationService;
        private readonly IRegulatorService _regulatorService;
        private readonly ILogger<RegulatorOrganisationController> _logger;

        public RegulatorOrganisationController(IRegulatorOrganisationService regulatorAccountsService,
            IRegulatorService regulatorService,
            IOptions<ApiConfig> baseApiConfigOptions,
            ILogger<RegulatorOrganisationController> logger) : base(baseApiConfigOptions)
        {
            _regulatorOrganisationService = regulatorAccountsService;
            _regulatorService = regulatorService; 
            _logger = logger;
        }

        [HttpGet(Name = "GetOrganisationIdFromNation")]
        [ProducesResponseType(typeof(CheckOrganisationExistResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetOrganisationIdFromNation([FromQuery] string nation)
        {
            var check = await _regulatorOrganisationService.GetRegulatorOrganisationByNationAsync(nation);

            return Ok(check);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRegulatorOrganisation([FromBody] CreateRegulatorOrganisationRequest request)
        {
            _logger.LogInformation("Creating the selected regulator organisation {Name}", request.Name);

            var result = await _regulatorOrganisationService.CreateNewRegulatorOrganisationAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Created the selected regulator organisation Id: {ExternalId}", result.Value.ExternalId);

                return CreatedAtRoute(nameof(GetOrganisationIdFromNation), new { nation = result.Value.Nation }, null);
            }

            _logger.LogInformation("Failed to create the selected regulator organisation {Name}", request.Name);

            return result.BuildErrorResponse();
        }
        
        [HttpGet]
        [Route("organisation-nation")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNationIdsFromOrganisationId([FromQuery] Guid organisationId)
        {
            _logger.LogInformation("Retrieving the nation Id for the organisation: {OrganisationId}", organisationId);
            
            var response = await _regulatorService.GetOrganisationNationsAsync(organisationId);
            var nationIds = response.Select(nation => nation.Id).ToList();
            
            return Ok(nationIds);
        }
    }
}