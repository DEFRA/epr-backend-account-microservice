using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Core.Services;

public interface INotificationsService
{
    Task<NotificationsResponse?> GetNotificationsForServiceAsync(Guid userId, Guid organisationId, string serviceKey);
}
