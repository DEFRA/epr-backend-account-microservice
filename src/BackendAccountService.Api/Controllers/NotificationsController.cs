using BackendAccountService.Api.Configuration;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace BackendAccountService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ApiControllerBase
{
    private readonly INotificationsService _notificationsService;

    public NotificationsController(
        INotificationsService notificationsService,
        IOptions<ApiConfig> baseApiConfigOptions)
        : base(baseApiConfigOptions)
    {
        _notificationsService = notificationsService;
    }


    [HttpGet]
    [ProducesResponseType(typeof(NotificationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationsResponse>> GetNotifications(
       [BindRequired, FromQuery] string serviceKey,
       [BindRequired, FromHeader(Name = "X-EPR-User")] Guid userId,
       [BindRequired, FromHeader(Name = "X-EPR-Organisation")] Guid organisationId)
    {
        var response = await _notificationsService.GetNotificationsForServiceAsync(userId, organisationId, serviceKey);

        if (response.Notifications.Count == 0)
        {
            return new NoContentResult();
        }

        return new OkObjectResult(response);
    }
}
