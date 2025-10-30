using BackendAccountService.Data.Infrastructure;
using BackendAccountService.ValidationData.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAccountService.ValidationData.Api.Services;

public class SubsidiaryDataService : ISubsidiaryDataService
{
    private readonly AccountsDbContext _accountsDbContext;

    public SubsidiaryDataService(AccountsDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }

    public async Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest)
    {
        var result = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = subsidiaryDetailsRequest.SubsidiaryOrganisationDetails
        };

        foreach (var organisationDetailsRequest in result.SubsidiaryOrganisationDetails)
        {
            foreach (var subsidiaryDetail in organisationDetailsRequest.SubsidiaryDetails)
            {
                var subsidiary = await _accountsDbContext.Organisations.FirstOrDefaultAsync(x => x.ReferenceNumber == subsidiaryDetail.ReferenceNumber);
                subsidiaryDetail.CompaniesHouseNumber = subsidiary?.CompaniesHouseNumber;

                subsidiaryDetail.SubsidiaryExists = await (
                    from childOrganisation in _accountsDbContext.Organisations
                    join organisationRelationship in _accountsDbContext.OrganisationRelationships
                        on childOrganisation.Id equals organisationRelationship.SecondOrganisationId
                    where childOrganisation.ReferenceNumber == subsidiaryDetail.ReferenceNumber
                    select new
                    {
                        organisationRelationship.Id
                    }).AnyAsync();

                subsidiaryDetail.SubsidiaryBelongsToAnyOtherOrganisation = await (
                    from childOrganisation in _accountsDbContext.Organisations
                    join organisationRelationship in _accountsDbContext.OrganisationRelationships
                        on childOrganisation.Id equals organisationRelationship.SecondOrganisationId
                    join parentOrganisation in _accountsDbContext.Organisations
                        on organisationRelationship.FirstOrganisationId equals parentOrganisation.Id
                    where childOrganisation.ReferenceNumber == subsidiaryDetail.ReferenceNumber
                          && parentOrganisation.ReferenceNumber != organisationDetailsRequest.OrganisationReference
                          && organisationRelationship.RelationToDate == null
                    select new
                    {
                        organisationRelationship.Id
                    }).AnyAsync();

                subsidiaryDetail.SubsidiaryDoesNotBelongToAnyOrganisation = !await (
                    from childOrganisation in _accountsDbContext.Organisations
                    join organisationRelationship in _accountsDbContext.OrganisationRelationships
                        on childOrganisation.Id equals organisationRelationship.SecondOrganisationId
                    where childOrganisation.ReferenceNumber == subsidiaryDetail.ReferenceNumber
                            && organisationRelationship.RelationToDate == null
                    select new
                    {
                        organisationRelationship.Id
                    }).AnyAsync();
            }
        }

        return result;
    }
}