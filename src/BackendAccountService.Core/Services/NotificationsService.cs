using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using EnrolmentStatus = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using ServiceRole = BackendAccountService.Data.DbConstants.ServiceRole;

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
            var enrolmentsWithNotifications = await _accountsDbContext.Enrolments
                .WhereUserObjectIdIs(userId)
                .WhereOrganisationIdIs(organisationId)
                .WhereServiceIs(serviceKey)
                .WhereServiceRoleIn(ServiceRole.Packaging.DelegatedPerson.Key, ServiceRole.Packaging.ApprovedPerson.Key)
                .WhereEnrolmentStatusIn(EnrolmentStatus.Nominated, EnrolmentStatus.Pending)
                .Include(e => e.ServiceRole)
                .ToListAsync();

            var nominationEnrolments = enrolmentsWithNotifications.Select(CreateNotification).ToList();

            notificationsResponse.Notifications.AddRange(nominationEnrolments);
        }

        return notificationsResponse;
    }
    private Notification CreateNotification(Enrolment enrolment)
    {
        const string EnrolmentId = nameof(EnrolmentId);
        var type = string.Empty;

        if (enrolment.ServiceRole.Key == ServiceRole.Packaging.DelegatedPerson.Key)
        {
            type = enrolment.EnrolmentStatusId == EnrolmentStatus.Nominated
               ? NotificationTypes.Packaging.DelegatedPersonNomination
   :            NotificationTypes.Packaging.DelegatedPersonPendingApproval;
        }

        if (enrolment.ServiceRole.Key == ServiceRole.Packaging.ApprovedPerson.Key)
        {
            type = enrolment.EnrolmentStatusId == EnrolmentStatus.Nominated
                ? NotificationTypes.Packaging.ApprovedPersonNomination
    :           NotificationTypes.Packaging.ApprovedPersonPendingApproval;
        }

        var data = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(EnrolmentId, enrolment.ExternalId.ToString())
        };

        return new Notification { Type = type, Data = data };
    }
}