using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Data.IntegrationTests;

public static class DatabaseDataGenerator
{
    public static async Task<Enrolment> InsertRandomEnrolment(AccountsDbContext context, Guid organisationId, string serviceRoleKey, int personInOrganisationRole, int enrolmentStatus, bool isComplianceScheme = false)
    {
        var accountService = new AccountService(context);

        var approvedPersonAccount = RandomModelData.GetAccountModel(serviceRoleKey, isComplianceScheme);
        
        var serviceRole = await accountService.GetServiceRoleAsync(approvedPersonAccount.Connection.ServiceRole);

        Enrolment enrolment = EnrolmentMappings.GetEnrolmentFromAccountModel(approvedPersonAccount, serviceRole.Id);

        var organisation = await context.Organisations.Where(o => o.ExternalId == organisationId).FirstOrDefaultAsync();

        bool isAddToExistingOrganisation = organisation != null;

        if (isAddToExistingOrganisation)
        {
            enrolment.Connection.Organisation = organisation;
        }
        else
        {
            enrolment.Connection.Organisation.ExternalId = organisationId;
        }

        enrolment.Connection.PersonRoleId = personInOrganisationRole;

        enrolment.EnrolmentStatusId = enrolmentStatus;

        context.Enrolments.Add(enrolment);

        await context.SaveChangesAsync(approvedPersonAccount.User.UserId!.Value, organisationId);

        return enrolment;
    }
}