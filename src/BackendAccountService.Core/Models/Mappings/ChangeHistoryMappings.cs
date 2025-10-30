using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace BackendAccountService.Core.Models.Mappings;

[ExcludeFromCodeCoverage]
public static class ChangeHistoryMappings
{

    public static ChangeHistoryModel? GetChangeHistoryModelFromChangeHistory(Data.Entities.ChangeHistory? changeHistory, Data.Entities.PersonOrganisationConnection poc, Data.Entities.Person person)
    {
        if (changeHistory == null)
        {
            return null;
        }
        var changeHistoryModel = new ChangeHistoryModel
        {
            Id = changeHistory.Id,
            ApprovedById = changeHistory.ApprovedById,
            ApproverComments = changeHistory.ApproverComments,
            DecisionDate = changeHistory.DecisionDate,
            DeclarationDate = changeHistory.DeclarationDate,
            IsActive = changeHistory.IsActive,
            NewValues = JsonSerializer.Deserialize<UserDetailsChangeModel>(changeHistory.NewValues),
            OldValues = JsonSerializer.Deserialize<UserDetailsChangeModel>(changeHistory.OldValues),
            OrganisationId = changeHistory.OrganisationId,
            PersonId = changeHistory.PersonId,
            CreatedOn = changeHistory.CreatedOn,
            ExternalId = changeHistory.ExternalId,
            LastUpdatedOn = changeHistory.LastUpdatedOn,
            OrganisationName = poc.Organisation.Name,
            CompaniesHouseNumber = poc.Organisation.CompaniesHouseNumber,
            Nation = poc.Organisation.Nation?.Name,
            EmailAddress = person.Email,
            Telephone = person.Telephone,
            OrganisationReferenceNumber = poc.Organisation.ReferenceNumber, 
            OrganisationType = poc.Organisation.OrganisationType?.Name,
            BusinessAddress = new AddressModel
            {
                BuildingName = poc.Organisation.BuildingName,
                BuildingNumber = poc.Organisation.BuildingNumber,
                SubBuildingName = poc.Organisation.SubBuildingName,
                Street   = poc.Organisation.Street,
                County = poc.Organisation.County,
                Postcode = poc.Organisation.Postcode,
                Town = poc.Organisation.Town,
                Country = poc.Organisation.Country,
                Locality = poc.Organisation.Locality 
            },
            ServiceRole = poc.Enrolments.SingleOrDefault(e => e.ConnectionId == poc.Id && !e.IsDeleted)?.ServiceRole?.Name
        };
        return changeHistoryModel;
    }
}
