using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Models.Mappings;

public static class OrganisationMappings
{
    public static int? GetProducerTypeId(ProducerType? producerType)
    {
        if (producerType == null)
        {
            return null;
        }

        return producerType switch
        {
            ProducerType.NotSet => Data.DbConstants.ProducerType.NotSet,
            ProducerType.Partnership => Data.DbConstants.ProducerType.Partnership,
            ProducerType.UnincorporatedBody => Data.DbConstants.ProducerType.UnincorporatedBody,
            ProducerType.NonUkOrganisation => Data.DbConstants.ProducerType.NonUkOrganisation,
            ProducerType.SoleTrader => Data.DbConstants.ProducerType.SoleTrader,
            ProducerType.Other => Data.DbConstants.ProducerType.Other,
            _ => throw new ArgumentException($"Unrecognised organisation type: '{producerType}'")
        };
    }
    
    public static ProducerType? GetProducerType(int? producerTypeId)
    {
        if (producerTypeId == null)
        {
            return null;
        }

        return producerTypeId switch
        {
            Data.DbConstants.ProducerType.NotSet => ProducerType.NotSet,
            Data.DbConstants.ProducerType.Partnership => ProducerType.Partnership,
            Data.DbConstants.ProducerType.UnincorporatedBody => ProducerType.UnincorporatedBody,
            Data.DbConstants.ProducerType.NonUkOrganisation => ProducerType.NonUkOrganisation,
            Data.DbConstants.ProducerType.SoleTrader => ProducerType.SoleTrader,
            Data.DbConstants.ProducerType.Other => ProducerType.Other,
            _ => throw new ArgumentException($"Unrecognised organisation type ID: '{producerTypeId}'")
        };
    }

    public static OrganisationResponseModel GetOrganisationModelFromOrganisation(Data.Entities.Organisation organisation)
    {

        var organisationModel = new OrganisationResponseModel();
        organisationModel.OrganisationType = OrganisationTypeMappings.GetOrganisationType(organisation.OrganisationTypeId);
        organisationModel.ProducerType = GetProducerType(organisation.ProducerTypeId);
        organisationModel.CompaniesHouseNumber = organisation.CompaniesHouseNumber;
        organisationModel.CreatedOn = organisation.CreatedOn;
        organisationModel.IsComplianceScheme = organisation.IsComplianceScheme;
        organisationModel.ValidatedWithCompaniesHouse = organisation.ValidatedWithCompaniesHouse;
        organisationModel.Name = organisation.Name;
        organisationModel.Address = new AddressModel
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
        };

        if (organisation.NationId != null)
        {
            organisationModel.Nation = NationMappings.GetNation(organisation.NationId.Value);
        }

        return organisationModel;
    }

    public static OrganisationDetailModel GetOrganisationDetailModel(Organisation organisation)
    {
        return new OrganisationDetailModel
        {
            Name = organisation.Name,
            OrganisationNumber = organisation.ReferenceNumber
        };
    }
}
