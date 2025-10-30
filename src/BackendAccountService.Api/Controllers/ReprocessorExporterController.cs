using BackendAccountService.Api.Configuration;
using BackendAccountService.Api.Filters;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api")]
public class ReprocessorExporterController(IOptions<ApiConfig> baseApiConfigOptions, 
    IReprocessorExporterService reprocessorExporterService
   ,IValidator<PersonsDetailsRequestDto> personsDetailsValidator
    ) : ApiControllerBase(baseApiConfigOptions)
{
    [HttpGet]
    [Route("nations/nation-id/{nationId}")]
    [ProducesResponseType(typeof(NationDetailsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ValidateInRange("nationId", 1, 4)]
    public async Task<IActionResult> GetNationDetailsByNationId(int nationId)
    {
        var nationDetails = await reprocessorExporterService.GetNationDetailsByNationId(nationId);
        return Ok(nationDetails);
    }

    [HttpGet]
    [Route("organisations/organisation-with-persons/{organisationId}")]
    [ProducesResponseType(typeof(OrganisationDetailsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganisationDetailsByOrgId(Guid organisationId)
    {
        var organisationDetails = await reprocessorExporterService.GetOrganisationDetailsByOrgId(organisationId);
        return Ok(organisationDetails);
    }

    [HttpPost]
    [Route("organisations/person-details-by-ids")]
    [ProducesResponseType(typeof(List<OrganisationPersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPersonDetailsByIds([FromBody] PersonsDetailsRequestDto request)
    {
        await personsDetailsValidator.ValidateAndThrowAsync(request);
        var personDetails = await reprocessorExporterService.GetPersonDetailsByIds(request);
        return Ok(personDetails);
    }
}