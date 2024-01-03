using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Core.Models.Mappings;

public static class LaOrganisationMappings
{
    public static LocalAuthorityResponseModel GetLaOrganisationModelFromOrganisation(
         Data.Entities.Organisation organisation, Data.Entities.LaOrganisation laOrganisation)
    {
        var localAuthorityResponseModel = new LocalAuthorityResponseModel
        {
            ExternalId = organisation.ExternalId,
            ModifiedOn = DateTimeOffset.Now,
            DistrictCode = laOrganisation.DistrictCode,
            OrganisationType = OrganisationTypeMappings.GetOrganisationType(organisation.OrganisationTypeId),
            CompaniesHouseNumber = organisation.CompaniesHouseNumber,
            Name = organisation.Name,
            TradingName = organisation.TradingName,
            ReferenceNumber = organisation.ReferenceNumber,
            Address = new AddressModel
            {
                SubBuildingName = organisation.SubBuildingName,
                BuildingName = organisation.BuildingName,
                BuildingNumber = organisation.BuildingNumber,
                Street = organisation.Street,
                Locality = organisation.Locality,
                DependentLocality = organisation.DependentLocality,
                Town = organisation.Town,
                County = organisation.County,
                Postcode = organisation.Postcode,
                Country = organisation.Country
            },
            ValidatedWithCompaniesHouse = organisation.ValidatedWithCompaniesHouse,
            IsComplianceScheme = organisation.IsComplianceScheme
        };

        if (organisation.NationId != null)
        {
            localAuthorityResponseModel.Nation = NationMappings.GetNation(organisation.NationId.Value);
        }

        return localAuthorityResponseModel;
    }
}
