using BackendAccountService.Core.Models;
using BackendAccountService.Data.Infrastructure;

namespace BackendAccountService.Core.Services;

public abstract class ServiceBase(AccountsDbContext _accountsDbContext)
{
    protected (string OrganisationType, bool IsComplianceSchemeMemberSubsidiary) DetermineOrganisationType(bool isComplianceScheme, int companyId)
    {
        if (isComplianceScheme)
        {
            return (OrganisationSchemeType.ComplianceScheme.ToString(), false);
        }

        // Check if the org is a compliance scheme member:
        var checkMatchInOrgConn = _accountsDbContext.OrganisationsConnections
            .FirstOrDefault(x => !x.IsDeleted && x.FromOrganisationId == companyId || x.ToOrganisationId == companyId);

        if (checkMatchInOrgConn is not null)
        {
            // They are a compliance scheme member so they are an Indirect Producer
            return (OrganisationSchemeType.InDirectProducer.ToString(), false);
        }

        // Check if the org is a subsidiary:
        var subsidiaryCheck = _accountsDbContext.OrganisationRelationships
            .OrderBy(x => x.RelationFromDate)
            .FirstOrDefault(x => x.RelationToDate == null && x.SecondOrganisationId == companyId);

        if (subsidiaryCheck is null)
        {
            // They are not a subsidiary so therefore a Direct Producer
            return (OrganisationSchemeType.DirectProducer.ToString(), false);
        }

        // They *are* a subsidiary ... need to check the parent. If the parent is a compliance scheme member, then
        // they are a indirect producer. If the parent is a not a CS member then the org is a direct producer

        var subsidiaryParentId = subsidiaryCheck.FirstOrganisationId;
        checkMatchInOrgConn = _accountsDbContext.OrganisationsConnections
           .FirstOrDefault(x => !x.IsDeleted && x.FromOrganisationId == subsidiaryParentId || x.ToOrganisationId == subsidiaryParentId);

        if (checkMatchInOrgConn is not null)
        {
            // This subsidiary is a compliance scheme member so they are an Indirect Producer
            return (OrganisationSchemeType.InDirectProducer.ToString(), true);
        }

        return (OrganisationSchemeType.DirectProducer.ToString(), false);
    }
}