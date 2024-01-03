using BackendAccountService.Data.Entities;

namespace BackendAccountService.Data.Extensions;

public static class EnrolmentQueryableExtensions
{
    private static readonly int[] ProducerOrganisationTypes =
        { DbConstants.OrganisationType.CompaniesHouseCompany, DbConstants.OrganisationType.NonCompaniesHouseCompany };

    private static readonly int RegulatorOrganisationType = DbConstants.OrganisationType.Regulators;

    
    public static IQueryable<Enrolment> WhereConnectionIdIs(this IQueryable<Enrolment> enrolments, Guid connectionId) =>
        enrolments.Where(enrolment => enrolment.Connection.ExternalId == connectionId);

    public static IQueryable<Enrolment> WhereServiceRoleIn(this IQueryable<Enrolment> enrolments, params string[] serviceRoles) =>
        enrolments.Where(enrolment => serviceRoles.Contains(enrolment.ServiceRole.Key));

    public static IQueryable<Enrolment> WhereServiceIs(this IQueryable<Enrolment> enrolments, string service) =>
        enrolments.Where(enrolment => enrolment.ServiceRole.Service.Key == service);

    public static IQueryable<Enrolment> WhereEnrolmentStatusIn(this IQueryable<Enrolment> enrolments, params int[] enrolmentStatuses) =>
        enrolments.Where(enrolment => enrolmentStatuses.Contains(enrolment.EnrolmentStatusId));
    
    public static IQueryable<Enrolment> WhereEnrolmentServiceRoleIn(this IQueryable<Enrolment> enrolments, params int[] enrolmentServiceRoles) =>
        enrolments.Where(enrolment => enrolmentServiceRoles.Contains(enrolment.ServiceRoleId));

    public static IQueryable<Enrolment> WhereOrganisationIsComplianceScheme(this IQueryable<Enrolment> enrolments) =>
        enrolments.Where(enrolment => ProducerOrganisationTypes.Contains(enrolment.Connection.Organisation.OrganisationTypeId) &&
                                      enrolment.Connection.Organisation.IsComplianceScheme);
                                      
    public static IQueryable<Enrolment> WhereOrganisationIsProducer(this IQueryable<Enrolment> enrolments) =>
        enrolments.Where(enrolment => ProducerOrganisationTypes.Contains(enrolment.Connection.Organisation.OrganisationTypeId) &&
                                      !enrolment.Connection.Organisation.IsComplianceScheme);

    public static IQueryable<Enrolment> WhereOrganisationIdIs(this IQueryable<Enrolment> enrolments, Guid organisationId) =>
        enrolments.Where(enrolment => enrolment.Connection.Organisation.ExternalId == organisationId);

    public static IQueryable<Enrolment> WhereUserObjectIdIs(this IQueryable<Enrolment> enrolments, Guid userObjectId) =>
        enrolments.Where(enrolment => enrolment.Connection.Person.User!.UserId == userObjectId);
    
    public static IQueryable<Enrolment> WherePersonIdIs(this IQueryable<Enrolment> enrolments, Guid personId) =>
        enrolments.Where(enrolment => enrolment.Connection.Person.ExternalId == personId);
    
    public static IQueryable<Enrolment> WhereUserServiceIdIs(this IQueryable<Enrolment> enrolments, int serviceId) =>
        enrolments.Where(enrolment => enrolment.ServiceRole.ServiceId == serviceId);

    public static IQueryable<Enrolment> WherePersonRoleIn(this IQueryable<Enrolment> enrolments, params string[] personRoles) =>
        enrolments.Where(enrolment => personRoles.Contains(enrolment.Connection.PersonRole.Name));

    public static Organisation? SelectDistinctSingleOrganisation(this IQueryable<Enrolment> enrolments) =>
        enrolments.Select(enrolment => enrolment.Connection.Organisation).Distinct().SingleOrDefault();

    public static Person? SelectDistinctSinglePerson(this IQueryable<Enrolment> enrolments) =>
        enrolments.Select(enrolment => enrolment.Connection.Person).Distinct().SingleOrDefault();
    
    public static IQueryable<Enrolment> WhereOrganisationIsRegulator(this IQueryable<Enrolment> enrolments) =>
        enrolments.Where(enrolment => enrolment.Connection.Organisation.OrganisationTypeId == RegulatorOrganisationType);
    
    public static IQueryable<Enrolment> WhereNationIs(this IQueryable<Enrolment> enrolments, int nationId) =>
        enrolments.Where(enrolment => enrolment.Connection.Organisation.NationId == nationId);
    
    public static IQueryable<Enrolment> WhereOrganisationIsNotRegulator(this IQueryable<Enrolment> enrolments) =>
        enrolments.Where(enrolment => enrolment.Connection.Organisation.OrganisationTypeId != RegulatorOrganisationType);
    
    public static IQueryable<Enrolment> WhereConnectionPersonRoleIdsIn(this IQueryable<Enrolment> enrolments, params int[] personRoleIds) =>
        enrolments.Where(enrolment => personRoleIds.Contains(enrolment.Connection.PersonRoleId));
}
