using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BackendAccountService.Core.Services;

public class NotificationsService : INotificationsService
{
  
    private readonly AccountsDbContext _accountsDbContext;

    public NotificationsService(AccountsDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }

    public async Task<NotificationsResponse?> GetNotificationsForServiceAsync(Guid userId, Guid organisationId, string serviceKey)
    {
        const string EnrolmentId = nameof(EnrolmentId);

        var notificationsResponse = new NotificationsResponse
        {
            Notifications = new List<Notification>()
        };

        // Delegated person is a packaging service concept
        // therefore we check for delegated person nominated and pending enrolments for packaging serviceKey only
        if (serviceKey.Equals(ServiceKeys.Packaging, StringComparison.CurrentCultureIgnoreCase))
        {
            var nominationEnrolments = await _accountsDbContext.Enrolments
                .WhereUserObjectIdIs(userId)
                .WhereOrganisationIdIs(organisationId)
                .WhereServiceIs(serviceKey)
                .WhereServiceRoleIn(ServiceRole.Packaging.DelegatedPerson.Key, ServiceRole.Packaging.ApprovedPerson.Key)
                .WhereEnrolmentStatusIn(EnrolmentStatus.Nominated, EnrolmentStatus.Pending)
                .Select(enrolment => new Notification
                {
                    Type = enrolment.EnrolmentStatusId == EnrolmentStatus.Nominated ?
                        NotificationTypes.Packaging.DelegatedPersonNomination :
                        NotificationTypes.Packaging.DelegatedPersonPendingApproval,
                    Data = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(EnrolmentId, enrolment.ExternalId.ToString()) }
                }).ToListAsync();

            notificationsResponse.Notifications.AddRange(nominationEnrolments);
        }
        
        return notificationsResponse;
    }
}