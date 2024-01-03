using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using BackendAccountService.Api.Configuration;
using Microsoft.Extensions.Options;
using BackendAccountService.Core.Services;
using BackendAccountService.Core.Models.Request.Attributes;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrolmentsController : ApiControllerBase
{
    private readonly IEnrolmentsService _enrolmentsService;
    private readonly ILogger<EnrolmentsController> _logger;
    private readonly IValidationService _validationService;
    
    public EnrolmentsController(IEnrolmentsService enrolmentsService, IOptions<ApiConfig> baseApiConfigOptions,
        ILogger<EnrolmentsController> logger, IValidationService validationService) : base(baseApiConfigOptions)
    {
        _enrolmentsService = enrolmentsService;
        _logger = logger;
        _validationService = validationService;
    }
    
    [HttpDelete("{personExternalId}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveEnrolment(
        [FromRoute, NotDefault] Guid personExternalId,
        [FromQuery, Required, NotDefault] Guid userId, 
        [FromQuery, Required, NotDefault] Guid organisationId,
        [FromQuery, Required, NotDefault] int serviceRoleId)
    {
        if (!_validationService.IsAuthorisedToRemoveEnrolledUser(userId, organisationId, serviceRoleId,
                personExternalId))
        {
            _logger.LogError(
                "User {userId} unauthorised to remove person {personExternalId} on behalf of organisation {organisationId} in service {serviceRoleId}.",
                userId, personExternalId, organisationId, serviceRoleId);
            return Problem(statusCode: StatusCodes.Status403Forbidden, type: "authorisation");
        }

        var succeeded =
            await _enrolmentsService.DeleteEnrolmentsForPersonAsync(userId, personExternalId, organisationId,
                serviceRoleId);
        
        return succeeded
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status500InternalServerError, type: "failed to remove person");
    }
}